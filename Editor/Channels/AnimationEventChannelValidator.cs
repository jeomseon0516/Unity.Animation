using System.Collections.Generic;
using System.Linq;
using Jeomseon.Unity.Animation.Channels;
using UnityEditor;
using UnityEngine;

namespace Jeomseon.Unity.Animation.Editor.Channels
{
    using LegacyAnimation = UnityEngine.Animation;

    internal static class AnimationEventChannelValidator
    {
        [MenuItem("Jeomseon/Tool/Animation/Validate Selected Event Channels")]
        private static void ValidateSelection()
        {
            IReadOnlyList<AnimationClip> clips =
                AnimationEventChannelAuthoringWindow.GatherSelectedClips(true);
            ValidationReport report = Validate(clips, Selection.gameObjects, true);
            string message =
                $"Clips: {clips.Count}\n" +
                $"Conversion candidates: {report.LegacyEvents}\n" +
                $"Invalid channel events: {report.InvalidEvents}\n" +
                $"Duplicate channel event groups: {report.DuplicateGroups.Count}\n" +
                $"Missing receivers: {report.MissingReceivers.Count}";
            string details = report.Details.Count == 0
                ? string.Empty
                : $"\n\nDetails:\n- {string.Join("\n- ", report.Details)}";

            if (!report.RequiresReview)
            {
                Debug.Log($"[Animation Event Channel Validation] PASS\n{message}");
            }
            else
            {
                Debug.LogWarning($"[Animation Event Channel Validation] REVIEW REQUIRED\n{message}{details}");
            }
        }

        internal static ValidationReport Validate(
            IReadOnlyList<AnimationClip> clips,
            IReadOnlyList<GameObject> selectedRoots,
            bool includeInactiveChildren)
        {
            int legacyEvents = 0;
            int invalidEvents = 0;
            List<DuplicateEventGroup> duplicateGroups = new();
            List<MissingReceiver> missingReceivers = FindMissingReceivers(
                selectedRoots,
                includeInactiveChildren);

            foreach (AnimationClip clip in clips)
            {
                AnimationEventChannelMigration.ValidationResult result =
                    AnimationEventChannelMigration.Validate(clip);
                legacyEvents += result.LegacyEvents;
                invalidEvents += result.InvalidEvents;
                duplicateGroups.AddRange(FindDuplicateEvents(clip));
            }

            return new ValidationReport(legacyEvents, invalidEvents, duplicateGroups, missingReceivers);
        }

        internal static IReadOnlyList<DuplicateEventGroup> FindDuplicateEvents(AnimationClip clip)
        {
            return AnimationUtility.GetAnimationEvents(clip)
                .Select((animationEvent, index) => (animationEvent, index))
                .Where(item =>
                    item.animationEvent.functionName == AnimationEventReceiver.RelayFunctionName &&
                    item.animationEvent.objectReferenceParameter is AnimationEventChannel)
                .GroupBy(item => (
                    item.animationEvent.time,
                    Channel: (AnimationEventChannel)item.animationEvent.objectReferenceParameter))
                .Where(group => group.Count() > 1)
                .Select(group => new DuplicateEventGroup(
                    clip,
                    group.Key.time,
                    group.Key.Channel,
                    group.Select(item => item.index).ToArray()))
                .ToArray();
        }

        internal static List<MissingReceiver> FindMissingReceivers(
            IReadOnlyList<GameObject> selectedRoots,
            bool includeInactiveChildren)
        {
            List<MissingReceiver> results = new();
            HashSet<GameObject> visited = new();

            foreach (GameObject root in selectedRoots)
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(includeInactiveChildren))
                {
                    GameObject gameObject = transform.gameObject;
                    if (!visited.Add(gameObject) || gameObject.TryGetComponent<AnimationEventReceiver>(out _))
                    {
                        continue;
                    }

                    IReadOnlyList<AnimationClip> clips = GatherPlayedClips(gameObject);
                    if (!clips.Any(ContainsChannelRelayEvent))
                    {
                        continue;
                    }

                    string relativePath = AnimationUtility.CalculateTransformPath(transform, root.transform);
                    string hierarchyPath = string.IsNullOrEmpty(relativePath)
                        ? root.name
                        : $"{root.name}/{relativePath}";
                    results.Add(new MissingReceiver(gameObject, hierarchyPath, clips));
                }
            }

            return results;
        }

        private static IReadOnlyList<AnimationClip> GatherPlayedClips(GameObject gameObject)
        {
            HashSet<AnimationClip> clips = new();
            if (gameObject.TryGetComponent(out Animator animator) && animator.runtimeAnimatorController != null)
            {
                clips.UnionWith(animator.runtimeAnimatorController.animationClips.Where(clip => clip != null));
            }

            if (gameObject.TryGetComponent(out LegacyAnimation legacyAnimation))
            {
                foreach (AnimationState state in legacyAnimation)
                {
                    if (state?.clip != null)
                    {
                        clips.Add(state.clip);
                    }
                }
            }

            return clips.ToArray();
        }

        private static bool ContainsChannelRelayEvent(AnimationClip clip)
        {
            return AnimationUtility.GetAnimationEvents(clip)
                .Any(animationEvent => animationEvent.functionName == AnimationEventReceiver.RelayFunctionName);
        }

        internal sealed class ValidationReport
        {
            internal ValidationReport(
                int legacyEvents,
                int invalidEvents,
                IReadOnlyList<DuplicateEventGroup> duplicateGroups,
                IReadOnlyList<MissingReceiver> missingReceivers)
            {
                LegacyEvents = legacyEvents;
                InvalidEvents = invalidEvents;
                DuplicateGroups = duplicateGroups;
                MissingReceivers = missingReceivers;
                Details = duplicateGroups.Select(group => group.Description)
                    .Concat(missingReceivers.Select(receiver => receiver.Description))
                    .ToArray();
            }

            internal int LegacyEvents { get; }
            internal int InvalidEvents { get; }
            internal IReadOnlyList<DuplicateEventGroup> DuplicateGroups { get; }
            internal IReadOnlyList<MissingReceiver> MissingReceivers { get; }
            internal IReadOnlyList<string> Details { get; }
            internal bool RequiresReview =>
                LegacyEvents > 0 || InvalidEvents > 0 ||
                DuplicateGroups.Count > 0 || MissingReceivers.Count > 0;
        }

        internal sealed class DuplicateEventGroup
        {
            internal DuplicateEventGroup(
                AnimationClip clip,
                float time,
                AnimationEventChannel channel,
                IReadOnlyList<int> eventIndices)
            {
                Clip = clip;
                Time = time;
                Channel = channel;
                EventIndices = eventIndices;
            }

            internal AnimationClip Clip { get; }
            internal float Time { get; }
            internal AnimationEventChannel Channel { get; }
            internal IReadOnlyList<int> EventIndices { get; }
            internal string Description =>
                $"Duplicate channel events: clip '{Clip.name}', time {Time:0.###}s, " +
                $"channel '{Channel.name}', indices [{string.Join(", ", EventIndices)}]";
        }

        internal sealed class MissingReceiver
        {
            internal MissingReceiver(
                GameObject gameObject,
                string hierarchyPath,
                IReadOnlyList<AnimationClip> clips)
            {
                GameObject = gameObject;
                HierarchyPath = hierarchyPath;
                Clips = clips;
            }

            internal GameObject GameObject { get; }
            internal string HierarchyPath { get; }
            internal IReadOnlyList<AnimationClip> Clips { get; }
            internal string Description =>
                $"Missing AnimationEventReceiver: GameObject '{HierarchyPath}', " +
                $"clips [{string.Join(", ", Clips.Select(clip => clip.name))}]";
        }
    }
}

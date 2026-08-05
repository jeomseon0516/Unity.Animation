using System;
using Jeomseon.Animation.Channels;
using UnityEditor;
using UnityEngine;

namespace Jeomseon.Animation.Editor.Channels
{
    internal static class AnimationEventChannelMigration
    {
        internal static MigrationResult Migrate(
            AnimationClip clip,
            Func<string, AnimationEventChannel> resolveChannel)
        {
            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
            int changedEvents = 0;
            int invalidEvents = 0;

            for (int index = 0; index < events.Length; index++)
            {
                AnimationEvent animationEvent = events[index];

                if (animationEvent.functionName == AnimationEventReceiver.RelayFunctionName)
                {
                    if (animationEvent.objectReferenceParameter is not AnimationEventChannel)
                    {
                        invalidEvents++;
                    }

                    continue;
                }

                if (string.IsNullOrWhiteSpace(animationEvent.functionName))
                {
                    invalidEvents++;
                    continue;
                }

                AnimationEventChannel channel = resolveChannel(animationEvent.functionName);
                if (channel == null)
                {
                    invalidEvents++;
                    continue;
                }

                animationEvent.functionName = AnimationEventReceiver.RelayFunctionName;
                animationEvent.objectReferenceParameter = channel;
                events[index] = animationEvent;
                changedEvents++;
            }

            if (changedEvents > 0)
            {
                Undo.RecordObject(clip, "Convert Animation Events To Channels");
                AnimationUtility.SetAnimationEvents(clip, events);
                EditorUtility.SetDirty(clip);
            }

            return new MigrationResult(changedEvents, invalidEvents);
        }

        internal static ValidationResult Validate(AnimationClip clip)
        {
            int legacyEvents = 0;
            int invalidEvents = 0;

            foreach (AnimationEvent animationEvent in AnimationUtility.GetAnimationEvents(clip))
            {
                if (string.IsNullOrWhiteSpace(animationEvent.functionName))
                {
                    invalidEvents++;
                }
                else if (animationEvent.functionName != AnimationEventReceiver.RelayFunctionName)
                {
                    legacyEvents++;
                }
                else if (animationEvent.objectReferenceParameter is not AnimationEventChannel)
                {
                    invalidEvents++;
                }
            }

            return new ValidationResult(legacyEvents, invalidEvents);
        }

        internal readonly struct MigrationResult
        {
            internal MigrationResult(int changedEvents, int invalidEvents)
            {
                ChangedEvents = changedEvents;
                InvalidEvents = invalidEvents;
            }

            internal int ChangedEvents { get; }
            internal int InvalidEvents { get; }
        }

        internal readonly struct ValidationResult
        {
            internal ValidationResult(int legacyEvents, int invalidEvents)
            {
                LegacyEvents = legacyEvents;
                InvalidEvents = invalidEvents;
            }

            internal int LegacyEvents { get; }
            internal int InvalidEvents { get; }
        }
    }
}

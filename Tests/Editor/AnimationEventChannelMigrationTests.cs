using Jeomseon.Animation.Channels;
using Jeomseon.Animation.Editor.Channels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Jeomseon.Animation.Editor.Tests
{
    public sealed class AnimationEventChannelMigrationTests
    {
        private const string TestFolder = "Assets/__JeomseonAnimationTests";

        [SetUp]
        public void SetUp()
        {
            AnimationEventChannelAuthoringWindow.EnsureFolderExists(TestFolder);
        }

        [TearDown]
        public void TearDown()
        {
            Undo.ClearAll();
            AssetDatabase.DeleteAsset(TestFolder);
            AssetDatabase.Refresh();
        }

        [Test]
        public void LoadOrCreateChannel_ReusesAssetForSameFunctionName()
        {
            AnimationEventChannel first =
                AnimationEventChannelAuthoringWindow.LoadOrCreateChannel(TestFolder, "Footstep");
            AnimationEventChannel second =
                AnimationEventChannelAuthoringWindow.LoadOrCreateChannel(TestFolder, "Footstep");

            Assert.That(second, Is.SameAs(first));
            Assert.That(AssetDatabase.GetAssetPath(first), Is.EqualTo(
                $"{TestFolder}/FootstepAnimationEventChannel.asset"));
        }

        [Test]
        public void Migrate_ReplacesLegacyFunctionsAndPreservesPayloads()
        {
            AnimationClip clip = CreateClipAsset("MultipleEvents");
            AnimationUtility.SetAnimationEvents(clip, new[]
            {
                new AnimationEvent
                {
                    time = 0.1f,
                    functionName = "Footstep",
                    stringParameter = "Left",
                    intParameter = 1
                },
                new AnimationEvent
                {
                    time = 0.2f,
                    functionName = "Footstep",
                    stringParameter = "Right",
                    intParameter = 2
                }
            });

            AnimationEventChannelMigration.MigrationResult result = AnimationEventChannelMigration.Migrate(
                clip,
                functionName => AnimationEventChannelAuthoringWindow.LoadOrCreateChannel(TestFolder, functionName));
            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);

            Assert.That(result.ChangedEvents, Is.EqualTo(2));
            Assert.That(result.InvalidEvents, Is.Zero);
            Assert.That(events[0].functionName, Is.EqualTo(AnimationEventReceiver.RelayFunctionName));
            Assert.That(events[1].functionName, Is.EqualTo(AnimationEventReceiver.RelayFunctionName));
            Assert.That(events[0].objectReferenceParameter, Is.SameAs(events[1].objectReferenceParameter));
            Assert.That(events[0].stringParameter, Is.EqualTo("Left"));
            Assert.That(events[1].intParameter, Is.EqualTo(2));
        }

        [Test]
        public void Migrate_CanBeUndone()
        {
            AnimationClip clip = CreateClipAsset("Undo");
            AnimationUtility.SetAnimationEvents(clip, new[]
            {
                new AnimationEvent { time = 0.5f, functionName = "Impact" }
            });

            AnimationEventChannelMigration.Migrate(
                clip,
                functionName => AnimationEventChannelAuthoringWindow.LoadOrCreateChannel(TestFolder, functionName));
            Undo.PerformUndo();

            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
            Assert.That(events, Has.Length.EqualTo(1));
            Assert.That(events[0].functionName, Is.EqualTo("Impact"));
            Assert.That(events[0].objectReferenceParameter, Is.Null);
        }

        [Test]
        public void ConvertClips_ConvertsMultipleClipsAndReusesChannelsByFunctionName()
        {
            AnimationClip firstClip = CreateClipAsset("First");
            AnimationClip secondClip = CreateClipAsset("Second");
            AnimationUtility.SetAnimationEvents(firstClip, new[]
            {
                new AnimationEvent { functionName = "Footstep" }
            });
            AnimationUtility.SetAnimationEvents(secondClip, new[]
            {
                new AnimationEvent { functionName = "Footstep" },
                new AnimationEvent { functionName = "Impact" }
            });

            AnimationEventChannelAuthoringWindow.BatchMigrationResult result =
                AnimationEventChannelAuthoringWindow.ConvertClips(
                    new[] { firstClip, secondClip },
                    _ => true,
                    functionName => AnimationEventChannelAuthoringWindow.LoadOrCreateChannel(
                        TestFolder,
                        functionName));
            AnimationEvent[] firstEvents = AnimationUtility.GetAnimationEvents(firstClip);
            AnimationEvent[] secondEvents = AnimationUtility.GetAnimationEvents(secondClip);

            Assert.That(result.ChangedClips, Is.EqualTo(2));
            Assert.That(result.ChangedEvents, Is.EqualTo(3));
            Assert.That(result.InvalidEvents, Is.Zero);
            Assert.That(result.SkippedClips, Is.Zero);
            Assert.That(firstEvents[0].objectReferenceParameter, Is.SameAs(secondEvents[0].objectReferenceParameter));
            Assert.That(secondEvents[1].objectReferenceParameter, Is.Not.SameAs(secondEvents[0].objectReferenceParameter));

            Undo.PerformUndo();

            Assert.That(AnimationUtility.GetAnimationEvents(firstClip)[0].functionName, Is.EqualTo("Footstep"));
            Assert.That(AnimationUtility.GetAnimationEvents(secondClip)[0].functionName, Is.EqualTo("Footstep"));
            Assert.That(AnimationUtility.GetAnimationEvents(secondClip)[1].functionName, Is.EqualTo("Impact"));
        }

        [Test]
        public void ConvertClips_SkipsClipsThatCannotBeEdited()
        {
            AnimationClip editableClip = CreateClipAsset("Editable");
            AnimationClip readOnlyClip = CreateClipAsset("ReadOnly");
            AnimationUtility.SetAnimationEvents(editableClip, new[]
            {
                new AnimationEvent { functionName = "EditableEvent" }
            });
            AnimationUtility.SetAnimationEvents(readOnlyClip, new[]
            {
                new AnimationEvent { functionName = "ReadOnlyEvent" }
            });

            AnimationEventChannelAuthoringWindow.BatchMigrationResult result =
                AnimationEventChannelAuthoringWindow.ConvertClips(
                    new[] { editableClip, readOnlyClip },
                    clip => clip != readOnlyClip,
                    functionName => AnimationEventChannelAuthoringWindow.LoadOrCreateChannel(
                        TestFolder,
                        functionName));

            Assert.That(result.ChangedClips, Is.EqualTo(1));
            Assert.That(result.ChangedEvents, Is.EqualTo(1));
            Assert.That(result.SkippedClips, Is.EqualTo(1));
            Assert.That(AnimationUtility.GetAnimationEvents(editableClip)[0].functionName,
                Is.EqualTo(AnimationEventReceiver.RelayFunctionName));
            Assert.That(AnimationUtility.GetAnimationEvents(readOnlyClip)[0].functionName,
                Is.EqualTo("ReadOnlyEvent"));
            Assert.That(AssetDatabase.LoadAssetAtPath<AnimationEventChannel>(
                $"{TestFolder}/ReadOnlyEventAnimationEventChannel.asset"), Is.Null);
        }

        [Test]
        public void Validate_SeparatesLegacyAndInvalidEvents()
        {
            AnimationClip clip = CreateClipAsset("Validation");
            AnimationEventChannel channel = ScriptableObject.CreateInstance<AnimationEventChannel>();
            AnimationUtility.SetAnimationEvents(clip, new[]
            {
                new AnimationEvent { functionName = "Footstep" },
                new AnimationEvent { functionName = AnimationEventReceiver.RelayFunctionName },
                new AnimationEvent { functionName = string.Empty },
                new AnimationEvent
                {
                    functionName = AnimationEventReceiver.RelayFunctionName,
                    objectReferenceParameter = channel
                }
            });

            AnimationEventChannelMigration.ValidationResult result = AnimationEventChannelMigration.Validate(clip);

            Assert.That(result.LegacyEvents, Is.EqualTo(1));
            Assert.That(result.InvalidEvents, Is.EqualTo(2));
            Object.DestroyImmediate(channel);
        }

        private static AnimationClip CreateClipAsset(string name)
        {
            AnimationClip clip = new() { name = name };
            AssetDatabase.CreateAsset(clip, $"{TestFolder}/{name}.anim");
            return clip;
        }
    }
}

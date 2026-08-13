using Jeomseon.Unity.Animation.Channels;
using Jeomseon.Unity.Animation.Editor.Channels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Jeomseon.Animation.Editor.Tests
{
    using LegacyAnimation = UnityEngine.Animation;

    public sealed class AnimationEventChannelValidatorTests
    {
        private GameObject _root;
        private AnimationEventChannel _channel;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root");
            _channel = ScriptableObject.CreateInstance<AnimationEventChannel>();
            _channel.name = "Footstep";
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_channel);
        }

        [Test]
        public void FindDuplicateEvents_ReportsSameTimeAndChannelWithIndices()
        {
            AnimationClip clip = new() { name = "Walk" };
            AnimationUtility.SetAnimationEvents(clip, new[]
            {
                CreateChannelEvent(0.1f, _channel),
                CreateChannelEvent(0.1f, _channel),
                CreateChannelEvent(0.2f, _channel)
            });

            var groups = AnimationEventChannelValidator.FindDuplicateEvents(clip);

            Assert.That(groups.Count, Is.EqualTo(1));
            Assert.That(groups[0].Clip, Is.SameAs(clip));
            Assert.That(groups[0].Time, Is.EqualTo(0.1f));
            Assert.That(groups[0].Channel, Is.SameAs(_channel));
            Assert.That(groups[0].EventIndices, Is.EqualTo(new[] { 0, 1 }));
            Assert.That(groups[0].Description, Does.Contain("Walk"));
            Assert.That(groups[0].Description, Does.Contain("0.1s"));
        }

        [Test]
        public void FindDuplicateEvents_DoesNotCombineDifferentChannels()
        {
            AnimationEventChannel otherChannel = ScriptableObject.CreateInstance<AnimationEventChannel>();
            AnimationClip clip = new();
            AnimationUtility.SetAnimationEvents(clip, new[]
            {
                CreateChannelEvent(0.1f, _channel),
                CreateChannelEvent(0.1f, otherChannel)
            });

            var groups = AnimationEventChannelValidator.FindDuplicateEvents(clip);

            Assert.That(groups, Is.Empty);
            Object.DestroyImmediate(otherChannel);
        }

        [Test]
        public void FindMissingReceivers_ReportsExactHierarchyPathForChannelEmitter()
        {
            GameObject child = new("AnimatedChild");
            child.transform.SetParent(_root.transform);
            LegacyAnimation animation = child.AddComponent<LegacyAnimation>();
            AnimationClip clip = new() { name = "Walk", legacy = true };
            AnimationUtility.SetAnimationEvents(clip, new[] { CreateChannelEvent(0.1f, _channel) });
            animation.AddClip(clip, clip.name);

            var results = AnimationEventChannelValidator.FindMissingReceivers(
                new[] { _root },
                true);

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].GameObject, Is.SameAs(child));
            Assert.That(results[0].HierarchyPath, Is.EqualTo("Root/AnimatedChild"));
            Assert.That(results[0].Description, Does.Contain("Walk"));
        }

        [Test]
        public void FindMissingReceivers_IgnoresEmitterWithReceiver()
        {
            LegacyAnimation animation = _root.AddComponent<LegacyAnimation>();
            _root.AddComponent<AnimationEventReceiver>();
            AnimationClip clip = new() { legacy = true };
            AnimationUtility.SetAnimationEvents(clip, new[] { CreateChannelEvent(0.1f, _channel) });
            animation.AddClip(clip, "Validation");

            var results = AnimationEventChannelValidator.FindMissingReceivers(
                new[] { _root },
                true);

            Assert.That(results, Is.Empty);
        }

        private static AnimationEvent CreateChannelEvent(float time, AnimationEventChannel channel)
        {
            return new AnimationEvent
            {
                time = time,
                functionName = nameof(AnimationEventReceiver.ReceiveAnimationEvent),
                objectReferenceParameter = channel
            };
        }
    }
}

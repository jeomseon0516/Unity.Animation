using System.Collections;
using Jeomseon.Unity.Animation.Channels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Jeomseon.Animation.Tests
{
    public sealed class AnimationEventReceiverTests
    {
        [Test]
        public void ReceiveAnimationEvent_RaisesReferencedChannel()
        {
            GameObject gameObject = new("Animation Event Receiver Test");
            AnimationEventChannel channel = ScriptableObject.CreateInstance<AnimationEventChannel>();
            AnimationEventReceiver receiver = gameObject.AddComponent<AnimationEventReceiver>();
            AnimationEvent animationEvent = new() { objectReferenceParameter = channel };
            AnimationEvent received = null;
            channel.Raised += value => received = value;

            receiver.ReceiveAnimationEvent(animationEvent);

            Assert.That(received, Is.SameAs(animationEvent));
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(channel);
        }

        [UnityTest]
        public IEnumerator AnimationPlayback_DeliversConfiguredEventToChannel()
        {
            var gameObject = new GameObject("Animation Event Playback Test");
            var animation = gameObject.AddComponent<UnityEngine.Animation>();
            gameObject.AddComponent<AnimationEventReceiver>();
            AnimationEventChannel channel = ScriptableObject.CreateInstance<AnimationEventChannel>();
            var clip = new AnimationClip { legacy = true };
            clip.SetCurve(
                string.Empty,
                typeof(Transform),
                "m_LocalPosition.x",
                AnimationCurve.Linear(0f, 0f, 0.1f, 1f));
            clip.AddEvent(new AnimationEvent
            {
                functionName = "ReceiveAnimationEvent",
                time = 0.05f,
                objectReferenceParameter = channel
            });
            animation.AddClip(clip, "Event Test");
            AnimationEvent received = null;
            channel.Raised += value => received = value;

            Assert.That(animation.Play("Event Test"), Is.True);

            yield return new WaitForSeconds(0.15f);

            Assert.That(received, Is.Not.Null);
            Assert.That(received.objectReferenceParameter, Is.SameAs(channel));

            Object.Destroy(gameObject);
            Object.Destroy(channel);
            Object.Destroy(clip);
            yield return null;
        }

        [Test]
        public void ReceiveAnimationEvent_WithoutChannel_LogsWarningAndReturns()
        {
            GameObject gameObject = new("Animation Event Receiver Test");
            AnimationEventReceiver receiver = gameObject.AddComponent<AnimationEventReceiver>();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("AnimationEventChannel"));

            receiver.ReceiveAnimationEvent(new AnimationEvent());

            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void ReceiveAnimationEvent_AllowsTypedChannelSubclassToTransformPayload()
        {
            GameObject gameObject = new("Typed Animation Event Receiver Test");
            IntAnimationEventChannel channel = ScriptableObject.CreateInstance<IntAnimationEventChannel>();
            AnimationEventReceiver receiver = gameObject.AddComponent<AnimationEventReceiver>();
            int receivedValue = 0;
            channel.ValueRaised += value => receivedValue = value;

            receiver.ReceiveAnimationEvent(new AnimationEvent
            {
                objectReferenceParameter = channel,
                intParameter = 42
            });

            Assert.That(receivedValue, Is.EqualTo(42));
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(channel);
        }

        public sealed class IntAnimationEventChannel : AnimationEventChannel
        {
            public event System.Action<int> ValueRaised;

            protected override void OnRaised(AnimationEvent animationEvent)
            {
                base.OnRaised(animationEvent);
                ValueRaised?.Invoke(animationEvent.intParameter);
            }
        }
    }
}

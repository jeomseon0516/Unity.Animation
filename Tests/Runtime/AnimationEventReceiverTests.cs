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

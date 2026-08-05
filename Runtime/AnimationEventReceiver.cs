using Jeomseon.Animation.Channels;
using UnityEngine;

namespace Jeomseon.Animation
{
    /// <summary>
    /// Relays Unity Animation Events to the channel stored in their object parameter.
    /// Unity Animation Events require a method name, so editor tooling owns that convention.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AnimationEventReceiver : MonoBehaviour
    {
        internal const string RelayFunctionName = nameof(ReceiveAnimationEvent);

        /// <summary>
        /// Called by Unity Animation Events configured by the channel authoring tool.
        /// Do not call or configure this method manually in normal usage.
        /// </summary>
        /// <param name="animationEvent">The Animation Event emitted by Unity.</param>
        public void ReceiveAnimationEvent(AnimationEvent animationEvent)
        {
            if (animationEvent.objectReferenceParameter is not AnimationEventChannel channel)
            {
                Debug.LogWarning(
                    $"[{nameof(AnimationEventReceiver)}] Animation Event에 " +
                    $"{nameof(AnimationEventChannel)}이 연결되어 있지 않습니다.",
                    this);
                return;
            }

            channel.Raise(animationEvent);
        }
    }
}

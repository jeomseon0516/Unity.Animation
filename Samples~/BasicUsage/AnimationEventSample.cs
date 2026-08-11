using Jeomseon.Animation;
using Jeomseon.Animation.Channels;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jeomseon.Samples.Animation
{
    /// <summary>
    /// Shows the code-driven subscription workflow for an Animation Event channel.
    /// Animation Event 채널을 코드로 구독하는 사용 예제입니다.
    /// </summary>
    [RequireComponent(typeof(AnimationEventReceiver))]
    public sealed class AnimationEventSample : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("_footstepChannel")] private AnimationEventChannel footstepChannel;

        private void OnEnable()
        {
            if (footstepChannel != null)
            {
                footstepChannel.Raised += OnFootstep;
            }
        }

        private void OnDisable()
        {
            if (footstepChannel != null)
            {
                footstepChannel.Raised -= OnFootstep;
            }
        }

        private static void OnFootstep(AnimationEvent animationEvent)
        {
            Debug.Log($"Animation Event 수신: {animationEvent.time}");
        }
    }
}

using Jeomseon.Animation.Channels;
using UnityEngine;

namespace Jeomseon.Samples.Animation
{
    /// <summary>
    /// Shows the code-driven subscription workflow for an Animation Event channel.
    /// Animation Event 채널을 코드로 구독하는 사용 예제입니다.
    /// </summary>
    [RequireComponent(typeof(AnimationEventReceiver))]
    public sealed class AnimationEventSample : MonoBehaviour
    {
        [SerializeField] private AnimationEventChannel _footstepChannel;

        private void OnEnable()
        {
            if (_footstepChannel != null)
            {
                _footstepChannel.Raised += OnFootstep;
            }
        }

        private void OnDisable()
        {
            if (_footstepChannel != null)
            {
                _footstepChannel.Raised -= OnFootstep;
            }
        }

        private static void OnFootstep(AnimationEvent animationEvent)
        {
            Debug.Log($"Animation Event 수신: {animationEvent.time}");
        }
    }
}

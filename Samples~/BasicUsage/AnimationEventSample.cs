using Jeomseon.Animation;
using UnityEngine;

namespace Jeomseon.Samples.Animation
{
    [RequireComponent(typeof(AnimationEventReceiver))]
    public sealed class AnimationEventSample : MonoBehaviour
    {
        private void Awake()
        {
            AnimationEventReceiver receiver = GetComponent<AnimationEventReceiver>();
            receiver.Register("Footstep", OnFootstep);
        }

        private static void OnFootstep(AnimationEvent animationEvent)
        {
            Debug.Log($"Animation Event 수신: {animationEvent.stringParameter}");
        }
    }
}

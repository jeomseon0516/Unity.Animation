using UnityEngine;
using UnityEngine.Events;

namespace Jeomseon.Animation.Channels
{
    /// <summary>
    /// Connects an Animation Event channel to an Inspector-configurable UnityEvent.
    /// Animation Event 채널을 Inspector에서 설정 가능한 UnityEvent에 연결합니다.
    /// </summary>
    [AddComponentMenu("Jeomseon/Animation/Animation Event Channel Listener")]
    public sealed class AnimationEventChannelListener : MonoBehaviour
    {
        [SerializeField] private AnimationEventChannel _channel;
        [SerializeField] private UnityEvent<AnimationEvent> _response = new();

        /// <summary>
        /// Gets or changes the observed channel while preserving subscription state.
        /// 구독 상태를 유지하면서 감시할 채널을 조회하거나 변경합니다.
        /// </summary>
        public AnimationEventChannel Channel
        {
            get => _channel;
            set
            {
                if (_channel == value)
                {
                    return;
                }

                if (isActiveAndEnabled && _channel != null)
                {
                    _channel.Raised -= OnRaised;
                }

                _channel = value;

                if (isActiveAndEnabled && _channel != null)
                {
                    _channel.Raised += OnRaised;
                }
            }
        }

        /// <summary>
        /// Raised when the configured channel receives an Animation Event.
        /// 설정된 채널이 Animation Event를 수신하면 호출됩니다.
        /// </summary>
        public event UnityAction<AnimationEvent> Responded
        {
            add => _response.AddListener(value);
            remove => _response.RemoveListener(value);
        }

        private void OnEnable()
        {
            if (_channel != null)
            {
                _channel.Raised += OnRaised;
            }
        }

        private void OnDisable()
        {
            if (_channel != null)
            {
                _channel.Raised -= OnRaised;
            }
        }

        private void OnRaised(AnimationEvent animationEvent)
        {
            _response.Invoke(animationEvent);
        }
    }
}

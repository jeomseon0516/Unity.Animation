using System;
using UnityEngine;

namespace Jeomseon.Unity.Animation.Channels
{
    /// <summary>
    /// Identifies an Animation Event and broadcasts it without a string routing key.
    /// 문자열 라우팅 키 없이 Animation Event를 식별하고 전달하는 채널입니다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AnimationEventChannel",
        menuName = "Jeomseon/Animation/Event Channel")]
    public class AnimationEventChannel : ScriptableObject
    {
        /// <summary>
        /// Raised when an Animation Event referencing this channel is received.
        /// 이 채널을 참조하는 Animation Event가 수신되면 호출됩니다.
        /// </summary>
        public event Action<AnimationEvent> Raised;

        internal void Raise(AnimationEvent animationEvent)
        {
            OnRaised(animationEvent);
        }

        /// <summary>
        /// Broadcasts the event and provides an extension point for typed channel subclasses.
        /// 이벤트를 전달하고 타입 채널 파생 클래스가 payload를 변환할 수 있는 확장 지점을 제공합니다.
        /// </summary>
        /// <param name="animationEvent">The Animation Event emitted by Unity.</param>
        protected virtual void OnRaised(AnimationEvent animationEvent)
        {
            Raised?.Invoke(animationEvent);
        }
    }
}

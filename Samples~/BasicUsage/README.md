# Animation Event Channel 기본 예제

`AnimationEventSample.unity` Scene에 `AnimationEventReceiver`와 `AnimationEventSample`이 이미
부착된 GameObject가 포함돼 있습니다.

## 확인 절차

1. `AnimationEventSample.unity`를 엽니다.
2. `Tool > Animation > Event Channel Authoring`을 엽니다.
3. AnimationClip, AnimatorController 또는 Animator가 있는 GameObject를 선택합니다.
4. 변환 버튼을 누르면 기존 Animation Event 함수명별 채널 에셋이 생성되고 자동 연결됩니다.
5. Scene의 `Animation Event Sample` GameObject에 위에서 생성한 채널을 재생하려는 Animator·Animation
   Clip 흐름에 연결하고, `AnimationEventSample`의 채널 필드에 생성된 채널을 지정합니다. 채널 에셋은
   Authoring 도구가 프로젝트별로 생성하므로 Sample에 미리 채워둘 수 없습니다.
6. Play Mode에서 애니메이션을 재생해 Console에 `Animation Event 수신: ...` 로그가 출력되는지
   확인합니다.

코드 없이 연결하려면 `AnimationEventChannelListener`를 추가하고 Channel과 Response를 Inspector에서 설정합니다.
사용자는 Animation Event의 내부 릴레이 함수명이나 Object Parameter 규약을 직접 관리하지 않습니다.

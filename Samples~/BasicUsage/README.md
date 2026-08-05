# Animation Event Channel 기본 예제

1. `Tool > Animation > Event Channel Authoring`을 엽니다.
2. AnimationClip, AnimatorController 또는 Animator가 있는 GameObject를 선택합니다.
3. 변환 버튼을 누르면 기존 Animation Event 함수명별 채널 에셋이 생성되고 자동 연결됩니다.
4. Animator가 있는 GameObject에 `AnimationEventReceiver`를 추가합니다.
5. `AnimationEventSample`의 채널 필드에 생성된 채널을 연결합니다.

코드 없이 연결하려면 `AnimationEventChannelListener`를 추가하고 Channel과 Response를 Inspector에서 설정합니다.
사용자는 Animation Event의 내부 릴레이 함수명이나 Object Parameter 규약을 직접 관리하지 않습니다.

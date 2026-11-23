# Animation Event Channel 기본 예제

`AnimationEventSample.unity` Scene에 `AnimationEventReceiver`와 `AnimationEventSample`이 이미
부착된 GameObject가 포함돼 있습니다.

## 확인 절차

1. `AnimationEventSample.unity`를 엽니다.
2. `Jeomseon > Tool > Animation > Event Channel Authoring`을 엽니다.
3. AnimationClip, AnimatorController 또는 Animator가 있는 GameObject를 선택합니다.
4. 변환 버튼을 누르면 기존 Animation Event 함수명별 채널 에셋이 생성되고 자동 연결됩니다.
5. Scene의 `Animation Event Sample` GameObject에 위에서 생성한 채널을 재생하려는 Animator·Animation
   Clip 흐름에 연결하고, `AnimationEventSample`의 채널 필드에 생성된 채널을 지정합니다. 채널 에셋은
   Authoring 도구가 프로젝트별로 생성하므로 Sample에 미리 채워둘 수 없습니다.
6. Play Mode에서 애니메이션을 재생해 Console에 `Animation Event 수신: ...` 로그가 출력되는지
   확인합니다.

코드 없이 연결하려면 `AnimationEventChannelListener`를 추가하고 Channel과 Response를 Inspector에서 설정합니다.
사용자는 Animation Event의 내부 릴레이 함수명이나 Object Parameter 규약을 직접 관리하지 않습니다.

## Authoring 미리보기와 변환

1. Package Manager에서 Animation 패키지의 **Basic Usage** 샘플을 Import합니다.
2. Project 창의 샘플 폴더에서 `AuthoringClips`를 열고 `FootstepPreview01`부터
   `FootstepPreview08`까지 선택합니다. 각 클립에는 기존 함수명 `Footstep` 이벤트가 2개 있습니다.
3. **Jeomseon > Tool > Animation > Event Channel Authoring**을 엽니다. **Preview (8 clips)**에서
   클립마다 **conversion candidates 2**, **invalid channel events 0**이 표시되는지 확인합니다.
   창 높이를 줄이고 마지막 클립까지 세로로 스크롤합니다.
4. **Convert Selected Events To Channels**를 누릅니다. 기본 Channel Folder를 사용하면
   `Assets/AnimationEventChannels/FootstepAnimationEventChannel.asset`이 생성됩니다.
   결과 창에서 **Changed clips 8**, **Changed events 16**, **Invalid events 0**을 확인합니다.
   변환 후에는 미리보기의 모든 전환 후보가 0이 됩니다.

변환은 가져온 `.anim`을 수정합니다. 같은 입력으로 다시 확인하려면 변환을 Undo하거나
샘플을 다시 Import합니다.

# Animation Event Channel 기본 예제

`AnimationEventSample.unity` Scene에 `AnimationEventReceiver`와 `AnimationEventSample`이 이미
부착된 GameObject가 포함돼 있습니다.

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

## 채널 수신 확인

`AnimationEventSample.unity`의 **Animation Event Sample** GameObject에
`AnimationEventReceiver`와 `AnimationEventSample`이 연결돼 있습니다. 변환한 클립을
재생하는 Animator와 생성된 채널을 연결하면 Play Mode에서
`Animation Event 수신: ...` 로그를 확인할 수 있습니다.

코드 없이 연결하려면 `AnimationEventChannelListener`를 추가하고 Channel과 Response를 Inspector에서 설정합니다.
사용자는 Animation Event의 내부 릴레이 함수명이나 Object Parameter 규약을 직접 관리하지 않습니다.

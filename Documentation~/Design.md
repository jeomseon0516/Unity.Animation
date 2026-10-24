# Animation Event Channel 설계

[English](Design.en.md) · [설치와 예제](../README.md)

## 채널을 사용하는 이유

Unity Animation Event는 재생 객체의 함수명으로 전달됩니다. 이 패키지는 그 엔진 규약을
Receiver 하나에 모으고, 게임 코드가 구독하는 의미는 ScriptableObject 채널 참조로 표현합니다.
클립 안의 함수명을 모두 없애는 방식은 아닙니다. Authoring 도구가 내부 릴레이 함수명과
`objectReferenceParameter` 채널을 함께 설정합니다.

```text
AnimationClip Event → AnimationEventReceiver → AnimationEventChannel.Raised → 구독자
```

Receiver는 이벤트를 발생시킨 Animator/Animation의 재생 객체에 필요합니다. Channel은 공유 에셋이므로
여러 캐릭터가 같은 채널을 사용하면 그 이벤트가 합쳐집니다. 호출자를 구분해야 한다면 payload와
채널 범위를 프로젝트에서 명시적으로 설계하세요. Listener 컴포넌트는 Inspector UnityEvent 연결용입니다.

## Authoring과 원본 보존

`Jeomseon/Tool/Animation/Event Channel Authoring`에서 Clip, Controller 또는 재생 GameObject를
선택합니다. 기존 의미 있는 함수명별 채널을 생성하고 이벤트를 변환합니다. GameObject 선택 경로는
Receiver도 Undo 가능하게 추가하지만 Clip/Controller만 변환할 때는 재생 객체의 Receiver를 별도로
확인해야 합니다.

FBX 내부 클립은 원본을 직접 덮어쓰지 않습니다. 편집 가능한 `.anim`, 원본 매핑,
AnimatorOverrideController를 만들고 사용자가 실제 Animator에 OverrideController를 연결합니다.
원본 재임포트 후에는 Preview의 refresh required를 확인하고 추출/갱신 명령을 다시 실행합니다.

## 구독·확장·검증

구독은 OnEnable에서 추가하고 OnDisable에서 해제합니다. `AnimationEventChannel`을 상속해
OnRaised에서 int/float 등 프로젝트 payload로 변환할 수 있습니다. 기존 Raised도 전달하려면
`base.OnRaised`를 호출합니다. 함수명 기반 Register/Unregister 서비스는 제공하지 않습니다.

Validator는 미변환 이벤트, 채널 누락, 중복 시간/채널, Receiver 누락과 파생 클립 갱신 필요 상태를
검사합니다. 이는 잘못된 Animator 전이나 게임 로직 전체를 검증하는 도구는 아닙니다.

[샘플](../Samples~/BasicUsage/README.md)을 Import하고 실제 클립·채널을 연결한 뒤 Play에서
구독 로그를 확인합니다. [채널](../Runtime/Channels/AnimationEventChannel.cs) →
[Receiver](../Runtime/AnimationEventReceiver.cs) →
[Authoring](../Editor/Channels/AnimationEventChannelAuthoringWindow.cs) →
[Imported workflow](../Editor/Channels/ImportedAnimationClipWorkflow.cs) 순서로 읽습니다.

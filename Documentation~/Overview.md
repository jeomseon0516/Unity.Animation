# Jeomseon Unity Animation

[English](Overview.en.md) · [설치](../README.md)

## 목적

Animation Event의 의미를 ScriptableObject 채널로 표현하고, Editor에서 클립의 릴레이 설정을 자동 구성합니다. Unity 자체의 함수명 호출은 내부 릴레이에 남습니다.

## 사용 흐름

의미 있는 Animation Event 작성 → Authoring 도구에서 채널 변환 → 재생 객체의 Receiver → 채널 Raised 구독. OnEnable/OnDisable에서 구독·해제를 대칭으로 처리합니다.

## 계약과 제한

공유 채널은 여러 재생 객체의 이벤트를 함께 전달할 수 있습니다. FBX 임포트 클립은 복사본과 OverrideController로 작업하며 원본 변경은 명시적으로 refresh해야 합니다.

## 패키지 요구 사항

- Package ID: `com.jeomseon.unity.animation`
- Version: `0.3.1`
- Unity: `6000.6` / `0f1`
- `com.unity.modules.animation`: `1.0.0`

## 샘플과 검증

Package Manager > Samples에서 다음 예제를 Import하세요. 포함된 Scene 또는 Editor 도구를 열고 샘플 README의 Setup 절차를 먼저 수행합니다.

- [Basic Usage](../Samples~/BasicUsage/README.md)
  - [AnimationEventSample](../Samples~/BasicUsage/AnimationEventSample.unity)

[테스트 코드](../Tests)

소비 프로젝트 manifest의 `testables`에 패키지 ID를 추가하고 Unity Test Runner의 관련 어셈블리를 실행합니다. 위 링크는 테스트 위치이며 이번 문서 수정으로 새 테스트 실행을 주장하지 않습니다.

## 코드 지도

- [AnimationEventReceiver](../Runtime/AnimationEventReceiver.cs)
- [AnimationEventChannel](../Runtime/Channels/AnimationEventChannel.cs)
- [AnimationEventChannelListener](../Runtime/Channels/AnimationEventChannelListener.cs)
- [AnimationEventChannelAuthoringWindow](../Editor/Channels/AnimationEventChannelAuthoringWindow.cs)
- [ImportedAnimationClipConversionMap](../Editor/Channels/ImportedAnimationClipConversionMap.cs)

[상세 설계](Design.md)

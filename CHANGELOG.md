# 변경 기록

## [Unreleased]

## [0.2.2] - 2026-08-11

- `Basic Usage` Sample에 `AnimationEventReceiver`·`AnimationEventSample`이 이미 부착된
  `AnimationEventSample.unity` Scene을 추가했습니다. 기존에는 Scene 자산 없이 README로
  GameObject 생성·컴포넌트 부착을 안내만 하고 있어 `AGENTS.md`의 샘플 정책(Scene 자산 필수)을
  충족하지 못했습니다.
- 워크스페이스 명명 규칙에 맞춰 `[SerializeField] private` 필드를 `_camelCase`에서 `camelCase`로
  정리하고 기존 이름을 `[FormerlySerializedAs]`로 보존했습니다. 공개 C# API 변경은 없으며 기존
  Scene·Prefab의 직렬화된 값은 그대로 유지됩니다. 리네이밍 과정에서
  `ImportedAnimationClipConversionMap.Entry.Set()`이 매개변수와 필드 이름이 같아지며 자기 대입
  (no-op)이 되던 결함을 발견해 `this.` 한정자로 수정했습니다.

## [0.2.0] - 2026-08-05

- 문자열 키 기반 `Register`/`Unregister` 라우팅을 `AnimationEventChannel` 에셋 기반 라우팅으로 교체했습니다.
- 코드 구독과 Inspector UnityEvent 연결을 위한 `AnimationEventChannelListener`를 추가했습니다.
- 기존 함수명별 채널 생성, 이벤트 연결, Receiver 추가를 자동화하는 Authoring 도구를 추가했습니다.
- 기존 이벤트와 누락된 채널 참조를 찾는 선택 항목 진단 메뉴를 추가했습니다.
- 채널 전달과 잘못된 이벤트 방어 동작을 검증하는 테스트를 추가했습니다.
- 읽기 전용 임포트 클립의 명시적 `.anim` 추출, 원본 매핑, 갱신 진단과 AnimatorOverrideController 생성을 추가했습니다.
- 동일 시간·동일 채널 중복 이벤트와 선택 계층의 AnimationEventReceiver 누락 상세 진단을 추가했습니다.
- 프로젝트가 타입별 payload 채널을 직접 정의할 수 있도록 `AnimationEventChannel.OnRaised` 확장 지점을 추가했습니다.

## [0.1.2] - 2026-07-29

- Runtime·Editor·Samples 어셈블리의 `rootNamespace`와 소스 파일 위치를 namespace에 맞게 정리했습니다.

## [0.1.1] - 2026-07-29

- Animation Event 라우팅을 확인하는 `Basic Usage` 샘플을 추가했습니다.

## [0.1.0] - 2026-07-29

- JeomseonScriptPack의 관련 모듈을 독립 UPM 패키지로 분리했습니다.


## [0.2.1] - 2026-08-05

- Unity 6000.5.7f1을 최소 지원 버전으로 상향했습니다.

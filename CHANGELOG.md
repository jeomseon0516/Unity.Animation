# 변경 기록

## [Unreleased]

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

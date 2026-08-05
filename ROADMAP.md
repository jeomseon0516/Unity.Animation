# Animation 로드맵

우선순위: `P0` 결함·안전성 → `P1` 핵심 구조 → `P2` 도구 안정성 → `P3` 장기 확장

## 완료

- 문자열 키 라우팅을 `AnimationEventChannel` ScriptableObject 계약으로 교체
- 코드 구독과 Inspector 기반 채널 리스너 제공
- 기존 함수명별 채널 생성 및 Animation Event 연결 자동화
- 선택 GameObject의 Receiver 자동 보강, 미리보기, Undo, 다중 선택, 읽기 전용 건너뛰기
- 기존 이벤트와 누락 채널 진단
- Editor 변환 자동 테스트와 TestProject PlayMode 채널 전달 검증
- 읽기 전용 임포트 클립 추출, 명시적 갱신과 AnimatorOverrideController 워크플로우
- 동일 시간·동일 채널 중복 이벤트와 Receiver 누락 상세 진단
- 사용자 정의 타입 채널을 위한 `AnimationEventChannel.OnRaised` 확장 지점

## 후속 확장 메모

- **Timeline 통합**: 기본 Signal로 해결되지 않는 실제 요구가 생기면 Marker/Emitter 어댑터를 검토합니다.
- **StateMachineBehaviour 통합**: State 진입·종료 채널 요구가 생기면 Animation Event와 분리된 기능 영역으로 검토합니다.
- **진단 UX 확장**: 결과 클릭 이동, 자동 수정, 프로젝트 전체 검사와 CI 진입점은 안정화 작업 이후 검토합니다.
- **비동기 수신**: 채널은 동기 전달을 유지하고, 완료 추적 요구가 확인될 때만 별도 계약을 검토합니다.

현재 우선순위는 새 확장 기능 구현이 아니라 전체 `Jeomseon.Unity.*` 패키지의 안정화와 리팩토링입니다.

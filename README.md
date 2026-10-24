# Jeomseon Unity Animation

한국어 | [English](README.en.md)

Animation Event의 의미를 ScriptableObject 채널로 표현하고, Editor에서 클립의 릴레이 설정을 자동 구성합니다. Unity 자체의 함수명 호출은 내부 릴레이에 남습니다.

## 빠르게 시작하기

의미 있는 Animation Event 작성 → Authoring 도구에서 채널 변환 → 재생 객체의 Receiver → 채널 Raised 구독. OnEnable/OnDisable에서 구독·해제를 대칭으로 처리합니다.

전체 샘플 코드: [AnimationEventSample.cs](Samples~/BasicUsage/AnimationEventSample.cs).

```csharp
using Jeomseon.Unity.Animation;
using Jeomseon.Unity.Animation.Channels;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jeomseon.Samples.Animation
{
    /// <summary>
    /// Shows the code-driven subscription workflow for an Animation Event channel.
    /// Animation Event 채널을 코드로 구독하는 사용 예제입니다.
    /// </summary>
    [RequireComponent(typeof(AnimationEventReceiver))]
    public sealed class AnimationEventSample : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("_footstepChannel")] private AnimationEventChannel footstepChannel;

        private void OnEnable()
        {
            if (footstepChannel != null)
            {
                footstepChannel.Raised += OnFootstep;
            }
        }

        private void OnDisable()
        {
            if (footstepChannel != null)
            {
                footstepChannel.Raised -= OnFootstep;
            }
        }

        private static void OnFootstep(AnimationEvent animationEvent)
        {
            Debug.Log($"Animation Event 수신: {animationEvent.time}");
        }
    }
}
```

## 계약과 주의사항

공유 채널은 여러 재생 객체의 이벤트를 함께 전달할 수 있습니다. FBX 임포트 클립은 복사본과 OverrideController로 작업하며 원본 변경은 명시적으로 refresh해야 합니다.

## 요구 사항

- Unity `6000.6.0f1`
- 패키지: `com.jeomseon.unity.animation` `0.3.1`
- `com.unity.modules.animation`: `1.0.0`

## 설치

프로젝트 `Packages/manifest.json`의 기존 dependencies와 registry를 유지하면서 다음 항목을 병합합니다.

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.jeomseon"
      ]
    }
  ],
  "dependencies": {
    "com.jeomseon.unity.animation": "0.3.1"
  }
}
```

또는 Package Manager의 **Install package from git URL**을 사용합니다:

```text
https://github.com/jeomseon0516/Unity.Animation.git#v0.3.1
```

## 설계와 동작

### 채널을 사용하는 이유

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

### Authoring과 원본 보존

`Jeomseon/Tool/Animation/Event Channel Authoring`에서 Clip, Controller 또는 재생 GameObject를
선택합니다. 기존 의미 있는 함수명별 채널을 생성하고 이벤트를 변환합니다. GameObject 선택 경로는
Receiver도 Undo 가능하게 추가하지만 Clip/Controller만 변환할 때는 재생 객체의 Receiver를 별도로
확인해야 합니다.

FBX 내부 클립은 원본을 직접 덮어쓰지 않습니다. 편집 가능한 `.anim`, 원본 매핑,
AnimatorOverrideController를 만들고 사용자가 실제 Animator에 OverrideController를 연결합니다.
원본 재임포트 후에는 Preview의 refresh required를 확인하고 추출/갱신 명령을 다시 실행합니다.

### 구독·확장·검증

구독은 OnEnable에서 추가하고 OnDisable에서 해제합니다. `AnimationEventChannel`을 상속해
OnRaised에서 int/float 등 프로젝트 payload로 변환할 수 있습니다. 기존 Raised도 전달하려면
`base.OnRaised`를 호출합니다. 함수명 기반 Register/Unregister 서비스는 제공하지 않습니다.

Validator는 미변환 이벤트, 채널 누락, 중복 시간/채널, Receiver 누락과 파생 클립 갱신 필요 상태를
검사합니다. 이는 잘못된 Animator 전이나 게임 로직 전체를 검증하는 도구는 아닙니다.

[샘플](Samples~/BasicUsage/README.md)을 Import하고 실제 클립·채널을 연결한 뒤 Play에서
구독 로그를 확인합니다. [채널](Runtime/Channels/AnimationEventChannel.cs) →
[Receiver](Runtime/AnimationEventReceiver.cs) →
[Authoring](Editor/Channels/AnimationEventChannelAuthoringWindow.cs) →
[Imported workflow](Editor/Channels/ImportedAnimationClipWorkflow.cs) 순서로 읽습니다.

## API 사용과 상세 설명

### 권장 워크플로우

1. AnimationClip에 평소처럼 Animation Event를 추가하고 의미를 나타내는 함수명(예: `Footstep`)을 입력합니다.
2. 클립, AnimatorController 또는 Animator가 있는 GameObject를 선택합니다.
3. `Jeomseon > Tool > Animation > Event Channel Authoring`을 열고 변환합니다.
4. 도구가 함수명별 `AnimationEventChannel` 에셋을 만들고 이벤트의 채널 연결을 자동 구성합니다.
5. 코드에서 채널의 `Raised` 이벤트를 구독하거나 `AnimationEventChannelListener`로 Inspector UnityEvent에 연결합니다.

GameObject를 선택해 변환하면 Animator 또는 Legacy Animation이 있는 대상에
`AnimationEventReceiver`도 Undo 가능한 방식으로 자동 추가됩니다. 클립이나 Controller만 선택한 경우에는
실제 재생 GameObject에 Receiver를 추가해야 합니다.

#### FBX 등 임포트 클립

FBX 내부 AnimationClip은 직접 수정하지 않습니다.

1. FBX, AnimatorController 또는 해당 Animator가 있는 GameObject를 선택합니다.
2. Authoring Window에서 Channel Folder와 Derived Clip Folder를 지정합니다.
3. `Extract Imported Clips And Create Overrides`를 누릅니다.
4. 도구가 편집 가능한 `.anim` 복사본, 원본 매핑 에셋과 `AnimatorOverrideController`를 생성합니다.
5. 생성된 Override Controller를 실제 Animator의 Controller에 지정합니다.

원본 FBX가 바뀌면 Preview에 `refresh required`가 표시됩니다. 같은 버튼을 다시 눌러 파생 클립과
Override Controller를 명시적으로 갱신합니다. 패키지는 `AssetPostprocessor`로 임포트 결과를 자동 변경하지 않습니다.

#### 코드 구독

```csharp
using Jeomseon.Unity.Animation.Channels;
using UnityEngine;

public sealed class FootstepPlayer : MonoBehaviour
{
    [SerializeField] private AnimationEventChannel _footstepChannel;

    private void OnEnable() => _footstepChannel.Raised += OnFootstep;
    private void OnDisable() => _footstepChannel.Raised -= OnFootstep;

    private static void OnFootstep(AnimationEvent animationEvent)
    {
        Debug.Log($"Footstep at {animationEvent.time}");
    }
}
```

#### Inspector 구독

`AnimationEventChannelListener` 컴포넌트에서 Channel을 지정하고 Response에 호출할 메서드를 연결합니다.
이 방식은 구독과 해제를 컴포넌트 활성 수명주기에 맞춰 자동 처리합니다.

#### 타입 채널 확장

패키지는 타입별 채널을 고정 제공하지 않습니다. 필요한 payload 계약은 `AnimationEventChannel`을 상속하고
`OnRaised`에서 변환해 프로젝트가 직접 정의할 수 있습니다.

```csharp
using System;
using Jeomseon.Unity.Animation.Channels;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Animation/Int Event Channel")]
public sealed class IntAnimationEventChannel : AnimationEventChannel
{
    public event Action<int> ValueRaised;

    protected override void OnRaised(AnimationEvent animationEvent)
    {
        base.OnRaised(animationEvent);
        ValueRaised?.Invoke(animationEvent.intParameter);
    }
}
```

`base.OnRaised`를 호출하면 기본 `Raised` 구독자와 타입별 구독자를 함께 지원합니다.

### 진단

`Jeomseon > Tool > Animation > Validate Selected Event Channels`는 선택한 클립의 다음 상태를 검사합니다.

- 아직 채널로 변환되지 않은 기존 Animation Event
- 내부 릴레이는 설정됐지만 채널 참조가 빠진 Animation Event
- 추출되지 않았거나 원본 변경 후 갱신이 필요한 임포트 클립
- 동일 클립에서 같은 시간·같은 채널을 참조하는 중복 Animation Event
- 채널 이벤트가 있는 클립을 재생하지만 `AnimationEventReceiver`가 없는 선택 계층의 GameObject

중복 이벤트는 클립명, 시간, 채널명과 이벤트 index를 표시합니다. Receiver 누락은 선택한 루트부터의
GameObject 경로와 관련 클립명을 표시합니다.

문자열 `Register`/`Unregister` 라우팅 API는 제공하지 않습니다.

## 샘플 실행

**Package Manager > Samples**에서 예제를 Import합니다. 포함된 Scene 또는 Editor 도구를 열고 필요한 Setup 메뉴를 먼저 실행합니다.

- [Basic Usage](Samples~/BasicUsage/README.md)
  - [AnimationEventSample](Samples~/BasicUsage/AnimationEventSample.unity)

## 테스트

[테스트 코드](Tests)

manifest의 `testables`에 패키지 ID를 추가하고 Unity Test Runner의 관련 EditMode/PlayMode 어셈블리를 실행합니다.

## 추가 문서

[전체 코드 지도와 패키지 구성](Documentation~/Overview.md)
- [Changelog](CHANGELOG.md)

## 라이선스

[License](LICENSE.md)

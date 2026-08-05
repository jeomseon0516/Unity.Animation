# Jeomseon Unity Animation

Unity Animation Event를 문자열 키가 아닌 `AnimationEventChannel` 에셋으로 라우팅하는 패키지입니다.
Editor 도구가 Unity 내부 릴레이 규약을 자동 설정하므로 사용자는 채널만 생성·구독하면 됩니다.

## 설치

Package Manager의 **Add package from git URL**에서 다음 주소를 사용합니다.

```text
https://github.com/jeomseon0516/Unity.Animation.git#v0.2.0
```

## 권장 워크플로우

1. AnimationClip에 평소처럼 Animation Event를 추가하고 의미를 나타내는 함수명(예: `Footstep`)을 입력합니다.
2. 클립, AnimatorController 또는 Animator가 있는 GameObject를 선택합니다.
3. `Tool > Animation > Event Channel Authoring`을 열고 변환합니다.
4. 도구가 함수명별 `AnimationEventChannel` 에셋을 만들고 이벤트의 채널 연결을 자동 구성합니다.
5. 코드에서 채널의 `Raised` 이벤트를 구독하거나 `AnimationEventChannelListener`로 Inspector UnityEvent에 연결합니다.

GameObject를 선택해 변환하면 Animator 또는 Legacy Animation이 있는 대상에
`AnimationEventReceiver`도 Undo 가능한 방식으로 자동 추가됩니다. 클립이나 Controller만 선택한 경우에는
실제 재생 GameObject에 Receiver를 추가해야 합니다.

### FBX 등 임포트 클립

FBX 내부 AnimationClip은 직접 수정하지 않습니다.

1. FBX, AnimatorController 또는 해당 Animator가 있는 GameObject를 선택합니다.
2. Authoring Window에서 Channel Folder와 Derived Clip Folder를 지정합니다.
3. `Extract Imported Clips And Create Overrides`를 누릅니다.
4. 도구가 편집 가능한 `.anim` 복사본, 원본 매핑 에셋과 `AnimatorOverrideController`를 생성합니다.
5. 생성된 Override Controller를 실제 Animator의 Controller에 지정합니다.

원본 FBX가 바뀌면 Preview에 `refresh required`가 표시됩니다. 같은 버튼을 다시 눌러 파생 클립과
Override Controller를 명시적으로 갱신합니다. 패키지는 `AssetPostprocessor`로 임포트 결과를 자동 변경하지 않습니다.

### 코드 구독

```csharp
using Jeomseon.Animation.Channels;
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

### Inspector 구독

`AnimationEventChannelListener` 컴포넌트에서 Channel을 지정하고 Response에 호출할 메서드를 연결합니다.
이 방식은 구독과 해제를 컴포넌트 활성 수명주기에 맞춰 자동 처리합니다.

### 타입 채널 확장

패키지는 타입별 채널을 고정 제공하지 않습니다. 필요한 payload 계약은 `AnimationEventChannel`을 상속하고
`OnRaised`에서 변환해 프로젝트가 직접 정의할 수 있습니다.

```csharp
using System;
using Jeomseon.Animation.Channels;
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

## 진단

`Tool > Animation > Validate Selected Event Channels`는 선택한 클립의 다음 상태를 검사합니다.

- 아직 채널로 변환되지 않은 기존 Animation Event
- 내부 릴레이는 설정됐지만 채널 참조가 빠진 Animation Event
- 추출되지 않았거나 원본 변경 후 갱신이 필요한 임포트 클립
- 동일 클립에서 같은 시간·같은 채널을 참조하는 중복 Animation Event
- 채널 이벤트가 있는 클립을 재생하지만 `AnimationEventReceiver`가 없는 선택 계층의 GameObject

중복 이벤트는 클립명, 시간, 채널명과 이벤트 index를 표시합니다. Receiver 누락은 선택한 루트부터의
GameObject 경로와 관련 클립명을 표시합니다.

문자열 `Register`/`Unregister` 라우팅 API는 제공하지 않습니다.

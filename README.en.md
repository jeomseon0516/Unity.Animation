# Jeomseon Unity Animation

[한국어](README.md) | English

Represents Animation Event meaning with ScriptableObject channels and automates clip relay configuration in the Editor. Unity's function-name dispatch remains inside the relay.

## Quick start

Author meaningful events, convert with the authoring tool, receive on the playing object, and subscribe to channel Raised. Pair subscriptions in OnEnable/OnDisable.

Complete sample: [AnimationEventSample.cs](Samples~/BasicUsage/AnimationEventSample.cs).

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

## Contracts and limitations

A shared channel can receive events from multiple playing objects. Imported FBX clips use derived copies and an OverrideController; explicitly refresh after source changes.

## Requirements

- Unity `6000.6.0f1`
- Package: `com.jeomseon.unity.animation` `0.3.1`
- `com.unity.modules.animation`: `1.0.0`

## Installation

Merge this into your project's `Packages/manifest.json`; preserve existing dependencies and registries.

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

Or use **Install package from git URL** in Package Manager:

```text
https://github.com/jeomseon0516/Unity.Animation.git#v0.3.1
```

## Design and behavior

Unity Animation Events dispatch by method name on the playing object. This package centralizes
that engine convention in one Receiver and represents gameplay meaning with ScriptableObject channel
references. It does not remove method names from clips: authoring assigns both the relay method and
the channel in objectReferenceParameter.

```text
AnimationClip Event → AnimationEventReceiver → AnimationEventChannel.Raised → subscribers
```

The playing Animator/Animation object needs a Receiver. A Channel is shared: events from multiple
characters using it are combined. Design payloads/channel scope explicitly when sender identity matters.
The Listener component connects events to Inspector UnityEvents.

### Authoring and source preservation

Select Clips, Controllers, or playing GameObjects in
`Jeomseon/Tool/Animation/Event Channel Authoring`. The tool creates channels by original meaningful
function name and converts events. GameObject selection also adds missing Receivers with Undo;
Clip/Controller-only conversion still requires checking the runtime object's Receiver.

Imported FBX clips are not overwritten. The tool creates editable .anim copies, a source mapping,
and an AnimatorOverrideController; assign that controller to the actual Animator. After source
reimport, inspect refresh required and explicitly rerun extraction/refresh.

### Subscriptions, extensions, and verification

Subscribe in OnEnable and unsubscribe in OnDisable. Derive AnimationEventChannel and override
OnRaised to convert int/float/project payloads. Call base.OnRaised to preserve base subscribers.
No function-name Register/Unregister service is supplied.

Validator detects unconverted events, missing channels, duplicate time/channel pairs, missing
Receivers, and stale derived clips. It does not validate all Animator transitions or gameplay logic.
Import the [sample](Samples~/BasicUsage/README.md), connect real clips/channels, and verify Play logs.
Read [Channel](Runtime/Channels/AnimationEventChannel.cs),
[Receiver](Runtime/AnimationEventReceiver.cs),
[Authoring](Editor/Channels/AnimationEventChannelAuthoringWindow.cs), then
[import workflow](Editor/Channels/ImportedAnimationClipWorkflow.cs).

## API usage and details

### Workflow

1. Add Animation Events with meaningful function names such as `Footstep`.
2. Select clips, Animator Controllers, or GameObjects containing an Animator.
3. Open `Jeomseon > Tool > Animation > Event Channel Authoring` and convert the selection.
4. The tool creates one channel per original function name and assigns it to each event.
5. Subscribe to `AnimationEventChannel.Raised` in code, or use `AnimationEventChannelListener` for Inspector-configured responses.

For animation clips embedded in FBX or other imported assets, select the clip, controller, or Animator GameObject and
click `Extract Imported Clips And Create Overrides`. The tool creates editable `.anim` copies, a stable source mapping,
and an `AnimatorOverrideController`. Assign the generated override controller to the runtime Animator. When the source
asset changes, the preview reports `refresh required`; run the same explicit action again to refresh derived assets.
No automatic `AssetPostprocessor` mutation is performed.

Typed payload contracts can be implemented by deriving from `AnimationEventChannel` and overriding the protected
`OnRaised(AnimationEvent)` hook. Call `base.OnRaised` when both the base `Raised` event and the typed event should fire.

When GameObjects are selected, missing `AnimationEventReceiver` components are added automatically with Undo support.
Use `Jeomseon > Tool > Animation > Validate Selected Event Channels` to find legacy or incomplete events, duplicate events that
reference the same channel at the same time, and selected Animator/Animation GameObjects that play channel events without
an `AnimationEventReceiver`. Diagnostics include clip names, event times and hierarchy paths.

## Samples

Import the desired entry from **Package Manager > Samples**. Use the included Scene/Editor tool and complete its Setup step first, if required.

- [Basic Usage](Samples~/BasicUsage/README.md)
  - [AnimationEventSample](Samples~/BasicUsage/AnimationEventSample.unity)

## Tests

[Test sources](Tests)

Add the package ID to manifest `testables`, then execute its relevant EditMode/PlayMode assemblies in Unity Test Runner.

## Further reading

[Complete source map and package metadata](Documentation~/Overview.en.md)
- [Changelog](CHANGELOG.md)

## License

[License](LICENSE.md)

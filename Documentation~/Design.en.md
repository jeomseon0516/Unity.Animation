# Animation Event Channel design

[한국어](Design.md) · [Installation and examples](../README.en.md)

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

## Authoring and source preservation

Select Clips, Controllers, or playing GameObjects in
`Jeomseon/Tool/Animation/Event Channel Authoring`. The tool creates channels by original meaningful
function name and converts events. GameObject selection also adds missing Receivers with Undo;
Clip/Controller-only conversion still requires checking the runtime object's Receiver.

Imported FBX clips are not overwritten. The tool creates editable .anim copies, a source mapping,
and an AnimatorOverrideController; assign that controller to the actual Animator. After source
reimport, inspect refresh required and explicitly rerun extraction/refresh.

## Subscriptions, extensions, and verification

Subscribe in OnEnable and unsubscribe in OnDisable. Derive AnimationEventChannel and override
OnRaised to convert int/float/project payloads. Call base.OnRaised to preserve base subscribers.
No function-name Register/Unregister service is supplied.

Validator detects unconverted events, missing channels, duplicate time/channel pairs, missing
Receivers, and stale derived clips. It does not validate all Animator transitions or gameplay logic.
Import the [sample](../Samples~/BasicUsage/README.md), connect real clips/channels, and verify Play logs.
Read [Channel](../Runtime/Channels/AnimationEventChannel.cs),
[Receiver](../Runtime/AnimationEventReceiver.cs),
[Authoring](../Editor/Channels/AnimationEventChannelAuthoringWindow.cs), then
[import workflow](../Editor/Channels/ImportedAnimationClipWorkflow.cs).

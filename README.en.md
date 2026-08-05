# Jeomseon Unity Animation

Routes Unity Animation Events through `AnimationEventChannel` assets instead of string keys.
Editor tooling configures Unity's relay convention, leaving consumers to create and subscribe to channels.

## Workflow

1. Add Animation Events with meaningful function names such as `Footstep`.
2. Select clips, Animator Controllers, or GameObjects containing an Animator.
3. Open `Tool > Animation > Event Channel Authoring` and convert the selection.
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
Use `Tool > Animation > Validate Selected Event Channels` to find legacy or incomplete events, duplicate events that
reference the same channel at the same time, and selected Animator/Animation GameObjects that play channel events without
an `AnimationEventReceiver`. Diagnostics include clip names, event times and hierarchy paths.

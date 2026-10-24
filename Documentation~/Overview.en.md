# Jeomseon Unity Animation

[한국어](Overview.md) · [Installation](../README.en.md)

## Purpose

Represents Animation Event meaning with ScriptableObject channels and automates clip relay configuration in the Editor. Unity's function-name dispatch remains inside the relay.

## Usage flow

Author meaningful events, convert with the authoring tool, receive on the playing object, and subscribe to channel Raised. Pair subscriptions in OnEnable/OnDisable.

## Contracts and limitations

A shared channel can receive events from multiple playing objects. Imported FBX clips use derived copies and an OverrideController; explicitly refresh after source changes.

## Package requirements

- Package ID: `com.jeomseon.unity.animation`
- Version: `0.3.1`
- Unity: `6000.6` / `0f1`
- `com.unity.modules.animation`: `1.0.0`

## Samples and verification

Import the following from Package Manager > Samples. Open the included Scene or Editor tool; follow any Setup step in its sample README.

- [Basic Usage](../Samples~/BasicUsage/README.md)
  - [AnimationEventSample](../Samples~/BasicUsage/AnimationEventSample.unity)

[Test sources](../Tests)

Add the package ID to manifest `testables`, then run the relevant Unity Test Runner assemblies. These links describe available tests, not a new successful test run.

## Source map

- [AnimationEventReceiver](../Runtime/AnimationEventReceiver.cs)
- [AnimationEventChannel](../Runtime/Channels/AnimationEventChannel.cs)
- [AnimationEventChannelListener](../Runtime/Channels/AnimationEventChannelListener.cs)
- [AnimationEventChannelAuthoringWindow](../Editor/Channels/AnimationEventChannelAuthoringWindow.cs)
- [ImportedAnimationClipConversionMap](../Editor/Channels/ImportedAnimationClipConversionMap.cs)

[Detailed design](Design.en.md)

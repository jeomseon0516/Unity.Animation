using System.Linq;
using Jeomseon.Unity.Animation.Channels;
using Jeomseon.Unity.Animation.Editor.Channels;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Jeomseon.Animation.Editor.Tests
{
    public sealed class ImportedAnimationClipWorkflowTests
    {
        private const string TestFolder = "Assets/__JeomseonImportedAnimationTests";
        private const string DerivedFolder = TestFolder + "/Derived";
        private const string ChannelFolder = TestFolder + "/Channels";

        [SetUp]
        public void SetUp()
        {
            AnimationEventChannelAuthoringWindow.EnsureFolderExists(DerivedFolder);
            AnimationEventChannelAuthoringWindow.EnsureFolderExists(ChannelFolder);
        }

        [TearDown]
        public void TearDown()
        {
            Undo.ClearAll();
            AssetDatabase.DeleteAsset(TestFolder);
            AssetDatabase.Refresh();
        }

        [Test]
        public void ExtractOrUpdate_CreatesEditableClipAndRecordsStableSourceIdentity()
        {
            AnimationClip sourceClip = CreateImportedClip("Walk", "Footstep");
            ImportedAnimationClipConversionMap map =
                ImportedAnimationClipWorkflow.LoadOrCreateMap(DerivedFolder);

            ImportedAnimationClipWorkflow.ExtractionResult first = Extract(sourceClip, map);
            ImportedAnimationClipWorkflow.ExtractionResult second = Extract(sourceClip, map);
            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(first.Entry.DerivedClip);

            Assert.That(first.Created, Is.True);
            Assert.That(second.Created, Is.False);
            Assert.That(second.Entry.DerivedClip, Is.SameAs(first.Entry.DerivedClip));
            Assert.That(AssetDatabase.GetAssetPath(first.Entry.DerivedClip), Does.EndWith(".anim"));
            Assert.That(first.Entry.SourceGuid, Is.Not.Empty);
            Assert.That(first.Entry.SourceLocalId, Is.Not.Zero);
            Assert.That(ImportedAnimationClipWorkflow.IsOutOfDate(first.Entry), Is.False);
            Assert.That(events, Has.Length.EqualTo(1));
            Assert.That(events[0].functionName, Is.EqualTo(AnimationEventReceiver.RelayFunctionName));
            Assert.That(events[0].objectReferenceParameter, Is.TypeOf<AnimationEventChannel>());
        }

        [Test]
        public void CreateOrUpdateOverrideController_ReplacesImportedClipWithDerivedClip()
        {
            AnimationClip sourceClip = CreateImportedClip("Attack", "Impact");
            ImportedAnimationClipConversionMap map =
                ImportedAnimationClipWorkflow.LoadOrCreateMap(DerivedFolder);
            ImportedAnimationClipWorkflow.ExtractionResult extraction = Extract(sourceClip, map);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(
                $"{TestFolder}/Character.controller");
            controller.AddMotion(sourceClip);

            AnimatorOverrideController overrideController =
                ImportedAnimationClipWorkflow.CreateOrUpdateOverrideController(
                    controller,
                    map.Entries,
                    DerivedFolder);

            Assert.That(overrideController[sourceClip], Is.SameAs(extraction.Entry.DerivedClip));
            Assert.That(AssetDatabase.GetAssetPath(overrideController),
                Is.EqualTo($"{DerivedFolder}/CharacterAnimationEventChannels.overrideController"));
        }

        [Test]
        public void ExtractOrUpdate_RejectsEditableAnimationAssets()
        {
            AnimationClip editableClip = new() { name = "Editable" };
            AssetDatabase.CreateAsset(editableClip, $"{TestFolder}/Editable.anim");
            ImportedAnimationClipConversionMap map =
                ImportedAnimationClipWorkflow.LoadOrCreateMap(DerivedFolder);

            Assert.That(
                () => Extract(editableClip, map),
                Throws.ArgumentException);
        }

        private static ImportedAnimationClipWorkflow.ExtractionResult Extract(
            AnimationClip sourceClip,
            ImportedAnimationClipConversionMap map)
        {
            return ImportedAnimationClipWorkflow.ExtractOrUpdate(
                sourceClip,
                DerivedFolder,
                functionName => AnimationEventChannelAuthoringWindow.LoadOrCreateChannel(
                    ChannelFolder,
                    functionName),
                map);
        }

        private static AnimationClip CreateImportedClip(string name, string functionName)
        {
            AnimationClip container = new() { name = $"{name}Container" };
            string path = $"{TestFolder}/{name}.asset";
            AssetDatabase.CreateAsset(container, path);

            AnimationClip sourceClip = new() { name = name };
            AssetDatabase.AddObjectToAsset(sourceClip, container);
            AnimationUtility.SetAnimationEvents(sourceClip, new[]
            {
                new AnimationEvent
                {
                    time = 0.1f,
                    functionName = functionName,
                    stringParameter = "Payload"
                }
            });
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .Single(AssetDatabase.IsSubAsset);
        }
    }
}

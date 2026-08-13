using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jeomseon.Unity.Animation.Channels;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Jeomseon.Unity.Animation.Editor.Channels
{
    internal static class ImportedAnimationClipWorkflow
    {
        private const string MapAssetName = "ImportedAnimationClipConversionMap.asset";

        internal static bool IsImportedReadOnlyClip(AnimationClip clip)
        {
            if (clip == null || !AssetDatabase.IsSubAsset(clip))
            {
                return false;
            }

            string path = AssetDatabase.GetAssetPath(clip);
            return !path.EndsWith(".anim", StringComparison.OrdinalIgnoreCase);
        }

        internal static ImportedAnimationClipConversionMap LoadOrCreateMap(string derivedFolder)
        {
            AnimationEventChannelAuthoringWindow.EnsureFolderExists(derivedFolder);
            string path = $"{derivedFolder}/{MapAssetName}";
            ImportedAnimationClipConversionMap map =
                AssetDatabase.LoadAssetAtPath<ImportedAnimationClipConversionMap>(path);
            if (map != null)
            {
                return map;
            }

            map = ScriptableObject.CreateInstance<ImportedAnimationClipConversionMap>();
            AssetDatabase.CreateAsset(map, path);
            return map;
        }

        internal static ImportedAnimationClipConversionMap LoadMap(string derivedFolder)
        {
            return AssetDatabase.LoadAssetAtPath<ImportedAnimationClipConversionMap>(
                $"{derivedFolder}/{MapAssetName}");
        }

        internal static ImportedAnimationClipConversionMap.Entry FindEntry(
            AnimationClip sourceClip,
            ImportedAnimationClipConversionMap map)
        {
            if (sourceClip == null || map == null ||
                !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sourceClip, out string sourceGuid, out long sourceLocalId))
            {
                return null;
            }

            return map.Find(sourceGuid, sourceLocalId);
        }

        internal static ExtractionResult ExtractOrUpdate(
            AnimationClip sourceClip,
            string derivedFolder,
            Func<string, AnimationEventChannel> resolveChannel,
            ImportedAnimationClipConversionMap map)
        {
            if (!IsImportedReadOnlyClip(sourceClip))
            {
                throw new ArgumentException("The source clip must be an imported read-only sub-asset.", nameof(sourceClip));
            }

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sourceClip, out string sourceGuid, out long sourceLocalId))
            {
                throw new InvalidOperationException($"Cannot identify imported clip '{sourceClip.name}'.");
            }

            string sourcePath = AssetDatabase.GetAssetPath(sourceClip);
            string dependencyHash = AssetDatabase.GetAssetDependencyHash(sourcePath).ToString();
            ImportedAnimationClipConversionMap.Entry entry = map.Find(sourceGuid, sourceLocalId);
            AnimationClip derivedClip = entry?.DerivedClip;
            bool created = derivedClip == null;

            if (created)
            {
                derivedClip = new AnimationClip();
            }
            else
            {
                Undo.RecordObject(derivedClip, "Refresh Imported Animation Clip");
            }

            EditorUtility.CopySerialized(sourceClip, derivedClip);
            derivedClip.name = sourceClip.name;

            if (created)
            {
                string sourceName = Path.GetFileNameWithoutExtension(sourcePath);
                string fileName = MakeSafeFileName($"{sourceName}_{sourceClip.name}.anim");
                string derivedPath = AssetDatabase.GenerateUniqueAssetPath($"{derivedFolder}/{fileName}");
                AssetDatabase.CreateAsset(derivedClip, derivedPath);
            }

            AnimationEventChannelMigration.MigrationResult migration =
                AnimationEventChannelMigration.Migrate(derivedClip, resolveChannel);
            Undo.RecordObject(map, "Update Imported Animation Clip Map");
            entry = map.Record(
                sourceGuid,
                sourceLocalId,
                dependencyHash,
                sourceClip,
                derivedClip);
            EditorUtility.SetDirty(map);
            EditorUtility.SetDirty(derivedClip);
            return new ExtractionResult(entry, created, migration.ChangedEvents, migration.InvalidEvents);
        }

        internal static bool IsOutOfDate(ImportedAnimationClipConversionMap.Entry entry)
        {
            if (entry?.SourceClip == null || entry.DerivedClip == null)
            {
                return true;
            }

            string sourcePath = AssetDatabase.GetAssetPath(entry.SourceClip);
            return AssetDatabase.GetAssetDependencyHash(sourcePath).ToString() != entry.SourceDependencyHash;
        }

        internal static bool HasReplacement(
            RuntimeAnimatorController controller,
            IReadOnlyList<ImportedAnimationClipConversionMap.Entry> entries)
        {
            HashSet<AnimationClip> sourceClips = entries
                .Where(entry => entry.SourceClip != null && entry.DerivedClip != null)
                .Select(entry => entry.SourceClip)
                .ToHashSet();
            return controller != null && controller.animationClips.Any(sourceClips.Contains);
        }

        internal static AnimatorOverrideController CreateOrUpdateOverrideController(
            RuntimeAnimatorController sourceController,
            IReadOnlyList<ImportedAnimationClipConversionMap.Entry> entries,
            string outputFolder)
        {
            if (sourceController == null)
            {
                throw new ArgumentNullException(nameof(sourceController));
            }

            AnimationEventChannelAuthoringWindow.EnsureFolderExists(outputFolder);
            string path = $"{outputFolder}/{MakeSafeFileName(sourceController.name)}AnimationEventChannels.overrideController";
            AnimatorOverrideController overrideController =
                AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
            if (overrideController == null)
            {
                overrideController = new AnimatorOverrideController(sourceController);
                AssetDatabase.CreateAsset(overrideController, path);
            }
            else
            {
                Undo.RecordObject(overrideController, "Update Animation Event Override Controller");
                overrideController.runtimeAnimatorController = sourceController;
            }

            Dictionary<AnimationClip, AnimationClip> replacements = entries
                .Where(entry => entry.SourceClip != null && entry.DerivedClip != null)
                .ToDictionary(entry => entry.SourceClip, entry => entry.DerivedClip);
            List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new();
            overrideController.GetOverrides(overrides);

            for (int index = 0; index < overrides.Count; index++)
            {
                KeyValuePair<AnimationClip, AnimationClip> current = overrides[index];
                if (replacements.TryGetValue(current.Key, out AnimationClip replacement))
                {
                    overrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(current.Key, replacement);
                }
            }

            overrideController.ApplyOverrides(overrides);
            EditorUtility.SetDirty(overrideController);
            return overrideController;
        }

        private static string MakeSafeFileName(string value)
        {
            char[] invalidCharacters = Path.GetInvalidFileNameChars();
            return new string(value.Select(character => invalidCharacters.Contains(character) ? '_' : character).ToArray());
        }

        internal readonly struct ExtractionResult
        {
            internal ExtractionResult(
                ImportedAnimationClipConversionMap.Entry entry,
                bool created,
                int changedEvents,
                int invalidEvents)
            {
                Entry = entry;
                Created = created;
                ChangedEvents = changedEvents;
                InvalidEvents = invalidEvents;
            }

            internal ImportedAnimationClipConversionMap.Entry Entry { get; }
            internal bool Created { get; }
            internal int ChangedEvents { get; }
            internal int InvalidEvents { get; }
        }
    }
}

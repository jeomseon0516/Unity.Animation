using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jeomseon.Animation.Editor.Channels
{
    public sealed class ImportedAnimationClipConversionMap : ScriptableObject
    {
        [SerializeField, FormerlySerializedAs("_entries")] private List<Entry> entries = new();

        internal IReadOnlyList<Entry> Entries => entries;

        internal Entry Find(string sourceGuid, long sourceLocalId)
        {
            return entries.Find(entry =>
                entry.SourceGuid == sourceGuid && entry.SourceLocalId == sourceLocalId);
        }

        internal Entry Record(
            string sourceGuid,
            long sourceLocalId,
            string sourceDependencyHash,
            AnimationClip sourceClip,
            AnimationClip derivedClip)
        {
            Entry entry = Find(sourceGuid, sourceLocalId);
            if (entry == null)
            {
                entry = new Entry();
                entries.Add(entry);
            }

            entry.Set(sourceGuid, sourceLocalId, sourceDependencyHash, sourceClip, derivedClip);
            return entry;
        }

        [Serializable]
        internal sealed class Entry
        {
            [SerializeField, FormerlySerializedAs("_sourceGuid")] private string sourceGuid;
            [SerializeField, FormerlySerializedAs("_sourceLocalId")] private long sourceLocalId;
            [SerializeField, FormerlySerializedAs("_sourceDependencyHash")] private string sourceDependencyHash;
            [SerializeField, FormerlySerializedAs("_sourceClip")] private AnimationClip sourceClip;
            [SerializeField, FormerlySerializedAs("_derivedClip")] private AnimationClip derivedClip;

            internal string SourceGuid => sourceGuid;
            internal long SourceLocalId => sourceLocalId;
            internal string SourceDependencyHash => sourceDependencyHash;
            internal AnimationClip SourceClip => sourceClip;
            internal AnimationClip DerivedClip => derivedClip;

            internal void Set(
                string sourceGuid,
                long sourceLocalId,
                string sourceDependencyHash,
                AnimationClip sourceClip,
                AnimationClip derivedClip)
            {
                this.sourceGuid = sourceGuid;
                this.sourceLocalId = sourceLocalId;
                this.sourceDependencyHash = sourceDependencyHash;
                this.sourceClip = sourceClip;
                this.derivedClip = derivedClip;
            }
        }
    }
}

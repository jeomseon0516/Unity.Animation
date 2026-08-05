using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jeomseon.Animation.Editor.Channels
{
    public sealed class ImportedAnimationClipConversionMap : ScriptableObject
    {
        [SerializeField] private List<Entry> _entries = new();

        internal IReadOnlyList<Entry> Entries => _entries;

        internal Entry Find(string sourceGuid, long sourceLocalId)
        {
            return _entries.Find(entry =>
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
                _entries.Add(entry);
            }

            entry.Set(sourceGuid, sourceLocalId, sourceDependencyHash, sourceClip, derivedClip);
            return entry;
        }

        [Serializable]
        internal sealed class Entry
        {
            [SerializeField] private string _sourceGuid;
            [SerializeField] private long _sourceLocalId;
            [SerializeField] private string _sourceDependencyHash;
            [SerializeField] private AnimationClip _sourceClip;
            [SerializeField] private AnimationClip _derivedClip;

            internal string SourceGuid => _sourceGuid;
            internal long SourceLocalId => _sourceLocalId;
            internal string SourceDependencyHash => _sourceDependencyHash;
            internal AnimationClip SourceClip => _sourceClip;
            internal AnimationClip DerivedClip => _derivedClip;

            internal void Set(
                string sourceGuid,
                long sourceLocalId,
                string sourceDependencyHash,
                AnimationClip sourceClip,
                AnimationClip derivedClip)
            {
                _sourceGuid = sourceGuid;
                _sourceLocalId = sourceLocalId;
                _sourceDependencyHash = sourceDependencyHash;
                _sourceClip = sourceClip;
                _derivedClip = derivedClip;
            }
        }
    }
}

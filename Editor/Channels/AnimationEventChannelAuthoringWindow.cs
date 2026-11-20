using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jeomseon.Unity.Animation.Channels;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Jeomseon.Unity.Animation.Editor.Channels
{
    using LegacyAnimation = UnityEngine.Animation;

    public sealed class AnimationEventChannelAuthoringWindow : EditorWindow
    {
        private const string DefaultChannelFolder = "Assets/AnimationEventChannels";
        private const string DefaultDerivedClipFolder = "Assets/AnimationEventClips";

        [SerializeField, FormerlySerializedAs("_channelFolder")] private DefaultAsset channelFolder;
        [SerializeField, FormerlySerializedAs("_derivedClipFolder")] private DefaultAsset derivedClipFolder;
        [SerializeField, FormerlySerializedAs("_includeInactiveChildren")] private bool includeInactiveChildren = true;
        private Vector2 _scrollPosition;

        private Label _previewTitle;
        private ScrollView _previewScrollView;
        private Button _convertButton;
        private Button _extractButton;

        [MenuItem("Jeomseon/Tool/Animation/Event Channel Authoring")]
        public static void Open()
        {
            AnimationEventChannelAuthoringWindow window =
                GetWindow<AnimationEventChannelAuthoringWindow>("Animation Event Channels");
            window.minSize = new Vector2(520f, 320f);
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            Label title = new("Animation Event Channel Authoring");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(title);
            root.Add(new HelpBox(
                "선택한 클립의 기존 함수명별로 채널 에셋을 만들고 Animation Event에 자동 연결합니다. " +
                "런타임 릴레이 함수명과 Object Parameter 규약을 직접 설정할 필요가 없습니다.",
                HelpBoxMessageType.Info));

            ObjectField channelFolderField = new("Channel Folder")
            {
                objectType = typeof(DefaultAsset),
                allowSceneObjects = false,
                value = channelFolder
            };
            channelFolderField.RegisterValueChangedCallback(evt =>
            {
                channelFolder = evt.newValue as DefaultAsset;
                RefreshPreview();
            });
            root.Add(channelFolderField);

            ObjectField derivedClipFolderField = new("Derived Clip Folder")
            {
                objectType = typeof(DefaultAsset),
                allowSceneObjects = false,
                value = derivedClipFolder
            };
            derivedClipFolderField.RegisterValueChangedCallback(evt =>
            {
                derivedClipFolder = evt.newValue as DefaultAsset;
                RefreshPreview();
            });
            root.Add(derivedClipFolderField);

            Toggle includeInactiveChildrenField = new("Include Inactive Children")
            {
                value = includeInactiveChildren
            };
            includeInactiveChildrenField.RegisterValueChangedCallback(evt =>
            {
                includeInactiveChildren = evt.newValue;
                RefreshPreview();
            });
            root.Add(includeInactiveChildrenField);

            _previewTitle = new Label();
            _previewTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            _previewTitle.style.marginTop = 6f;
            root.Add(_previewTitle);

            _previewScrollView = new ScrollView(ScrollViewMode.Vertical);
            _previewScrollView.style.flexGrow = 1f;
            _previewScrollView.verticalScroller.valueChanged +=
                _ => _scrollPosition = _previewScrollView.scrollOffset;
            _previewScrollView.horizontalScroller.valueChanged +=
                _ => _scrollPosition = _previewScrollView.scrollOffset;
            root.Add(_previewScrollView);

            _convertButton = new Button(() =>
            {
                Convert(GatherSelectedClips(includeInactiveChildren));
                RefreshPreview();
            })
            {
                text = "Convert Selected Events To Channels"
            };
            _convertButton.style.height = 34f;
            root.Add(_convertButton);

            _extractButton = new Button(() =>
            {
                ExtractImportedClips(GatherSelectedClips(includeInactiveChildren));
                RefreshPreview();
            })
            {
                text = "Extract Imported Clips And Create Overrides"
            };
            _extractButton.style.height = 34f;
            root.Add(_extractButton);

            RefreshPreview();
            Vector2 restoredScrollPosition = _scrollPosition;
            _previewScrollView.schedule.Execute(() => _previewScrollView.scrollOffset = restoredScrollPosition);
        }

        // IMGUI OnGUI가 다시 그려질 때마다 미리보기를 다시 계산하던 동작을, 선택·에셋·Hierarchy·포커스
        // 변경 시점의 갱신으로 옮긴다.
        private void OnSelectionChange()
        {
            RefreshPreview();
        }

        private void OnProjectChange()
        {
            RefreshPreview();
        }

        private void OnHierarchyChange()
        {
            RefreshPreview();
        }

        private void OnFocus()
        {
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            if (_previewScrollView == null)
            {
                return;
            }

            IReadOnlyList<AnimationClip> clips = GatherSelectedClips(includeInactiveChildren);
            ImportedAnimationClipConversionMap map = ImportedAnimationClipWorkflow.LoadMap(
                GetDerivedClipFolderPath());
            DrawPreview(clips, map);

            bool hasClips = clips.Count > 0;
            _convertButton.SetEnabled(hasClips);
            _extractButton.SetEnabled(hasClips);
        }

        private void ExtractImportedClips(IReadOnlyList<AnimationClip> clips)
        {
            string channelFolderPath = ResolveChannelFolder();
            string derivedFolder = ResolveDerivedClipFolder();
            ImportedAnimationClipConversionMap map = ImportedAnimationClipWorkflow.LoadOrCreateMap(derivedFolder);
            int extractedClips = 0;
            int updatedClips = 0;
            int changedEvents = 0;
            int invalidEvents = 0;

            foreach (AnimationClip clip in clips.Where(ImportedAnimationClipWorkflow.IsImportedReadOnlyClip))
            {
                ImportedAnimationClipWorkflow.ExtractionResult result = ImportedAnimationClipWorkflow.ExtractOrUpdate(
                    clip,
                    derivedFolder,
                    functionName => LoadOrCreateChannel(channelFolderPath, functionName),
                    map);
                extractedClips += result.Created ? 1 : 0;
                updatedClips += result.Created ? 0 : 1;
                changedEvents += result.ChangedEvents;
                invalidEvents += result.InvalidEvents;
            }

            IReadOnlyList<RuntimeAnimatorController> controllers = GatherSelectedControllers(includeInactiveChildren)
                .Where(controller => ImportedAnimationClipWorkflow.HasReplacement(controller, map.Entries))
                .ToArray();
            foreach (RuntimeAnimatorController controller in controllers)
            {
                ImportedAnimationClipWorkflow.CreateOrUpdateOverrideController(controller, map.Entries, derivedFolder);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Imported Animation Clip Conversion",
                $"Extracted clips: {extractedClips}\n" +
                $"Updated clips: {updatedClips}\n" +
                $"Changed events: {changedEvents}\n" +
                $"Invalid events: {invalidEvents}\n" +
                $"Override controllers: {controllers.Count}",
                "OK");
        }

        private void DrawPreview(
            IReadOnlyList<AnimationClip> clips,
            ImportedAnimationClipConversionMap map)
        {
            _previewTitle.text = $"Preview ({clips.Count} clips)";
            _previewScrollView.Clear();

            foreach (AnimationClip clip in clips)
            {
                AnimationEventChannelMigration.ValidationResult result = AnimationEventChannelMigration.Validate(clip);
                HelpBoxMessageType type = result.InvalidEvents > 0
                    ? HelpBoxMessageType.Error
                    : result.LegacyEvents > 0
                        ? HelpBoxMessageType.Warning
                        : HelpBoxMessageType.Info;

                string importedStatus = GetImportedClipStatus(clip, map);
                _previewScrollView.Add(new HelpBox(
                    $"{clip.name}: conversion candidates {result.LegacyEvents}, " +
                    $"invalid channel events {result.InvalidEvents}{importedStatus}",
                    type));
            }
        }

        private static string GetImportedClipStatus(
            AnimationClip clip,
            ImportedAnimationClipConversionMap map)
        {
            if (!ImportedAnimationClipWorkflow.IsImportedReadOnlyClip(clip))
            {
                return string.Empty;
            }

            ImportedAnimationClipConversionMap.Entry entry = ImportedAnimationClipWorkflow.FindEntry(clip, map);
            if (entry == null)
            {
                return ", imported clip not extracted";
            }

            return ImportedAnimationClipWorkflow.IsOutOfDate(entry)
                ? ", imported source changed; refresh required"
                : ", derived clip ready";
        }

        private void Convert(IReadOnlyList<AnimationClip> clips)
        {
            string folder = ResolveChannelFolder();
            int addedReceivers = AddMissingReceiversToSelectedHierarchies(includeInactiveChildren);
            BatchMigrationResult result = ConvertClips(
                clips,
                clip =>
                {
                    string clipPath = AssetDatabase.GetAssetPath(clip);
                    return string.IsNullOrEmpty(clipPath) ||
                           AssetDatabase.IsOpenForEdit(clip, StatusQueryOptions.UseCachedIfPossible);
                },
                functionName => LoadOrCreateChannel(folder, functionName));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Animation Event Channel Conversion",
                $"Changed clips: {result.ChangedClips}\n" +
                $"Changed events: {result.ChangedEvents}\n" +
                $"Receivers added: {addedReceivers}\n" +
                $"Invalid events: {result.InvalidEvents}\n" +
                $"Read-only clips skipped: {result.SkippedClips}",
                "OK");
        }

        internal static BatchMigrationResult ConvertClips(
            IReadOnlyList<AnimationClip> clips,
            System.Func<AnimationClip, bool> canEdit,
            System.Func<string, AnimationEventChannel> resolveChannel)
        {
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Convert Animation Events To Channels");
            int changedClips = 0;
            int changedEvents = 0;
            int invalidEvents = 0;
            int skippedClips = 0;

            foreach (AnimationClip clip in clips)
            {
                if (!canEdit(clip))
                {
                    skippedClips++;
                    continue;
                }

                AnimationEventChannelMigration.MigrationResult result =
                    AnimationEventChannelMigration.Migrate(clip, resolveChannel);
                if (result.ChangedEvents > 0)
                {
                    changedClips++;
                    changedEvents += result.ChangedEvents;
                }

                invalidEvents += result.InvalidEvents;
            }

            Undo.CollapseUndoOperations(undoGroup);
            return new BatchMigrationResult(changedClips, changedEvents, invalidEvents, skippedClips);
        }

        internal readonly struct BatchMigrationResult
        {
            internal BatchMigrationResult(int changedClips, int changedEvents, int invalidEvents, int skippedClips)
            {
                ChangedClips = changedClips;
                ChangedEvents = changedEvents;
                InvalidEvents = invalidEvents;
                SkippedClips = skippedClips;
            }

            internal int ChangedClips { get; }
            internal int ChangedEvents { get; }
            internal int InvalidEvents { get; }
            internal int SkippedClips { get; }
        }

        private static int AddMissingReceiversToSelectedHierarchies(bool includeInactiveChildren)
        {
            int addedReceivers = 0;
            HashSet<GameObject> visited = new();

            foreach (GameObject selectedRoot in Selection.gameObjects)
            {
                foreach (Transform transform in selectedRoot.GetComponentsInChildren<Transform>(includeInactiveChildren))
                {
                    GameObject gameObject = transform.gameObject;
                    if (!visited.Add(gameObject))
                    {
                        continue;
                    }

                    bool emitsAnimationEvents = gameObject.TryGetComponent<Animator>(out _) ||
                                                gameObject.TryGetComponent<LegacyAnimation>(out _);
                    if (!emitsAnimationEvents || gameObject.TryGetComponent<AnimationEventReceiver>(out _))
                    {
                        continue;
                    }

                    Undo.AddComponent<AnimationEventReceiver>(gameObject);
                    addedReceivers++;
                }
            }

            return addedReceivers;
        }

        private string ResolveChannelFolder()
        {
            string folder = channelFolder == null
                ? DefaultChannelFolder
                : AssetDatabase.GetAssetPath(channelFolder);

            if (!AssetDatabase.IsValidFolder(folder))
            {
                folder = DefaultChannelFolder;
            }

            EnsureFolderExists(folder);
            return folder;
        }

        private string ResolveDerivedClipFolder()
        {
            string folder = GetDerivedClipFolderPath();
            EnsureFolderExists(folder);
            return folder;
        }

        private string GetDerivedClipFolderPath()
        {
            string folder = derivedClipFolder == null
                ? DefaultDerivedClipFolder
                : AssetDatabase.GetAssetPath(derivedClipFolder);
            return AssetDatabase.IsValidFolder(folder) ? folder : DefaultDerivedClipFolder;
        }

        internal static AnimationEventChannel LoadOrCreateChannel(string folder, string functionName)
        {
            string safeName = MakeSafeFileName(functionName);
            string path = $"{folder}/{safeName}AnimationEventChannel.asset";
            AnimationEventChannel channel = AssetDatabase.LoadAssetAtPath<AnimationEventChannel>(path);
            if (channel != null)
            {
                return channel;
            }

            channel = CreateInstance<AnimationEventChannel>();
            channel.name = $"{safeName}AnimationEventChannel";
            AssetDatabase.CreateAsset(channel, path);
            return channel;
        }

        private static string MakeSafeFileName(string value)
        {
            char[] invalidCharacters = Path.GetInvalidFileNameChars();
            return new string(value.Select(character => invalidCharacters.Contains(character) ? '_' : character).ToArray());
        }

        internal static void EnsureFolderExists(string folder)
        {
            string[] segments = folder.Split('/');
            string current = segments[0];

            for (int index = 1; index < segments.Length; index++)
            {
                string next = $"{current}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }

        internal static IReadOnlyList<AnimationClip> GatherSelectedClips(bool includeInactiveChildren)
        {
            HashSet<AnimationClip> clips = new();
            foreach (Object selectedObject in Selection.objects)
            {
                switch (selectedObject)
                {
                    case AnimationClip clip:
                        clips.Add(clip);
                        break;
                    case RuntimeAnimatorController controller:
                        AddControllerClips(controller, clips);
                        break;
                    case GameObject gameObject:
                        AddHierarchyClips(gameObject, includeInactiveChildren, clips);
                        break;
                }
            }

            return clips.Where(clip => clip != null).OrderBy(clip => clip.name).ToArray();
        }

        internal static IReadOnlyList<RuntimeAnimatorController> GatherSelectedControllers(bool includeInactiveChildren)
        {
            HashSet<RuntimeAnimatorController> controllers = new();
            foreach (Object selectedObject in Selection.objects)
            {
                if (selectedObject is RuntimeAnimatorController controller)
                {
                    controllers.Add(controller);
                }
                else if (selectedObject is GameObject gameObject)
                {
                    foreach (Animator animator in gameObject.GetComponentsInChildren<Animator>(includeInactiveChildren))
                    {
                        if (animator.runtimeAnimatorController != null)
                        {
                            controllers.Add(animator.runtimeAnimatorController);
                        }
                    }
                }
            }

            return controllers.OrderBy(controller => controller.name).ToArray();
        }

        private static void AddHierarchyClips(
            GameObject root,
            bool includeInactiveChildren,
            ISet<AnimationClip> clips)
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(includeInactiveChildren))
            {
                if (transform.TryGetComponent(out Animator animator) && animator.runtimeAnimatorController != null)
                {
                    AddControllerClips(animator.runtimeAnimatorController, clips);
                }

                if (!transform.TryGetComponent(out LegacyAnimation legacyAnimation))
                {
                    continue;
                }

                foreach (AnimationState state in legacyAnimation)
                {
                    if (state?.clip != null)
                    {
                        clips.Add(state.clip);
                    }
                }
            }
        }

        private static void AddControllerClips(RuntimeAnimatorController controller, ISet<AnimationClip> clips)
        {
            foreach (AnimationClip clip in controller.animationClips)
            {
                if (clip != null)
                {
                    clips.Add(clip);
                }
            }
        }
    }
}

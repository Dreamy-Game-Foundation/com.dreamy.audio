using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dreamy.Audio;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    internal sealed class DreamyAudioLibraryWindow : EditorWindow
    {
        private const string DefaultOutput = "Assets/Audio";
        private const string DefaultGeneratedOutput = "Assets/DreamyGenerated/Audio";
        private const string PrefPrefix = "Dreamy.Audio.Library.";
        private AudioLibrary library;
        private int tab;
        private string outputFolder = DefaultOutput;
        private string newGroup = "General";
        private string editingGroup;
        private string groupRename;
        private Vector2 scroll;
        private readonly Dictionary<string, bool> groups = new Dictionary<string, bool>();

        [MenuItem("Tools/Dreamy/Audio/Audio Library", priority = 0)]
        internal static void Open()
        {
            var window = GetWindow<DreamyAudioLibraryWindow>("Dreamy Audio");
            window.minSize = new Vector2(520f, 400f);
            window.Show();
        }

        internal static void Open(AudioLibrary target)
        {
            Open();
            var window = GetWindow<DreamyAudioLibraryWindow>();
            window.SetLibrary(target);
            window.Repaint();
        }

        private void OnEnable()
        {
            AudioPreviewSession.Changed += Repaint;
            LoadState();
        }

        private void OnDisable()
        {
            AudioPreviewSession.Changed -= Repaint;
        }

        private void OnGUI()
        {
            DrawHeader();
            if (library == null)
            {
                DrawEmpty();
                return;
            }

            DrawTabs();
            DrawSummary();
            DrawExportAction();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawGroups();
            EditorGUILayout.EndScrollView();
            DrawFooter();
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("DREAMY AUDIO", EditorStyles.boldLabel, GUILayout.Width(125f));
                var next = (AudioLibrary)EditorGUILayout.ObjectField(library, typeof(AudioLibrary), false, GUILayout.MinWidth(200f));
                if (next != library) SetLibrary(next);
                if (GUILayout.Button("New Library", EditorStyles.toolbarButton, GUILayout.Width(82f))) CreateLibrary();

            }
        }

        private void DrawEmpty()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("One place for your game audio", EditorStyles.boldLabel);
            GUILayout.Label("Create or select a Library, then drag Sound and Music clips straight into a group.", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Create Audio Library", GUILayout.Height(30f))) CreateLibrary();
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }

        private void DrawTabs()
        {
            tab = GUILayout.Toolbar(tab, new[] { "Sounds", "Music" }, GUILayout.Height(27f));
        }

        private void DrawSummary()
        {
            var files = CurrentFiles().ToList();
            var valid = files.Count(x => x != null && x.Clips.Count > 0);
            var missing = files.Count - valid;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(tab == 0 ? "Sound Library" : "Music Library", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{valid} ready", EditorStyles.miniLabel);
            if (missing > 0) GUILayout.Label($"{missing} missing", EditorStyles.miniLabel);
            var duplicates = GetDuplicateIds();
            if (duplicates.Count > 0) GUILayout.Label($"⚠ {duplicates.Count} duplicate ID", EditorStyles.miniLabel);
            if (GUILayout.Button("Show all", EditorStyles.miniButton, GUILayout.Width(58f))) SetAllGroups(true);
            if (GUILayout.Button("Hide all", EditorStyles.miniButton, GUILayout.Width(58f))) SetAllGroups(false);
            EditorGUILayout.EndHorizontal();
            var duplicateIds = GetDuplicateIds();
            if (duplicateIds.Count > 0) EditorGUILayout.HelpBox("Duplicate runtime IDs block Export: " + string.Join(", ", duplicateIds), MessageType.Error);
        }

        private void DrawGroups()
        {
            var files = CurrentFiles().Where(x => x != null).ToList();
            var ordered = CurrentCategories().Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            foreach (var file in files)
            {
                if (!ordered.Contains(GroupOf(file))) ordered.Add(GroupOf(file));
            }
            if (ordered.Count == 0) ordered.Add("General");

            foreach (var group in ordered)
            {
                var groupFiles = files.Where(x => GroupOf(x) == group).ToList();
                var key = (tab == 0 ? "S:" : "M:") + group;
                if (!groups.TryGetValue(key, out var open)) open = true;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                using (new EditorGUILayout.HorizontalScope())
                {
                    open = EditorGUILayout.Foldout(open, $"{group}  ({groupFiles.Count})", true);
                    groups[key] = open;
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("+", "Add audio file to this group."), EditorStyles.miniButton, GUILayout.Width(22f))) CreateAudioFile(group);
                    if (GUILayout.Button("Rename", EditorStyles.miniButton, GUILayout.Width(54f)))
                    {
                        editingGroup = group;
                        groupRename = group == "General" ? string.Empty : group;
                    }
                    using (new EditorGUI.DisabledScope(group == "General"))
                    {
                        if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(48f))) DeleteGroup(group);
                    }
                }
                if (editingGroup == group)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        groupRename = EditorGUILayout.TextField(groupRename);
                        if (GUILayout.Button("Save", EditorStyles.miniButton, GUILayout.Width(42f))) RenameGroup(group, groupRename);
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.Width(48f))) editingGroup = null;
                    }
                }
                var drop = GUILayoutUtility.GetRect(1f, open ? 26f : 18f, GUILayout.ExpandWidth(true));
                GUI.Box(drop, open ? "Drop AudioClips, folders, or Dreamy Audio files here" : string.Empty, EditorStyles.centeredGreyMiniLabel);
                HandleDrop(drop, group);
                if (open)
                {
                    foreach (var file in groupFiles) DrawFileRow(file, group);
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void RenameGroup(string oldName, string nextName)
        {
            nextName = nextName == null ? string.Empty : nextName.Trim();
            if (string.IsNullOrEmpty(nextName)) return;
            if (oldName == nextName)
            {
                editingGroup = null;
                return;
            }
            var categories = new SerializedObject(library).FindProperty(tab == 0 ? "soundCategories" : "musicCategories");
            for (var i = 0; i < categories.arraySize; i++)
            {
                if (categories.GetArrayElementAtIndex(i).stringValue == nextName)
                {
                    EditorUtility.DisplayDialog("Group already exists", "Choose a different group name.", "OK");
                    return;
                }
            }

            Undo.RecordObject(library, "Rename Audio Group");
            categories.InsertArrayElementAtIndex(categories.arraySize);
            categories.GetArrayElementAtIndex(categories.arraySize - 1).stringValue = nextName;
            for (var i = categories.arraySize - 2; i >= 0; i--)
            {
                if (categories.GetArrayElementAtIndex(i).stringValue == oldName) categories.DeleteArrayElementAtIndex(i);
            }
            categories.serializedObject.ApplyModifiedProperties();
            foreach (var file in CurrentFiles().Where(x => x != null && GroupOf(x) == oldName)) SetFileGroupWithUndo(file, nextName, "Rename Audio Group");
            EditorUtility.SetDirty(library);
            editingGroup = null;
        }

        private void DeleteGroup(string group)
        {
            if (!EditorUtility.DisplayDialog("Delete Audio Group", "Files in " + group + " will be moved to General. Continue?", "Delete", "Cancel")) return;
            Undo.RecordObject(library, "Delete Audio Group");
            var categories = new SerializedObject(library).FindProperty(tab == 0 ? "soundCategories" : "musicCategories");
            for (var i = categories.arraySize - 1; i >= 0; i--)
            {
                if (categories.GetArrayElementAtIndex(i).stringValue == group) categories.DeleteArrayElementAtIndex(i);
            }
            categories.serializedObject.ApplyModifiedProperties();
            foreach (var file in CurrentFiles().Where(x => x != null && GroupOf(x) == group)) SetFileGroupWithUndo(file, string.Empty, "Delete Audio Group");
            EditorUtility.SetDirty(library);
        }

        private static void SetFileGroupWithUndo(AudioFileObject file, string group, string undoName)
        {
            Undo.RecordObject(file, undoName);
            var serialized = new SerializedObject(file);
            serialized.FindProperty("category").stringValue = group;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(file);
        }

        private void DrawFileRow(AudioFileObject file, string group)
        {
            var row = EditorGUILayout.GetControlRect(false, 24f);
            GUI.Box(row, GUIContent.none, EditorStyles.toolbar);
            var dragRect = new Rect(row.x + 2f, row.y + 2f, 22f, 20f);
            var playRect = new Rect(row.x + 26f, row.y + 2f, 24f, 20f);
            var copyRect = new Rect(row.xMax - 108f, row.y + 2f, 52f, 20f);
            var pingRect = new Rect(row.xMax - 54f, row.y + 2f, 52f, 20f);
            var nameRect = new Rect(row.x + 53f, row.y + 2f, Mathf.Max(80f, copyRect.x - row.x - 58f), 20f);
            GUI.Label(dragRect, new GUIContent("≡", "Drag to reorder within this Library."), EditorStyles.centeredGreyMiniLabel);
            var clip = file.Clips.FirstOrDefault(x => x != null);
            var owns = AudioPreviewSession.Clip == clip;
            if (GUI.Button(playRect, owns && AudioPreviewSession.IsPlaying ? "■" : "▶", EditorStyles.toolbarButton))
            {
                if (owns && AudioPreviewSession.IsPlaying) AudioPreviewSession.Stop(); else AudioPreviewSession.Play(file, clip);
            }
            var id = BuildId(file);
            var duplicate = IsDuplicate(file);
            var content = new GUIContent((duplicate ? "⚠ " : string.Empty) + file.DisplayName, duplicate ? "Duplicate runtime ID: " + id : (clip == null ? "Missing AudioClip" : AssetDatabase.GetAssetPath(clip)));
            if (GUI.Button(nameRect, content, EditorStyles.toolbarButton)) Selection.activeObject = file;
            if (GUI.Button(copyRect, new GUIContent("Copy ID", id), EditorStyles.toolbarButton)) EditorGUIUtility.systemCopyBuffer = id;
            if (GUI.Button(pingRect, "Ping", EditorStyles.toolbarButton)) EditorGUIUtility.PingObject(file);
            HandleRowDrag(row, dragRect, file);
        }

        private void HandleRowDrag(Rect row, Rect dragRect, AudioFileObject target)
        {
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && dragRect.Contains(e.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new UnityEngine.Object[] { target };
                DragAndDrop.StartDrag("Reorder Dreamy Audio");
                e.Use();
                return;
            }

            if (!row.Contains(e.mousePosition) || !TryGetDraggedAudioFile(out var dragged) || dragged == target) return;
            if (e.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                e.Use();
            }
            else if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                MoveBefore(dragged, target);
                e.Use();
            }
        }

        private static bool TryGetDraggedAudioFile(out AudioFileObject file)
        {
            file = DragAndDrop.objectReferences.OfType<AudioFileObject>().FirstOrDefault();
            return file != null;
        }

        private string BuildId(AudioFileObject file)
        {
            return (library != null ? library.LibraryId : string.Empty) + "/" + (file != null ? file.Key : string.Empty);
        }

        private HashSet<string> GetDuplicateIds()
        {
            var seen = new HashSet<string>();
            var duplicates = new HashSet<string>();
            if (library == null) return duplicates;
            foreach (var file in library.EnumerateFiles())
            {
                if (file == null) continue;
                var id = BuildId(file);
                if (!seen.Add(id)) duplicates.Add(id);
            }
            return duplicates;
        }

        private bool IsDuplicate(AudioFileObject file)
        {
            return GetDuplicateIds().Contains(BuildId(file));
        }

        private string MakeUniqueKey(string baseKey)
        {
            baseKey = string.IsNullOrWhiteSpace(baseKey) ? "audio" : baseKey;
            var used = new HashSet<string>(library.EnumerateFiles().Where(x => x != null).Select(x => x.Key));
            if (!used.Contains(baseKey)) return baseKey;
            for (var suffix = 2; ; suffix++)
            {
                var candidate = baseKey + "." + suffix;
                if (!used.Contains(candidate)) return candidate;
            }
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUILayout.HorizontalScope())
            {
                newGroup = EditorGUILayout.TextField(newGroup, GUILayout.MinWidth(160f));
                if (GUILayout.Button("+ Add Group", GUILayout.Width(92f))) AddGroup(newGroup);
                GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();
                outputFolder = EditorGUILayout.TextField("New assets", outputFolder, GUILayout.MinWidth(200f));
                if (EditorGUI.EndChangeCheck()) SaveState();
            }
            EditorGUILayout.EndVertical();
        }

        private IEnumerable<AudioFileObject> CurrentFiles()
        {
            return tab == 0 ? library.Sounds.Cast<AudioFileObject>() : library.Music.Cast<AudioFileObject>();
        }

        private IEnumerable<string> CurrentCategories()
        {
            return tab == 0 ? library.SoundCategories : library.MusicCategories;
        }

        private static string GroupOf(AudioFileObject file) => string.IsNullOrWhiteSpace(file.Category) ? "General" : file.Category;

        private void HandleDrop(Rect rect, string group)
        {
            var e = Event.current;
            if (!rect.Contains(e.mousePosition) || (e.type != EventType.DragUpdated && e.type != EventType.DragPerform)) return;
            var clips = CollectDroppedClips();
            var hasFiles = DragAndDrop.objectReferences.OfType<AudioFileObject>().Any();
            DragAndDrop.visualMode = clips.Count > 0 || hasFiles ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
            if (e.type == EventType.DragPerform && (clips.Count > 0 || hasFiles))
            {
                DragAndDrop.AcceptDrag();
                foreach (var audioFile in DragAndDrop.objectReferences.OfType<AudioFileObject>())
                {
                    SetGroup(audioFile, group);
                    AudioEditorUtility.AddToLibrary(library, audioFile);
                }
                foreach (var clip in clips) CreateAudioFile(group, clip);
                AssetDatabase.SaveAssets();
                Repaint();
            }
            e.Use();
        }

        private static List<AudioClip> CollectDroppedClips()
        {
            var clips = new HashSet<AudioClip>();
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is AudioClip clip) clips.Add(clip);
            }
            foreach (var path in DragAndDrop.paths)
            {
                if (AssetDatabase.IsValidFolder(path))
                {
                    foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { path }))
                    {
                        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
                        if (clip != null) clips.Add(clip);
                    }
                }
                else
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    if (clip != null) clips.Add(clip);
                }
            }
            return clips.ToList();
        }

        private void CreateAudioFile(string group, AudioClip clip = null)
        {
            EnsureFolder(outputFolder);
            var type = tab == 0 ? typeof(SoundAudioFile) : typeof(MusicAudioFile);
            var display = clip == null ? (tab == 0 ? "New Sound" : "New Music") : clip.name;
            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(outputFolder, display + ".asset"));
            var file = (AudioFileObject)CreateInstance(type);
            AssetDatabase.CreateAsset(file, path);
            if (clip != null) AudioEditorUtility.ConfigureFile(file, clip, MakeUniqueKey(AudioEditorUtility.ToKey(clip.name)), group, tab == 0 ? "sfx" : "music", tab == 0 ? AudioEventType.OneShot : AudioEventType.Music);
            else SetGroup(file, group);
            AudioEditorUtility.AddToLibrary(library, file);
            AssetDatabase.SaveAssets();
            Selection.activeObject = file;
        }

        private void CreateLibrary()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Dreamy Audio Library", "DreamyAudioLibrary", "asset", "Choose a location for the Library.");
            if (string.IsNullOrEmpty(path)) return;
            library = CreateInstance<AudioLibrary>();
            AssetDatabase.CreateAsset(library, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = library;
            SaveState();
        }

        private void AddGroup(string group)
        {
            group = string.IsNullOrWhiteSpace(group) ? "General" : group.Trim();
            var so = new SerializedObject(library);
            var list = so.FindProperty(tab == 0 ? "soundCategories" : "musicCategories");
            for (var i = 0; i < list.arraySize; i++) if (list.GetArrayElementAtIndex(i).stringValue == group) return;
            list.InsertArrayElementAtIndex(list.arraySize);
            list.GetArrayElementAtIndex(list.arraySize - 1).stringValue = group;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(library);
        }

        private void SetGroup(AudioFileObject file, string group)
        {
            var so = new SerializedObject(file);
            so.FindProperty("category").stringValue = group == "General" ? string.Empty : group;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(file);
        }

        private void MoveBefore(AudioFileObject source, AudioFileObject target)
        {
            var so = new SerializedObject(library);
            var list = so.FindProperty(tab == 0 ? "sounds" : "music");
            var sourceIndex = -1;
            var targetIndex = -1;
            for (var i = 0; i < list.arraySize; i++)
            {
                var current = list.GetArrayElementAtIndex(i).objectReferenceValue;
                if (current == source) sourceIndex = i;
                if (current == target) targetIndex = i;
            }
            if (sourceIndex < 0 || targetIndex < 0 || sourceIndex == targetIndex) return;
            Undo.RecordObject(library, "Reorder Dreamy Audio");
            list.MoveArrayElement(sourceIndex, sourceIndex < targetIndex ? targetIndex - 1 : targetIndex);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(library);
        }

        private void SetAllGroups(bool value)
        {
            foreach (var group in CurrentCategories().DefaultIfEmpty("General")) groups[(tab == 0 ? "S:" : "M:") + group] = value;
        }

        private void DrawExportAction()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Runtime String IDs", EditorStyles.boldLabel);
            var buttonStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold, fontSize = 12, alignment = TextAnchor.MiddleCenter };
            if (GUILayout.Button(new GUIContent("EXPORT AUDIO LIBRARY IDS", "Writes a runtime-ready C# file under Assets, outside any package asmdef."), buttonStyle, GUILayout.Height(38f))) ExportIds();
            GUILayout.Label("Output: " + DefaultGeneratedOutput + "/DreamyAudioLibraryIds.cs", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void SetLibrary(AudioLibrary target)
        {
            library = target;
            SaveState();
        }

        private void LoadState()
        {
            outputFolder = EditorPrefs.GetString(OutputKey, DefaultOutput);
            if (!outputFolder.StartsWith("Assets", StringComparison.Ordinal)) outputFolder = DefaultOutput;
            var savedGuid = EditorPrefs.GetString(LibraryKey, string.Empty);
            if (!string.IsNullOrEmpty(savedGuid)) library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(AssetDatabase.GUIDToAssetPath(savedGuid));
            if (library != null) return;

            var candidates = AssetDatabase.FindAssets("t:AudioLibrary");
            if (candidates.Length == 0) return;
            Array.Sort(candidates, (a, b) => string.CompareOrdinal(AssetDatabase.GUIDToAssetPath(a), AssetDatabase.GUIDToAssetPath(b)));
            library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(AssetDatabase.GUIDToAssetPath(candidates[0]));
            SaveState();
        }

        private void SaveState()
        {
            EditorPrefs.SetString(OutputKey, string.IsNullOrWhiteSpace(outputFolder) ? DefaultOutput : outputFolder);
            if (library == null)
            {
                EditorPrefs.DeleteKey(LibraryKey);
                return;
            }

            EditorPrefs.SetString(LibraryKey, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(library)));
        }

        private static string LibraryKey => PrefPrefix + Application.dataPath + ".LastLibraryGuid";
        private static string OutputKey => PrefPrefix + Application.dataPath + ".DefaultOutput";

        private void ExportIds()
        {
            if (library == null) return;
            if (!AudioKeyGenerator.TryValidateLibrary(library, out var error))
            {
                EditorUtility.DisplayDialog("Cannot Export Audio IDs", error, "OK");
                return;
            }
            EnsureFolder(DefaultGeneratedOutput);
            var path = DefaultGeneratedOutput + "/DreamyAudioLibraryIds.cs";
            File.WriteAllText(path, AudioKeyGenerator.Generate(library));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var generated = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (generated != null)
            {
                Selection.activeObject = generated;
                EditorGUIUtility.PingObject(generated);
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            var leaf = Path.GetFileName(folder);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}

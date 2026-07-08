using System.IO;
using Dreamy.Audio;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    public sealed class AudioFileWizard : EditorWindow
    {
        private AudioLibrary library;
        private DefaultAsset outputFolder;
        private bool createMusic;
        private string keyPrefix = "ui";
        private string category = "UI";
        private string soundBus = "sfx";
        private string musicBus = "music";
        private Vector2 scroll;

        [MenuItem("Tools/Dreamy/Audio/File Wizard")]
        public static void Open()
        {
            GetWindow<AudioFileWizard>("Audio File Wizard");
        }

        [MenuItem("Assets/Dreamy/Audio/Create Audio Files From Selection")]
        private static void OpenFromSelection()
        {
            Open();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Create Audio File Assets", EditorStyles.boldLabel);
            library = (AudioLibrary)EditorGUILayout.ObjectField("Audio Library", library, typeof(AudioLibrary), false);
            outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder", outputFolder, typeof(DefaultAsset), false);
            createMusic = EditorGUILayout.Toggle("Create Music Files", createMusic);
            keyPrefix = EditorGUILayout.TextField("Key Prefix", keyPrefix);
            category = EditorGUILayout.TextField("Category", category);
            soundBus = EditorGUILayout.TextField("Sound Bus", soundBus);
            musicBus = EditorGUILayout.TextField("Music Bus", musicBus);

            var clips = AudioEditorUtility.GetSelectedClips();
            EditorGUILayout.HelpBox($"Selected AudioClips: {clips.Count}", clips.Count > 0 ? MessageType.Info : MessageType.Warning);

            using (new EditorGUI.DisabledScope(clips.Count == 0))
            {
                if (GUILayout.Button("Create Audio Files"))
                {
                    CreateFiles(clips);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void CreateFiles(System.Collections.Generic.IReadOnlyList<AudioClip> clips)
        {
            var folderPath = outputFolder != null ? AssetDatabase.GetAssetPath(outputFolder) : "Assets/Audio";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            for (var i = 0; i < clips.Count; i++)
            {
                var clip = clips[i];
                var key = string.IsNullOrWhiteSpace(keyPrefix) ? AudioEditorUtility.ToKey(clip.name) : $"{AudioEditorUtility.ToKey(keyPrefix)}.{AudioEditorUtility.ToKey(clip.name)}";
                var fileName = $"{AudioEditorUtility.ToIdentifier(key)}.asset";
                var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, fileName).Replace("\\", "/"));
                AudioFileObject file = createMusic ? CreateInstance<MusicAudioFile>() : CreateInstance<SoundAudioFile>();
                AudioEditorUtility.ConfigureFile(file, clip, key, category, createMusic ? musicBus : soundBus, createMusic ? AudioEventType.Music : AudioEventType.OneShot);
                AssetDatabase.CreateAsset(file, path);
                AudioEditorUtility.AddToLibrary(library, file);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

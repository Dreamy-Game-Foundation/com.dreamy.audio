using System.IO;
using Dreamy.Audio;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    public sealed class DreamyAudioWindow : EditorWindow
    {
        private DreamyAudioProfile profile;
        private DreamyAudioCatalog catalog;
        private AudioLibrary library;
        private Vector2 scroll;

        [MenuItem("Tools/Dreamy/Audio")]
        public static void Open()
        {
            GetWindow<DreamyAudioWindow>("Dreamy Audio");
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            profile = (DreamyAudioProfile)EditorGUILayout.ObjectField("Profile", profile, typeof(DreamyAudioProfile), false);
            catalog = (DreamyAudioCatalog)EditorGUILayout.ObjectField("Catalog", catalog, typeof(DreamyAudioCatalog), false);
            library = (AudioLibrary)EditorGUILayout.ObjectField("Library", library, typeof(AudioLibrary), false);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Profile"))
                {
                    CreateAsset<DreamyAudioProfile>("DreamyAudioProfile.asset");
                }

                if (GUILayout.Button("Create Catalog"))
                {
                    CreateAsset<DreamyAudioCatalog>("DreamyAudioCatalog.asset");
                }

                if (GUILayout.Button("Create Library"))
                {
                    CreateAsset<AudioLibrary>("DreamyAudioLibrary.asset");
                }
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open File Wizard"))
                {
                    AudioFileWizard.Open();
                }

                if (GUILayout.Button("Open Playback Tool"))
                {
                    AudioPlaybackTool.Open();
                }
            }

            if (GUILayout.Button("Validate Profile") && profile != null)
            {
                var issues = AudioCatalogValidator.Validate(profile);
                for (var i = 0; i < issues.Count; i++)
                {
                    var issue = issues[i];
                    var type = issue.Severity == AudioValidationSeverity.Error ? LogType.Error : LogType.Warning;
                    Debug.unityLogger.Log(type, issue.Code, issue.Message);
                }

                if (issues.Count == 0)
                {
                    Debug.Log("Dreamy Audio validation passed.");
                }
            }

            if (GUILayout.Button("Generate Keys") && catalog != null)
            {
                var defaultPath = $"Assets/Generated/DreamyAudio/{catalog.name}AudioKeys.cs";
                var path = EditorUtility.SaveFilePanelInProject("Generate Audio Keys", Path.GetFileName(defaultPath), "cs", "Choose generated key output path.", Path.GetDirectoryName(defaultPath));
                if (!string.IsNullOrEmpty(path))
                {
                    var directory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(path, AudioKeyGenerator.Generate(catalog));
                    AssetDatabase.Refresh();
                }
            }

            if (GUILayout.Button("Generate Library Enums") && library != null)
            {
                var defaultPath = $"Assets/Generated/DreamyAudio/{library.name}AudioLibraryKeys.cs";
                var path = EditorUtility.SaveFilePanelInProject("Generate Audio Library Keys", Path.GetFileName(defaultPath), "cs", "Choose generated key output path.", Path.GetDirectoryName(defaultPath));
                if (!string.IsNullOrEmpty(path))
                {
                    var directory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(path, AudioKeyGenerator.Generate(library));
                    AssetDatabase.Refresh();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private static void CreateAsset<T>(string fileName) where T : ScriptableObject
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Dreamy Audio Asset", fileName, "asset", "Choose asset location.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }
    }
}

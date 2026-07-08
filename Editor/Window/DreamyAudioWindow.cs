using System.IO;
using Dreamy.Audio;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    public sealed class DreamyAudioWindow : EditorWindow
    {
        private const string MenuRoot = "Tools/Dreamy/Audio/";

        private DreamyAudioProfile profile;
        private DreamyAudioCatalog catalog;
        private AudioLibrary library;
        private Vector2 scroll;

        [MenuItem(MenuRoot + "Open Window")]
        public static void Open()
        {
            GetWindow<DreamyAudioWindow>("Dreamy Audio");
        }

        [MenuItem(MenuRoot + "Create/Profile")]
        public static void CreateProfile()
        {
            CreateAsset<DreamyAudioProfile>("DreamyAudioProfile.asset");
        }

        [MenuItem(MenuRoot + "Create/Catalog")]
        public static void CreateCatalog()
        {
            CreateAsset<DreamyAudioCatalog>("DreamyAudioCatalog.asset");
        }

        [MenuItem(MenuRoot + "Create/Library")]
        public static void CreateLibrary()
        {
            CreateAsset<AudioLibrary>("DreamyAudioLibrary.asset");
        }

        [MenuItem(MenuRoot + "Validate Selected")]
        public static void ValidateSelected()
        {
            if (Selection.activeObject is DreamyAudioProfile selectedProfile)
            {
                LogValidation(AudioCatalogValidator.Validate(selectedProfile));
                return;
            }

            Debug.LogWarning("Select a DreamyAudioProfile to validate.");
        }

        [MenuItem(MenuRoot + "Generate Keys From Selected Catalog")]
        public static void GenerateKeysFromSelectedCatalog()
        {
            if (Selection.activeObject is DreamyAudioCatalog selectedCatalog)
            {
                SaveGeneratedFile($"{selectedCatalog.name}AudioKeys.cs", AudioKeyGenerator.Generate(selectedCatalog));
                return;
            }

            Debug.LogWarning("Select a DreamyAudioCatalog to generate keys.");
        }

        [MenuItem(MenuRoot + "Generate Keys From Selected Library")]
        public static void GenerateKeysFromSelectedLibrary()
        {
            if (Selection.activeObject is AudioLibrary selectedLibrary)
            {
                SaveGeneratedFile($"{selectedLibrary.name}AudioLibraryKeys.cs", AudioKeyGenerator.Generate(selectedLibrary));
                return;
            }

            Debug.LogWarning("Select an AudioLibrary to generate library keys.");
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
                LogValidation(AudioCatalogValidator.Validate(profile));
            }

            if (GUILayout.Button("Generate Keys") && catalog != null)
            {
                SaveGeneratedFile($"{catalog.name}AudioKeys.cs", AudioKeyGenerator.Generate(catalog));
            }

            if (GUILayout.Button("Generate Library Enums") && library != null)
            {
                SaveGeneratedFile($"{library.name}AudioLibraryKeys.cs", AudioKeyGenerator.Generate(library));
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

        private static void SaveGeneratedFile(string fileName, string contents)
        {
            var defaultPath = $"Assets/Generated/DreamyAudio/{fileName}";
            var path = EditorUtility.SaveFilePanelInProject("Generate Dreamy Audio Keys", Path.GetFileName(defaultPath), "cs", "Choose generated key output path.", Path.GetDirectoryName(defaultPath));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, contents);
            AssetDatabase.Refresh();
        }

        private static void LogValidation(System.Collections.Generic.IReadOnlyList<AudioValidationIssue> issues)
        {
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
    }
}

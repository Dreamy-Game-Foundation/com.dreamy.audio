using Dreamy.Audio;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    internal static class DreamyAudioAssetMenu
    {
        [MenuItem("Tools/Dreamy/Audio/Create/Profile", priority = 10)]
        private static void CreateProfile()
        {
            CreateAsset<DreamyAudioProfile>("DreamyAudioProfile.asset");
        }

        [MenuItem("Tools/Dreamy/Audio/Create/Library", priority = 11)]
        private static void CreateLibrary()
        {
            CreateAsset<AudioLibrary>("DreamyAudioLibrary.asset");
        }

        private static void CreateAsset<T>(string defaultName) where T : ScriptableObject
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Dreamy Audio Asset", defaultName, "asset", "Choose an asset location.");
            if (string.IsNullOrEmpty(path)) return;
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }
    }
}

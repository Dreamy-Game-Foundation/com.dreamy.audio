using System.Collections.Generic;
using System.IO;
using Dreamy.Audio;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    internal static class AudioEditorUtility
    {
        public static string ToIdentifier(string key)
        {
            return AudioKeyGenerator.ToIdentifier(key);
        }

        public static string ToKey(string name)
        {
            var sb = new System.Text.StringBuilder();
            for (var i = 0; i < name.Length; i++)
            {
                var c = char.ToLowerInvariant(name[i]);
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                    continue;
                }

                if (sb.Length > 0 && sb[sb.Length - 1] != '.')
                {
                    sb.Append('.');
                }
            }

            return sb.ToString().Trim('.');
        }

        public static List<AudioClip> GetSelectedClips()
        {
            var clips = new List<AudioClip>();
            foreach (var obj in Selection.objects)
            {
                if (obj is AudioClip clip)
                {
                    AddUnique(clips, clip);
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (Directory.Exists(path))
                {
                    var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { path });
                    for (var i = 0; i < guids.Length; i++)
                    {
                        AddUnique(clips, AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guids[i])));
                    }
                }
            }

            return clips;
        }

        public static void AddToLibrary(AudioLibrary library, AudioFileObject file)
        {
            if (library == null || file == null)
            {
                return;
            }

            var serialized = new SerializedObject(library);
            var property = file is MusicAudioFile ? serialized.FindProperty("music") : serialized.FindProperty("sounds");
            for (var i = 0; i < property.arraySize; i++)
            {
                if (property.GetArrayElementAtIndex(i).objectReferenceValue == file)
                {
                    return;
                }
            }

            property.InsertArrayElementAtIndex(property.arraySize);
            property.GetArrayElementAtIndex(property.arraySize - 1).objectReferenceValue = file;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(library);
        }

        public static void ConfigureFile(AudioFileObject file, AudioClip clip, string key, string category, string bus, AudioEventType eventType)
        {
            var serialized = new SerializedObject(file);
            serialized.FindProperty("key").stringValue = key;
            serialized.FindProperty("displayName").stringValue = ObjectNames.NicifyVariableName(Path.GetFileNameWithoutExtension(clip.name));
            serialized.FindProperty("category").stringValue = category ?? string.Empty;
            serialized.FindProperty("bus").stringValue = bus;
            serialized.FindProperty("eventType").enumValueIndex = (int)eventType;
            serialized.FindProperty("loop").boolValue = eventType == AudioEventType.Music || eventType == AudioEventType.Ambience || eventType == AudioEventType.Loop;
            var clips = serialized.FindProperty("clips");
            clips.ClearArray();
            clips.InsertArrayElementAtIndex(0);
            clips.GetArrayElementAtIndex(0).objectReferenceValue = clip;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(file);
        }

        private static void AddUnique(List<AudioClip> clips, AudioClip clip)
        {
            if (clip != null && !clips.Contains(clip))
            {
                clips.Add(clip);
            }
        }
    }
}

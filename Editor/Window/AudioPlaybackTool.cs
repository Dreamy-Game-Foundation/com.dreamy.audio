using System;
using System.Reflection;
using Dreamy.Audio;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    public sealed class AudioPlaybackTool : EditorWindow
    {
        private const string MissingEditorPreviewApiMessage = "Editor preview API is unavailable. Runtime playback still works in Play Mode.";

        private DreamyAudioProfile profile;
        private DreamyAudioCatalog catalog;
        private AudioLibrary library;
        private AudioFileObject directFile;
        private AudioKey key;
        private AudioHandle lastHandle;
        private string search;
        private bool loopEditorPreview;
        private bool showSounds = true;
        private bool showMusic = true;
        private Vector2 scroll;

        [MenuItem("Tools/Dreamy/Audio/Playback Tool")]
        public static void Open()
        {
            GetWindow<AudioPlaybackTool>("Audio Playback");
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawConfiguration();
            DrawRuntimeControls();
            DrawEditorPreviewControls();
            DrawDirectFileControls();
            DrawKeyControls();
            DrawLibraryControls();
            DrawStopControls();
            EditorGUILayout.EndScrollView();
        }

        private void OnDisable()
        {
            AudioPreviewUtility.StopAll();
        }

        private void DrawConfiguration()
        {
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            profile = (DreamyAudioProfile)EditorGUILayout.ObjectField("Profile", profile, typeof(DreamyAudioProfile), false);
            catalog = (DreamyAudioCatalog)EditorGUILayout.ObjectField("Catalog", catalog, typeof(DreamyAudioCatalog), false);
            library = (AudioLibrary)EditorGUILayout.ObjectField("Library", library, typeof(AudioLibrary), false);
        }

        private void DrawRuntimeControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Playback", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(Application.isPlaying ? "Runtime playback uses DreamyAudio service." : "Enter Play Mode to test mixer, bus, cooldown, max instance, and transitions.", MessageType.Info);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || profile == null))
            {
                if (GUILayout.Button(DreamyAudio.IsInitialized ? "Reinitialize Profile" : "Initialize Profile"))
                {
                    DreamyAudio.Initialize(profile);
                }
            }
        }

        private void DrawEditorPreviewControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor Preview", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Toggle("Preview API Available", AudioPreviewUtility.IsAvailable);
            }

            if (!AudioPreviewUtility.IsAvailable)
            {
                EditorGUILayout.HelpBox(MissingEditorPreviewApiMessage, MessageType.Warning);
            }

            loopEditorPreview = EditorGUILayout.Toggle("Loop Preview", loopEditorPreview);
        }

        private void DrawDirectFileControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Play By File", EditorStyles.boldLabel);
            directFile = (AudioFileObject)EditorGUILayout.ObjectField("Audio File", directFile, typeof(AudioFileObject), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(directFile == null || !AudioPreviewUtility.IsAvailable))
                {
                    if (GUILayout.Button("Preview File"))
                    {
                        PreviewFile(directFile);
                    }
                }

                using (new EditorGUI.DisabledScope(!Application.isPlaying || directFile == null))
                {
                    if (GUILayout.Button("Play Runtime"))
                    {
                        PlayRuntime(directFile);
                    }
                }
            }
        }

        private void DrawKeyControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Play By Key", EditorStyles.boldLabel);
            key = DrawAudioKey(key);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!key.IsValid || !AudioPreviewUtility.IsAvailable))
                {
                    if (GUILayout.Button("Preview Key"))
                    {
                        if (TryResolveFile(key, out var file))
                        {
                            PreviewFile(file);
                        }
                        else
                        {
                            Debug.LogWarning($"Audio key '{key}' was not found in selected catalog/library.");
                        }
                    }
                }

                using (new EditorGUI.DisabledScope(!Application.isPlaying || !key.IsValid))
                {
                    if (GUILayout.Button("Play Runtime"))
                    {
                        var result = DreamyAudio.Play(key);
                        lastHandle = result.Handle;
                        Debug.Log(result.Succeeded ? $"Played {key}" : result.Message);
                    }
                }
            }
        }

        private void DrawLibraryControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Play From Library", EditorStyles.boldLabel);
            search = EditorGUILayout.TextField("Search", search);
            showSounds = EditorGUILayout.Foldout(showSounds, "Sounds", true);
            if (showSounds && library != null)
            {
                for (var i = 0; i < library.Sounds.Count; i++)
                {
                    DrawFileRow(library.Sounds[i]);
                }
            }

            showMusic = EditorGUILayout.Foldout(showMusic, "Music", true);
            if (showMusic && library != null)
            {
                for (var i = 0; i < library.Music.Count; i++)
                {
                    DrawFileRow(library.Music[i]);
                }
            }
        }

        private void DrawStopControls()
        {
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!AudioPreviewUtility.IsAvailable))
                {
                    if (GUILayout.Button("Stop Editor Preview"))
                    {
                        AudioPreviewUtility.StopAll();
                    }
                }

                using (new EditorGUI.DisabledScope(!Application.isPlaying || !lastHandle.IsValid))
                {
                    if (GUILayout.Button("Stop Runtime Last"))
                    {
                        DreamyAudio.Stop(lastHandle, new AudioTransition(0.2f));
                        lastHandle = default;
                    }
                }
            }
        }

        private void DrawFileRow(AudioFileObject file)
        {
            if (!ShouldDraw(file))
            {
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.ObjectField(file, typeof(AudioFileObject), false);
                EditorGUILayout.LabelField(file.Key, GUILayout.MinWidth(120));

                using (new EditorGUI.DisabledScope(!AudioPreviewUtility.IsAvailable))
                {
                    if (GUILayout.Button("Preview", GUILayout.Width(72)))
                    {
                        PreviewFile(file);
                    }
                }

                using (new EditorGUI.DisabledScope(!Application.isPlaying))
                {
                    if (GUILayout.Button("Runtime", GUILayout.Width(72)))
                    {
                        PlayRuntime(file);
                    }
                }
            }
        }

        private bool ShouldDraw(AudioFileObject file)
        {
            if (file == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            return file.Key.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0
                || file.DisplayName.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0
                || file.Category.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void PreviewFile(AudioFileObject file)
        {
            if (file == null)
            {
                Debug.LogWarning("Audio preview skipped because file is null.");
                return;
            }

            var clip = file.SelectClip();
            if (!clip)
            {
                Debug.LogWarning($"Audio file '{file.name}' has no clip to preview.");
                return;
            }

            AudioPreviewUtility.Play(clip, loopEditorPreview || file.Loop);
            Debug.Log($"Previewing {file.Key}: {clip.name}");
        }

        private void PlayRuntime(AudioFileObject file)
        {
            var result = file is MusicAudioFile music ? AudioPlayResult.Played(DreamyAudio.PlayMusic(music)) : DreamyAudio.Play(file);
            lastHandle = result.Handle;
            Debug.Log(result.Succeeded ? $"Played {file.Key}" : result.Message);
        }

        private bool TryResolveFile(AudioKey audioKey, out AudioFileObject file)
        {
            if (catalog != null && (string.IsNullOrEmpty(audioKey.CatalogId) || catalog.CatalogId == audioKey.CatalogId))
            {
                if (catalog.TryGetFile(audioKey.Key, out file))
                {
                    return true;
                }
            }

            if (library != null && (string.IsNullOrEmpty(audioKey.CatalogId) || library.LibraryId == audioKey.CatalogId))
            {
                return library.TryGetFile(audioKey.Key, out file);
            }

            file = null;
            return false;
        }

        private static AudioKey DrawAudioKey(AudioKey value)
        {
            var catalog = EditorGUILayout.TextField("Catalog", value.CatalogId);
            var keyValue = EditorGUILayout.TextField("Key", value.Key);
            return new AudioKey(catalog, keyValue);
        }

        private static class AudioPreviewUtility
        {
            private static readonly Type AudioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            private static readonly MethodInfo PlayPreviewClipMethod = FindMethod("PlayPreviewClip", typeof(AudioClip), typeof(int), typeof(bool));
            private static readonly MethodInfo StopAllPreviewClipsMethod = FindMethod("StopAllPreviewClips");
            private static readonly MethodInfo IsPreviewClipPlayingMethod = FindMethod("IsPreviewClipPlaying");

            public static bool IsAvailable => AudioUtilType != null && PlayPreviewClipMethod != null && StopAllPreviewClipsMethod != null;

            public static bool IsPlaying
            {
                get
                {
                    if (IsPreviewClipPlayingMethod == null)
                    {
                        return false;
                    }

                    return (bool)IsPreviewClipPlayingMethod.Invoke(null, null);
                }
            }

            public static void Play(AudioClip clip, bool loop)
            {
                if (!clip)
                {
                    Debug.LogWarning("Audio preview skipped because clip is null.");
                    return;
                }

                if (!IsAvailable)
                {
                    Debug.LogWarning("Unity AudioUtil preview API is unavailable in this editor version.");
                    return;
                }

                StopAll();
                PlayPreviewClipMethod.Invoke(null, new object[] { clip, 0, loop });
            }

            public static void StopAll()
            {
                if (StopAllPreviewClipsMethod != null)
                {
                    StopAllPreviewClipsMethod.Invoke(null, null);
                }
            }

            private static MethodInfo FindMethod(string methodName, params Type[] parameterTypes)
            {
                if (AudioUtilType == null)
                {
                    return null;
                }

                return AudioUtilType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);
            }
        }
    }
}

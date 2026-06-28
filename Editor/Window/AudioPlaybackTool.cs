using Dreamy.Audio;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    public sealed class AudioPlaybackTool : EditorWindow
    {
        private DreamyAudioProfile profile;
        private AudioLibrary library;
        private AudioFileObject directFile;
        private AudioKey key;
        private AudioHandle lastHandle;
        private Vector2 scroll;

        [MenuItem("Tools/Dreamy/Audio Playback Tool")]
        public static void Open()
        {
            GetWindow<AudioPlaybackTool>("Audio Playback");
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Runtime Playback", EditorStyles.boldLabel);
            profile = (DreamyAudioProfile)EditorGUILayout.ObjectField("Profile", profile, typeof(DreamyAudioProfile), false);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || profile == null))
            {
                if (GUILayout.Button(DreamyAudio.IsInitialized ? "Reinitialize Profile" : "Initialize Profile"))
                {
                    DreamyAudio.Initialize(profile);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Play By File", EditorStyles.boldLabel);
            directFile = (AudioFileObject)EditorGUILayout.ObjectField("Audio File", directFile, typeof(AudioFileObject), false);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || directFile == null))
            {
                if (GUILayout.Button("Play File"))
                {
                    var result = directFile is MusicAudioFile music ? AudioPlayResult.Played(DreamyAudio.PlayMusic(music)) : DreamyAudio.Play(directFile);
                    lastHandle = result.Handle;
                    Debug.Log(result.Succeeded ? $"Played {directFile.name}" : result.Message);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Play From Library", EditorStyles.boldLabel);
            library = (AudioLibrary)EditorGUILayout.ObjectField("Library", library, typeof(AudioLibrary), false);
            if (library != null)
            {
                DrawLibrary(library);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Play By Key", EditorStyles.boldLabel);
            key = DrawAudioKey(key);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || !key.IsValid))
            {
                if (GUILayout.Button("Play Key"))
                {
                    var result = DreamyAudio.Play(key);
                    lastHandle = result.Handle;
                    Debug.Log(result.Succeeded ? $"Played {key}" : result.Message);
                }
            }

            using (new EditorGUI.DisabledScope(!Application.isPlaying || !lastHandle.IsValid))
            {
                if (GUILayout.Button("Stop Last"))
                {
                    DreamyAudio.Stop(lastHandle, new AudioTransition(0.2f));
                    lastHandle = default;
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawLibrary(AudioLibrary audioLibrary)
        {
            EditorGUILayout.LabelField("Sounds", EditorStyles.miniBoldLabel);
            for (var i = 0; i < audioLibrary.Sounds.Count; i++)
            {
                DrawFileButton(audioLibrary.Sounds[i]);
            }

            EditorGUILayout.LabelField("Music", EditorStyles.miniBoldLabel);
            for (var i = 0; i < audioLibrary.Music.Count; i++)
            {
                DrawFileButton(audioLibrary.Music[i]);
            }
        }

        private void DrawFileButton(AudioFileObject file)
        {
            if (file == null)
            {
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.ObjectField(file, typeof(AudioFileObject), false);
                using (new EditorGUI.DisabledScope(!Application.isPlaying))
                {
                    if (GUILayout.Button("Play", GUILayout.Width(80)))
                    {
                        var result = file is MusicAudioFile music ? AudioPlayResult.Played(DreamyAudio.PlayMusic(music)) : DreamyAudio.Play(file);
                        lastHandle = result.Handle;
                        Debug.Log(result.Succeeded ? $"Played {file.Key}" : result.Message);
                    }
                }
            }
        }

        private static AudioKey DrawAudioKey(AudioKey value)
        {
            var catalog = EditorGUILayout.TextField("Catalog", value.CatalogId);
            var keyValue = EditorGUILayout.TextField("Key", value.Key);
            return new AudioKey(catalog, keyValue);
        }
    }
}

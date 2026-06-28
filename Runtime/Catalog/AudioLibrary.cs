using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Audio
{
    [CreateAssetMenu(menuName = "Dreamy/Audio/Library", fileName = "DreamyAudioLibrary")]
    public sealed class AudioLibrary : ScriptableObject
    {
        [SerializeField] private string libraryId = "core";
        [SerializeField] private string generatedNamespace = "Dreamy.Audio.Generated";
        [SerializeField] private string soundEnumName = "SoundKeys";
        [SerializeField] private string musicEnumName = "MusicKeys";
        [SerializeField] private List<string> soundCategories = new List<string>();
        [SerializeField] private List<string> musicCategories = new List<string>();
        [SerializeField] private List<SoundAudioFile> sounds = new List<SoundAudioFile>();
        [SerializeField] private List<MusicAudioFile> music = new List<MusicAudioFile>();

        public string LibraryId => libraryId;
        public string GeneratedNamespace => generatedNamespace;
        public string SoundEnumName => string.IsNullOrWhiteSpace(soundEnumName) ? "SoundKeys" : soundEnumName;
        public string MusicEnumName => string.IsNullOrWhiteSpace(musicEnumName) ? "MusicKeys" : musicEnumName;
        public IReadOnlyList<string> SoundCategories => soundCategories;
        public IReadOnlyList<string> MusicCategories => musicCategories;
        public IReadOnlyList<SoundAudioFile> Sounds => sounds;
        public IReadOnlyList<MusicAudioFile> Music => music;

        public bool TryGetSound(string key, out SoundAudioFile file)
        {
            file = Find(sounds, key);
            return file != null;
        }

        public bool TryGetMusic(string key, out MusicAudioFile file)
        {
            file = Find(music, key);
            return file != null;
        }

        public bool TryGetFile(string key, out AudioFileObject file)
        {
            if (TryGetSound(key, out var sound))
            {
                file = sound;
                return true;
            }

            if (TryGetMusic(key, out var musicFile))
            {
                file = musicFile;
                return true;
            }

            file = null;
            return false;
        }

        public IEnumerable<AudioFileObject> EnumerateFiles()
        {
            for (var i = 0; i < sounds.Count; i++)
            {
                if (sounds[i] != null)
                {
                    yield return sounds[i];
                }
            }

            for (var i = 0; i < music.Count; i++)
            {
                if (music[i] != null)
                {
                    yield return music[i];
                }
            }
        }

        private static T Find<T>(IReadOnlyList<T> files, string key) where T : AudioFileObject
        {
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (file != null && file.Key == key)
                {
                    return file;
                }
            }

            return null;
        }
    }
}

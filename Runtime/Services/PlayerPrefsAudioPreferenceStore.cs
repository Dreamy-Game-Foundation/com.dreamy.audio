using UnityEngine;

namespace Dreamy.Audio
{
    public sealed class PlayerPrefsAudioPreferenceStore : IAudioPreferenceStore
    {
        public bool TryGetFloat(string key, out float value)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                value = default;
                return false;
            }

            value = PlayerPrefs.GetFloat(key);
            return true;
        }

        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }
    }
}

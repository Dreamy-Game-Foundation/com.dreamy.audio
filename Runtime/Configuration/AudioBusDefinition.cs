using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Dreamy.Audio
{
    [Serializable]
    public sealed class AudioBusDefinition
    {
        [SerializeField] private string id = "sfx";
        [SerializeField] private string displayName = "SFX";
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField] private string exposedVolumeParameter;
        [SerializeField, Range(0f, 1f)] private float defaultVolume = 1f;
        [SerializeField] private bool persistVolume = true;
        [SerializeField] private string preferenceKey;
        [SerializeField, Min(1)] private int voiceBudget = 32;

        public AudioBusId Id => new AudioBusId(id);
        public string DisplayName => displayName;
        public AudioMixerGroup MixerGroup => mixerGroup;
        public string ExposedVolumeParameter => exposedVolumeParameter;
        public float DefaultVolume => defaultVolume;
        public bool PersistVolume => persistVolume;
        public string PreferenceKey => string.IsNullOrWhiteSpace(preferenceKey) ? $"dreamy.audio.volume.{id}" : preferenceKey;
        public int VoiceBudget => Mathf.Max(1, voiceBudget);
    }
}

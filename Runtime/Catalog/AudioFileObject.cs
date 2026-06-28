using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Dreamy.Audio
{
    public abstract class AudioFileObject : ScriptableObject
    {
        [SerializeField] private string key;
        [SerializeField] private string displayName;
        [SerializeField] private string category;
        [SerializeField] private string bus = "sfx";
        [SerializeField] private AudioEventType eventType = AudioEventType.OneShot;
        [SerializeField] private AudioVariantSelectionMode selectionMode = AudioVariantSelectionMode.Random;
        [SerializeField] private List<AudioClip> clips = new List<AudioClip>();
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField, Range(-3f, 3f)] private float pitch = 1f;
        [SerializeField, Range(0f, 0.5f)] private float randomPitch = 0.05f;
        [SerializeField, Min(0f)] private float fadeInSeconds;
        [SerializeField, Min(0f)] private float fadeOutSeconds;
        [SerializeField] private bool loop;
        [SerializeField] private bool neverRepeat;
        [SerializeField] private AudioTimeMode timeMode = AudioTimeMode.Scaled;
        [SerializeField, Range(0, 256)] private int priority = 128;
        [SerializeField, Min(0)] private int maxInstances = 10;
        [SerializeField, Min(0f)] private float cooldownSeconds;
        [SerializeField] private AudioSpatialSettings spatial = new AudioSpatialSettings();
        [SerializeField] private AudioMixerGroup mixerGroupOverride;
        [SerializeField] private bool bypassEffects;
        [SerializeField] private bool bypassListenerEffects;
        [SerializeField] private bool bypassReverbZones;

        private int lastClipIndex = -1;

        public string Key => string.IsNullOrWhiteSpace(key) ? name : key;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? Key : displayName;
        public string Category => category ?? string.Empty;
        public AudioBusId Bus => new AudioBusId(bus);
        public AudioEventType EventType => eventType;
        public AudioVariantSelectionMode SelectionMode => selectionMode;
        public IReadOnlyList<AudioClip> Clips => clips;
        public float Volume => Mathf.Clamp01(volume);
        public float Pitch => Mathf.Clamp(pitch, -3f, 3f);
        public float RandomPitch => Mathf.Clamp(randomPitch, 0f, 0.5f);
        public float FadeInSeconds => Mathf.Max(0f, fadeInSeconds);
        public float FadeOutSeconds => Mathf.Max(0f, fadeOutSeconds);
        public bool Loop => loop || eventType == AudioEventType.Loop || eventType == AudioEventType.Music || eventType == AudioEventType.Ambience;
        public bool NeverRepeat => neverRepeat;
        public AudioTimeMode TimeMode => timeMode;
        public int Priority => priority;
        public int MaxInstances => Mathf.Max(0, maxInstances);
        public float CooldownSeconds => Mathf.Max(0f, cooldownSeconds);
        public AudioSpatialSettings Spatial => spatial;
        public AudioMixerGroup MixerGroupOverride => mixerGroupOverride;
        public bool BypassEffects => bypassEffects;
        public bool BypassListenerEffects => bypassListenerEffects;
        public bool BypassReverbZones => bypassReverbZones;

        public AudioClip SelectClip()
        {
            if (clips == null || clips.Count == 0)
            {
                return null;
            }

            if (selectionMode == AudioVariantSelectionMode.Sequential)
            {
                lastClipIndex = (lastClipIndex + 1) % clips.Count;
                return clips[lastClipIndex];
            }

            var next = Random.Range(0, clips.Count);
            if (neverRepeat && clips.Count > 1 && next == lastClipIndex)
            {
                next = (next + 1) % clips.Count;
            }

            lastClipIndex = next;
            return clips[next];
        }

        public AudioEventDefinition ToEventDefinition()
        {
            return AudioEventDefinition.FromAudioFile(this);
        }
    }
}

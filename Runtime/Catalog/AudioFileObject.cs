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
        [SerializeField] private List<float> clipWeights = new List<float>();
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField, Range(-3f, 3f)] private float pitch = 1f;
        [SerializeField, Range(0f, 0.5f)] private float randomPitch = 0.05f;
        [SerializeField, Min(0f)] private float fadeInSeconds;
        [SerializeField, Min(0f)] private float fadeOutSeconds;
        [SerializeField, Min(0f), Tooltip("Non-destructive playback start. The source AudioClip is never edited.")] private float startSeconds;
        [SerializeField, Min(0f), Tooltip("Non-destructive playback end. Zero plays to the end of the clip.")] private float endSeconds;
        [SerializeField] private bool loop;
        [SerializeField] private AudioLoopMode loopMode = AudioLoopMode.WholeClip;
        [SerializeField, Min(0f)] private float loopStartSeconds;
        [SerializeField, Min(0f)] private float loopEndSeconds;
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
        public IReadOnlyList<float> ClipWeights => clipWeights;
        public float Volume => Mathf.Clamp01(volume);
        public float Pitch => Mathf.Clamp(pitch, -3f, 3f);
        public float RandomPitch => Mathf.Clamp(randomPitch, 0f, 0.5f);
        public float FadeInSeconds => Mathf.Max(0f, fadeInSeconds);
        public float FadeOutSeconds => Mathf.Max(0f, fadeOutSeconds);
        public float StartSeconds => Mathf.Max(0f, startSeconds);
        public float EndSeconds => Mathf.Max(0f, endSeconds);
        public bool Loop => loop || eventType == AudioEventType.Loop || eventType == AudioEventType.Music || eventType == AudioEventType.Ambience;
        public AudioLoopMode LoopMode => loopMode;
        public float LoopStartSeconds => Mathf.Max(0f, loopStartSeconds);
        public float LoopEndSeconds => Mathf.Max(0f, loopEndSeconds);
        public bool HasLoopRegion => Loop && loopMode == AudioLoopMode.IntroThenLoop && LoopEndSeconds > LoopStartSeconds;

        public float GetPlaybackEndSeconds(AudioClip clip)
        {
            return clip == null || EndSeconds <= StartSeconds ? (clip != null ? clip.length : 0f) : Mathf.Min(EndSeconds, clip.length);
        }
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
                return clips[SelectSequentialIndex()];
            }

            var next = selectionMode == AudioVariantSelectionMode.WeightedRandom
                ? SelectWeightedIndex()
                : Random.Range(0, clips.Count);
            return clips[RememberSelection(next)];
        }

        private void OnValidate()
        {
            clips ??= new List<AudioClip>();
            clipWeights ??= new List<float>();
            while (clipWeights.Count < clips.Count) clipWeights.Add(1f);
            while (clipWeights.Count > clips.Count) clipWeights.RemoveAt(clipWeights.Count - 1);
        }

        private int SelectSequentialIndex()
        {
            return RememberSelection((lastClipIndex + 1) % clips.Count);
        }

        private int SelectWeightedIndex()
        {
            var total = 0f;
            for (var i = 0; i < clips.Count; i++) total += GetClipWeight(i);
            if (total <= 0f) return Random.Range(0, clips.Count);

            var roll = Random.value * total;
            for (var i = 0; i < clips.Count; i++)
            {
                roll -= GetClipWeight(i);
                if (roll <= 0f) return i;
            }

            return clips.Count - 1;
        }

        private int RememberSelection(int next)
        {
            if (neverRepeat && clips.Count > 1 && next == lastClipIndex)
            {
                next = (next + 1) % clips.Count;
            }

            lastClipIndex = next;
            return next;
        }

        private float GetClipWeight(int index)
        {
            return clipWeights != null && index < clipWeights.Count ? Mathf.Max(0f, clipWeights[index]) : 1f;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Audio
{
    [Serializable]
    public sealed class AudioEventDefinition
    {
        [SerializeField] private string key;
        [SerializeField] private string displayName;
        [SerializeField] private string bus = "sfx";
        [SerializeField] private AudioEventType eventType = AudioEventType.OneShot;
        [SerializeField] private AudioVariantSelectionMode selectionMode = AudioVariantSelectionMode.Random;
        [SerializeField] private List<AudioVariant> variants = new List<AudioVariant>();
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField, Range(-3f, 3f)] private float pitch = 1f;
        [SerializeField, Min(0f)] private float fadeInSeconds;
        [SerializeField, Min(0f)] private float fadeOutSeconds;
        [SerializeField] private bool loop;
        [SerializeField] private AudioTimeMode timeMode = AudioTimeMode.Scaled;
        [SerializeField, Range(0, 256)] private int priority = 128;
        [SerializeField, Min(0)] private int maxInstances = 0;
        [SerializeField, Min(0f)] private float cooldownSeconds = 0f;
        [SerializeField] private AudioSpatialSettings spatial = new AudioSpatialSettings();

        private int sequentialIndex;

        public string Key => key ?? string.Empty;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? Key : displayName;
        public AudioBusId Bus => new AudioBusId(bus);
        public AudioEventType EventType => eventType;
        public AudioVariantSelectionMode SelectionMode => selectionMode;
        public IReadOnlyList<AudioVariant> Variants => variants;
        public float Volume => Mathf.Clamp01(volume);
        public float Pitch => Mathf.Clamp(pitch, -3f, 3f);
        public float FadeInSeconds => Mathf.Max(0f, fadeInSeconds);
        public float FadeOutSeconds => Mathf.Max(0f, fadeOutSeconds);
        public bool Loop => loop || eventType == AudioEventType.Loop || eventType == AudioEventType.Music || eventType == AudioEventType.Ambience;
        public AudioTimeMode TimeMode => timeMode;
        public int Priority => priority;
        public int MaxInstances => Mathf.Max(0, maxInstances);
        public float CooldownSeconds => Mathf.Max(0f, cooldownSeconds);
        public AudioSpatialSettings Spatial => spatial;

        public AudioVariant SelectVariant()
        {
            if (variants == null || variants.Count == 0)
            {
                return null;
            }

            if (selectionMode == AudioVariantSelectionMode.Sequential)
            {
                var variant = variants[sequentialIndex % variants.Count];
                sequentialIndex = (sequentialIndex + 1) % variants.Count;
                return variant;
            }

            if (selectionMode == AudioVariantSelectionMode.WeightedRandom)
            {
                var total = 0f;
                for (var i = 0; i < variants.Count; i++)
                {
                    total += variants[i] != null ? variants[i].Weight : 0f;
                }

                if (total > 0f)
                {
                    var roll = UnityEngine.Random.value * total;
                    for (var i = 0; i < variants.Count; i++)
                    {
                        var weight = variants[i] != null ? variants[i].Weight : 0f;
                        if (roll <= weight)
                        {
                            return variants[i];
                        }

                        roll -= weight;
                    }
                }
            }

            return variants[UnityEngine.Random.Range(0, variants.Count)];
        }
    }
}

using System;
using UnityEngine;

namespace Dreamy.Audio
{
    [Serializable]
    public sealed class AudioVariant
    {
        [SerializeField] private AudioClip clip;
        [SerializeField, Min(0f)] private float weight = 1f;
        [SerializeField, Range(0f, 2f)] private float volumeMultiplier = 1f;
        [SerializeField, Range(-3f, 3f)] private float pitchMultiplier = 1f;

        public AudioClip Clip => clip;
        public float Weight => Mathf.Max(0f, weight);
        public float VolumeMultiplier => Mathf.Clamp(volumeMultiplier, 0f, 2f);
        public float PitchMultiplier => Mathf.Clamp(pitchMultiplier, -3f, 3f);
    }
}

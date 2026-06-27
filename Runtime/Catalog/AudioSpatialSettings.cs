using System;
using UnityEngine;

namespace Dreamy.Audio
{
    [Serializable]
    public sealed class AudioSpatialSettings
    {
        [SerializeField, Range(0f, 1f)] private float spatialBlend;
        [SerializeField, Min(0f)] private float minDistance = 1f;
        [SerializeField, Min(0.01f)] private float maxDistance = 25f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        [SerializeField, Range(0f, 5f)] private float dopplerLevel = 1f;
        [SerializeField, Range(0f, 360f)] private float spread;

        public float SpatialBlend => spatialBlend;
        public float MinDistance => minDistance;
        public float MaxDistance => Mathf.Max(minDistance, maxDistance);
        public AudioRolloffMode RolloffMode => rolloffMode;
        public float DopplerLevel => dopplerLevel;
        public float Spread => spread;
    }
}

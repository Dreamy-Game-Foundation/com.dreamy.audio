using System;
using UnityEngine;

namespace Dreamy.Audio
{
    [Serializable]
    public readonly struct AudioTransition
    {
        public AudioTransition(float seconds)
        {
            Seconds = Mathf.Max(0f, seconds);
        }

        public float Seconds { get; }
    }
}

using System;
using UnityEngine;

namespace Dreamy.Audio
{
    [Serializable]
    public readonly struct AudioBusId : IEquatable<AudioBusId>
    {
        public static readonly AudioBusId Master = new AudioBusId("master");
        public static readonly AudioBusId Music = new AudioBusId("music");
        public static readonly AudioBusId Sfx = new AudioBusId("sfx");
        public static readonly AudioBusId Ui = new AudioBusId("ui");
        public static readonly AudioBusId Voice = new AudioBusId("voice");
        public static readonly AudioBusId Ambience = new AudioBusId("ambience");

        [SerializeField] private readonly string value;

        public AudioBusId(string value)
        {
            this.value = value ?? string.Empty;
        }

        public string Value => value ?? string.Empty;
        public bool IsValid => AudioKey.IsValidPart(Value);

        public bool Equals(AudioBusId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is AudioBusId other && Equals(other);
        }

        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;
        public override string ToString() => Value;

        public static bool operator ==(AudioBusId left, AudioBusId right) => left.Equals(right);
        public static bool operator !=(AudioBusId left, AudioBusId right) => !left.Equals(right);
    }
}

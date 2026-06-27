using System;

namespace Dreamy.Audio
{
    public readonly struct AudioHandle : IEquatable<AudioHandle>
    {
        public static readonly AudioHandle Invalid = new AudioHandle(0);

        public AudioHandle(int id)
        {
            Id = id;
        }

        public int Id { get; }
        public bool IsValid => Id != 0;

        public bool Equals(AudioHandle other) => Id == other.Id;
        public override bool Equals(object obj) => obj is AudioHandle other && Equals(other);
        public override int GetHashCode() => Id;
        public override string ToString() => IsValid ? Id.ToString() : "Invalid";
    }
}

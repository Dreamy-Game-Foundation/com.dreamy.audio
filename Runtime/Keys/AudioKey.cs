using System;
using UnityEngine;

namespace Dreamy.Audio
{
    [Serializable]
    public readonly struct AudioKey : IEquatable<AudioKey>
    {
        [SerializeField] private readonly string catalogId;
        [SerializeField] private readonly string key;

        public AudioKey(string catalogId, string key)
        {
            this.catalogId = catalogId ?? string.Empty;
            this.key = key ?? string.Empty;
        }

        public string CatalogId => catalogId ?? string.Empty;
        public string Key => key ?? string.Empty;
        public bool IsValid => IsValidPart(CatalogId) && IsValidPart(Key);

        public override string ToString()
        {
            return string.IsNullOrEmpty(CatalogId) ? Key : $"{CatalogId}:{Key}";
        }

        public bool Equals(AudioKey other)
        {
            return string.Equals(CatalogId, other.CatalogId, StringComparison.Ordinal)
                && string.Equals(Key, other.Key, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is AudioKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((CatalogId != null ? CatalogId.GetHashCode() : 0) * 397)
                    ^ (Key != null ? Key.GetHashCode() : 0);
            }
        }

        public static bool operator ==(AudioKey left, AudioKey right) => left.Equals(right);
        public static bool operator !=(AudioKey left, AudioKey right) => !left.Equals(right);

        public static bool IsValidPart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.' || c == '/')
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}

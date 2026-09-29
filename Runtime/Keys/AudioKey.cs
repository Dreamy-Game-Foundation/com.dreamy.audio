using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamy.Audio
{
    [Serializable]
    public struct AudioKey : IEquatable<AudioKey>
    {
        [FormerlySerializedAs("catalogId")]
        [SerializeField] private string libraryId;
        [SerializeField] private string key;

        public AudioKey(string libraryId, string key)
        {
            this.libraryId = libraryId ?? string.Empty;
            this.key = key ?? string.Empty;
        }

        public string LibraryId => libraryId ?? string.Empty;
        public string Key => key ?? string.Empty;
        /// <summary>Stable, human-readable identifier used in data, logs, and optional code constants.</summary>
        public string Id => string.IsNullOrEmpty(LibraryId) ? Key : $"{LibraryId}/{Key}";
        public bool IsValid => IsValidPart(LibraryId) && IsValidPart(Key);

        public override string ToString()
        {
            return Id;
        }

        public bool Equals(AudioKey other)
        {
            return string.Equals(LibraryId, other.LibraryId, StringComparison.Ordinal)
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
                return ((LibraryId != null ? LibraryId.GetHashCode() : 0) * 397)
                    ^ (Key != null ? Key.GetHashCode() : 0);
            }
        }

        public static bool operator ==(AudioKey left, AudioKey right) => left.Equals(right);
        public static bool operator !=(AudioKey left, AudioKey right) => !left.Equals(right);

        /// <summary>Creates a key from the canonical <c>library/key</c> form.</summary>
        public static AudioKey FromId(string id)
        {
            return TryParse(id, out var audioKey) ? audioKey : default;
        }

        public static bool TryParse(string id, out AudioKey audioKey)
        {
            audioKey = default;
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            var separator = id.IndexOf( '/');
            if (separator <= 0 || separator == id.Length - 1 || id.IndexOf( '/', separator + 1) >= 0)
            {
                return false;
            }

            audioKey = new AudioKey(id.Substring(0, separator), id.Substring(separator + 1));
            return audioKey.IsValid;
        }

        public static bool IsValidPart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.')
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Audio
{
    [CreateAssetMenu(menuName = "Dreamy/Audio/Catalog", fileName = "DreamyAudioCatalog")]
    public sealed class DreamyAudioCatalog : ScriptableObject
    {
        [SerializeField] private string catalogId = "core";
        [SerializeField] private string generatedNamespace = "Dreamy.Audio.Generated";
        [SerializeField] private List<AudioLibrary> libraries = new List<AudioLibrary>();
        [SerializeField] private List<AudioEventDefinition> events = new List<AudioEventDefinition>();

        public string CatalogId => catalogId;
        public string GeneratedNamespace => generatedNamespace;
        public IReadOnlyList<AudioLibrary> Libraries => libraries;
        public IReadOnlyList<AudioEventDefinition> Events => events;

        public bool TryGetEvent(string eventKey, out AudioEventDefinition definition)
        {
            for (var i = 0; i < events.Count; i++)
            {
                var candidate = events[i];
                if (candidate != null && candidate.Key == eventKey)
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public bool TryGetFile(string eventKey, out AudioFileObject file)
        {
            for (var i = 0; i < libraries.Count; i++)
            {
                var library = libraries[i];
                if (library == null)
                {
                    continue;
                }

                if (library.TryGetSound(eventKey, out var sound))
                {
                    file = sound;
                    return true;
                }

                if (library.TryGetMusic(eventKey, out var music))
                {
                    file = music;
                    return true;
                }
            }

            file = null;
            return false;
        }
    }
}

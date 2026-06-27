using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Audio
{
    [CreateAssetMenu(menuName = "Dreamy/Audio/Catalog", fileName = "DreamyAudioCatalog")]
    public sealed class DreamyAudioCatalog : ScriptableObject
    {
        [SerializeField] private string catalogId = "core";
        [SerializeField] private string generatedNamespace = "Dreamy.Audio.Generated";
        [SerializeField] private List<AudioEventDefinition> events = new List<AudioEventDefinition>();

        public string CatalogId => catalogId;
        public string GeneratedNamespace => generatedNamespace;
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
    }
}

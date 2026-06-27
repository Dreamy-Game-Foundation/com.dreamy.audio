using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Audio
{
    [CreateAssetMenu(menuName = "Dreamy/Audio/Profile", fileName = "DreamyAudioProfile")]
    public sealed class DreamyAudioProfile : ScriptableObject
    {
        [SerializeField, Min(1)] private int schemaVersion = 1;
        [SerializeField] private List<AudioBusDefinition> buses = new List<AudioBusDefinition>();
        [SerializeField] private List<DreamyAudioCatalog> catalogs = new List<DreamyAudioCatalog>();
        [SerializeField, Min(1)] private int initialPoolSize = 16;
        [SerializeField, Min(1)] private int maxPoolSize = 64;
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool logWarnings = true;

        public int SchemaVersion => schemaVersion;
        public IReadOnlyList<AudioBusDefinition> Buses => buses;
        public IReadOnlyList<DreamyAudioCatalog> Catalogs => catalogs;
        public int InitialPoolSize => Mathf.Max(1, initialPoolSize);
        public int MaxPoolSize => Mathf.Max(InitialPoolSize, maxPoolSize);
        public bool KeepAliveAcrossScenes => dontDestroyOnLoad;
        public bool LogWarnings => logWarnings;

        public bool TryGetBus(AudioBusId id, out AudioBusDefinition definition)
        {
            for (var i = 0; i < buses.Count; i++)
            {
                if (buses[i] != null && buses[i].Id == id)
                {
                    definition = buses[i];
                    return true;
                }
            }

            definition = null;
            return false;
        }
    }
}

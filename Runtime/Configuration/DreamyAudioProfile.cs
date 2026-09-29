using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Audio
{
    [CreateAssetMenu(menuName = "Dreamy/Audio/Profile", fileName = "DreamyAudioProfile")]
    public sealed class DreamyAudioProfile : ScriptableObject
    {
        [SerializeField, Min(1)] private int schemaVersion = 1;
        [SerializeField] private List<AudioBusDefinition> buses = CreateCasualDefaultBuses();
        [SerializeField] private List<AudioLibrary> libraries = new List<AudioLibrary>();
        [SerializeField, Min(1)] private int initialPoolSize = 16;
        [SerializeField, Min(1)] private int maxPoolSize = 64;
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool logWarnings = true;

        /// <summary>Adds only missing casual-game buses; existing bus settings are left unchanged.</summary>
        [ContextMenu("Add Missing Casual Default Buses")]
        public void AddMissingCasualDefaultBuses()
        {
            buses ??= new List<AudioBusDefinition>();
            foreach (var defaultBus in CreateCasualDefaultBuses())
            {
                if (!TryGetBus(defaultBus.Id, out _)) buses.Add(defaultBus);
            }
        }

        private static List<AudioBusDefinition> CreateCasualDefaultBuses()
        {
            return new List<AudioBusDefinition>
            {
                AudioBusDefinition.Create(AudioBusId.Music.Value, "Music", 0.8f, 2),
                AudioBusDefinition.Create(AudioBusId.Sfx.Value, "SFX", 1f, 24),
                AudioBusDefinition.Create(AudioBusId.Ui.Value, "UI", 1f, 8),
                AudioBusDefinition.Create(AudioBusId.Voice.Value, "Voice", 1f, 4),
                AudioBusDefinition.Create(AudioBusId.Ambience.Value, "Ambience", 0.7f, 4)
            };
        }

        public int SchemaVersion => schemaVersion;
        public IReadOnlyList<AudioBusDefinition> Buses => buses;
        public IReadOnlyList<AudioLibrary> Libraries => libraries;
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

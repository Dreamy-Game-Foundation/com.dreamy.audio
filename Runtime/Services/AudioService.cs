using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Audio
{
    public sealed class AudioService : IAudioService
    {
        private sealed class ActiveVoice
        {
            public AudioHandle Handle;
            public AudioKey Key;
            public AudioBusId Bus;
            public AudioSource Source;
            public bool Looping;
            public int Priority;
            public float RemainingFadeOut;
            public float FadeOutStartVolume;
        }

        private readonly Dictionary<int, ActiveVoice> voices = new Dictionary<int, ActiveVoice>();
        private readonly Dictionary<string, float> lastPlayTimes = new Dictionary<string, float>();
        private readonly Dictionary<string, int> instanceCounts = new Dictionary<string, int>();
        private readonly Dictionary<string, bool> mutedBuses = new Dictionary<string, bool>();
        private readonly IAudioPreferenceStore preferenceStore;
        private DreamyAudioProfile profile;
        private GameObject root;
        private AudioSourcePool pool;
        private int nextHandleId = 1;

        public AudioService() : this(new PlayerPrefsAudioPreferenceStore())
        {
        }

        public AudioService(IAudioPreferenceStore preferenceStore)
        {
            this.preferenceStore = preferenceStore;
        }

        public bool IsInitialized => profile != null && pool != null;
        public IReadOnlyList<AudioBusDefinition> Buses => profile != null ? profile.Buses : System.Array.Empty<AudioBusDefinition>();

        public void Initialize(DreamyAudioProfile profile)
        {
            this.profile = profile;

            if (profile == null)
            {
                Warn("DreamyAudio.Initialize called with a null profile.");
                return;
            }

            if (root == null)
            {
                root = new GameObject("DreamyAudio");
                var host = root.AddComponent<AudioRuntimeHost>();
                host.Service = this;
                if (profile.KeepAliveAcrossScenes && Application.isPlaying)
                {
                    Object.DontDestroyOnLoad(root);
                }
            }

            pool = new AudioSourcePool(root.transform, profile.InitialPoolSize, profile.MaxPoolSize);
            ApplyStoredBusVolumes();
        }

        public AudioPlayResult Play(AudioKey key)
        {
            return PlayInternal(key, null, null, false, default);
        }

        public AudioPlayResult Play(AudioKey key, Vector3 position)
        {
            return PlayInternal(key, position, null, false, default);
        }

        public AudioPlayResult PlayAttached(AudioKey key, Transform target)
        {
            return PlayInternal(key, null, target, false, default);
        }

        public AudioHandle PlayLoop(AudioKey key)
        {
            return PlayInternal(key, null, null, true, default).Handle;
        }

        public AudioHandle PlayMusic(AudioKey key, AudioTransition transition)
        {
            if (TryResolve(key, out var definition, out _))
            {
                StopBus(definition.Bus, transition);
            }

            return PlayInternal(key, null, null, true, transition).Handle;
        }

        public bool Stop(AudioHandle handle, AudioTransition transition = default)
        {
            if (!handle.IsValid || !voices.TryGetValue(handle.Id, out var voice))
            {
                return false;
            }

            if (transition.Seconds <= 0f)
            {
                ReleaseVoice(voice);
                return true;
            }

            voice.RemainingFadeOut = transition.Seconds;
            voice.FadeOutStartVolume = voice.Source != null ? voice.Source.volume : 0f;
            return true;
        }

        public void StopBus(AudioBusId bus, AudioTransition transition = default)
        {
            var handles = new List<AudioHandle>();
            foreach (var voice in voices.Values)
            {
                if (voice.Bus == bus)
                {
                    handles.Add(voice.Handle);
                }
            }

            for (var i = 0; i < handles.Count; i++)
            {
                Stop(handles[i], transition);
            }
        }

        public void PauseBus(AudioBusId bus)
        {
            foreach (var voice in voices.Values)
            {
                if (voice.Bus == bus && voice.Source != null)
                {
                    voice.Source.Pause();
                }
            }
        }

        public void ResumeBus(AudioBusId bus)
        {
            foreach (var voice in voices.Values)
            {
                if (voice.Bus == bus && voice.Source != null)
                {
                    voice.Source.UnPause();
                }
            }
        }

        public float GetVolume(AudioBusId bus)
        {
            if (profile == null || !profile.TryGetBus(bus, out var definition))
            {
                return 1f;
            }

            if (preferenceStore.TryGetFloat(definition.PreferenceKey, out var stored))
            {
                return Mathf.Clamp01(stored);
            }

            return definition.DefaultVolume;
        }

        public void SetVolume(AudioBusId bus, float normalizedVolume, bool persist = true)
        {
            if (profile == null || !profile.TryGetBus(bus, out var definition))
            {
                return;
            }

            var clamped = Mathf.Clamp01(normalizedVolume);
            if (persist && definition.PersistVolume)
            {
                preferenceStore.SetFloat(definition.PreferenceKey, clamped);
            }

            ApplyBusVolume(definition, clamped);
        }

        public void SetMuted(AudioBusId bus, bool muted, bool persist = true)
        {
            mutedBuses[bus.Value] = muted;
            SetVolume(bus, muted ? 0f : GetVolume(bus), persist);
        }

        internal void Tick(float deltaTime)
        {
            if (!IsInitialized)
            {
                return;
            }

            var finished = new List<AudioHandle>();
            foreach (var voice in voices.Values)
            {
                if (voice.Source == null)
                {
                    finished.Add(voice.Handle);
                    continue;
                }

                if (voice.RemainingFadeOut > 0f)
                {
                    voice.RemainingFadeOut -= deltaTime;
                    var t = Mathf.Clamp01(voice.RemainingFadeOut / Mathf.Max(0.0001f, deltaTime + voice.RemainingFadeOut));
                    voice.Source.volume = voice.FadeOutStartVolume * t;
                    if (voice.RemainingFadeOut <= 0f)
                    {
                        finished.Add(voice.Handle);
                    }
                    continue;
                }

                if (!voice.Looping && !voice.Source.isPlaying)
                {
                    finished.Add(voice.Handle);
                }
            }

            for (var i = 0; i < finished.Count; i++)
            {
                if (voices.TryGetValue(finished[i].Id, out var voice))
                {
                    ReleaseVoice(voice);
                }
            }
        }

        private AudioPlayResult PlayInternal(AudioKey key, Vector3? position, Transform target, bool forceLoop, AudioTransition transition)
        {
            if (!IsInitialized)
            {
                return AudioPlayResult.Fail(profile == null ? AudioPlayStatus.MissingProfile : AudioPlayStatus.MissingService, "DreamyAudio is not initialized.");
            }

            if (!TryResolve(key, out var definition, out var resolveFailure))
            {
                return resolveFailure;
            }

            if (mutedBuses.TryGetValue(definition.Bus.Value, out var muted) && muted)
            {
                return AudioPlayResult.Fail(AudioPlayStatus.Muted, $"Audio bus '{definition.Bus}' is muted.");
            }

            var clock = Time.unscaledTime;
            if (definition.CooldownSeconds > 0f && lastPlayTimes.TryGetValue(key.ToString(), out var lastTime) && clock - lastTime < definition.CooldownSeconds)
            {
                return AudioPlayResult.Fail(AudioPlayStatus.Cooldown, $"Audio key '{key}' is on cooldown.");
            }

            if (definition.MaxInstances > 0 && instanceCounts.TryGetValue(key.ToString(), out var count) && count >= definition.MaxInstances)
            {
                return AudioPlayResult.Fail(AudioPlayStatus.InstanceLimit, $"Audio key '{key}' reached max instances.");
            }

            var variant = definition.SelectVariant();
            if (variant == null || variant.Clip == null)
            {
                return AudioPlayResult.Fail(AudioPlayStatus.MissingClip, $"Audio key '{key}' has no playable clip.");
            }

            if (!pool.TryRent(out var source))
            {
                return AudioPlayResult.Fail(AudioPlayStatus.PoolLimit, "AudioSource pool reached its limit.");
            }

            ConfigureSource(source, definition, variant, position, target);
            var handle = new AudioHandle(nextHandleId++);
            var voice = new ActiveVoice
            {
                Handle = handle,
                Key = key,
                Bus = definition.Bus,
                Source = source,
                Looping = forceLoop || definition.Loop,
                Priority = definition.Priority
            };

            source.loop = voice.Looping;
            source.Play();
            voices[handle.Id] = voice;
            IncrementInstance(key);
            lastPlayTimes[key.ToString()] = clock;

            if (transition.Seconds > 0f || definition.FadeInSeconds > 0f)
            {
                source.volume = definition.Volume * variant.VolumeMultiplier;
            }

            return AudioPlayResult.Played(handle);
        }

        private bool TryResolve(AudioKey key, out AudioEventDefinition definition, out AudioPlayResult failure)
        {
            definition = null;
            if (!key.IsValid)
            {
                failure = AudioPlayResult.Fail(AudioPlayStatus.MissingKey, $"Invalid audio key '{key}'.");
                return false;
            }

            for (var i = 0; i < profile.Catalogs.Count; i++)
            {
                var catalog = profile.Catalogs[i];
                if (catalog == null || catalog.CatalogId != key.CatalogId)
                {
                    continue;
                }

                if (catalog.TryGetEvent(key.Key, out definition))
                {
                    failure = default;
                    return true;
                }
            }

            failure = AudioPlayResult.Fail(AudioPlayStatus.MissingKey, $"Audio key '{key}' was not found.");
            return false;
        }

        private void ConfigureSource(AudioSource source, AudioEventDefinition definition, AudioVariant variant, Vector3? position, Transform target)
        {
            source.clip = variant.Clip;
            source.outputAudioMixerGroup = profile.TryGetBus(definition.Bus, out var bus) ? bus.MixerGroup : null;
            source.volume = definition.Volume * variant.VolumeMultiplier * GetVolume(definition.Bus);
            source.pitch = definition.Pitch * variant.PitchMultiplier;
            source.priority = definition.Priority;
            source.ignoreListenerPause = definition.TimeMode == AudioTimeMode.IgnoreListenerPause;
            source.spatialBlend = definition.Spatial.SpatialBlend;
            source.minDistance = definition.Spatial.MinDistance;
            source.maxDistance = definition.Spatial.MaxDistance;
            source.rolloffMode = definition.Spatial.RolloffMode;
            source.dopplerLevel = definition.Spatial.DopplerLevel;
            source.spread = definition.Spatial.Spread;

            if (target != null)
            {
                source.transform.position = target.position;
            }
            else if (position.HasValue)
            {
                source.transform.position = position.Value;
            }
            else
            {
                source.transform.localPosition = Vector3.zero;
            }
        }

        private void ReleaseVoice(ActiveVoice voice)
        {
            voices.Remove(voice.Handle.Id);
            DecrementInstance(voice.Key);
            pool.Return(voice.Source);
        }

        private void IncrementInstance(AudioKey key)
        {
            var id = key.ToString();
            instanceCounts.TryGetValue(id, out var count);
            instanceCounts[id] = count + 1;
        }

        private void DecrementInstance(AudioKey key)
        {
            var id = key.ToString();
            if (!instanceCounts.TryGetValue(id, out var count))
            {
                return;
            }

            if (count <= 1)
            {
                instanceCounts.Remove(id);
            }
            else
            {
                instanceCounts[id] = count - 1;
            }
        }

        private void ApplyStoredBusVolumes()
        {
            for (var i = 0; i < profile.Buses.Count; i++)
            {
                var bus = profile.Buses[i];
                if (bus == null)
                {
                    continue;
                }

                ApplyBusVolume(bus, GetVolume(bus.Id));
            }
        }

        private static void ApplyBusVolume(AudioBusDefinition bus, float normalizedVolume)
        {
            if (bus.MixerGroup == null || string.IsNullOrWhiteSpace(bus.ExposedVolumeParameter))
            {
                return;
            }

            var db = normalizedVolume <= 0.0001f ? -80f : Mathf.Log10(normalizedVolume) * 20f;
            bus.MixerGroup.audioMixer.SetFloat(bus.ExposedVolumeParameter, db);
        }

        private void Warn(string message)
        {
            if (profile == null || profile.LogWarnings)
            {
                Debug.LogWarning(message);
            }
        }
    }
}

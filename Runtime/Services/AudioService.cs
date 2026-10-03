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
            public bool HasLoopRegion;
            public float LoopStartSeconds;
            public float LoopEndSeconds;
            public bool HasPlaybackEnd;
            public float PlaybackEndSeconds;
            public int Priority;
            public Transform FollowTarget;
            public float FadeInRemaining;
            public float FadeInDuration;
            public float TargetVolume;
            public float FadeVolume = 1f;
            public float RemainingFadeOut;
            public float FadeOutStartVolume;
        }

        private readonly Dictionary<int, ActiveVoice> voices = new Dictionary<int, ActiveVoice>();
        private readonly Dictionary<string, float> lastPlayTimes = new Dictionary<string, float>();
        private readonly Dictionary<string, int> instanceCounts = new Dictionary<string, int>();
        private readonly Dictionary<string, float> busVolumes = new Dictionary<string, float>();
        private readonly HashSet<string> mixerControlledBuses = new HashSet<string>();
        private readonly Dictionary<string, bool> mutedBuses = new Dictionary<string, bool>();
        private readonly List<int> finishedVoiceIds = new List<int>();
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

            busVolumes.Clear();
            mixerControlledBuses.Clear();
            pool = new AudioSourcePool(root.transform, profile.InitialPoolSize, profile.MaxPoolSize);
            ApplyStoredBusVolumes();
        }

        public AudioPlayResult Play(AudioKey key)
        {
            return PlayInternal(key, null, null, false, default);
        }

        public AudioPlayResult Play(string id)
        {
            return Play(AudioKey.FromId(id));
        }

        public AudioPlayResult Play(AudioFileObject file)
        {
            return PlayInternal(file, null, null, false, default);
        }

        public AudioPlayResult Play(AudioKey key, Vector3 position)
        {
            return PlayInternal(key, position, null, false, default);
        }

        public AudioPlayResult Play(AudioFileObject file, Vector3 position)
        {
            return PlayInternal(file, position, null, false, default);
        }

        public AudioPlayResult PlayAttached(AudioKey key, Transform target)
        {
            return PlayInternal(key, null, target, false, default);
        }

        public AudioPlayResult PlayAttached(AudioFileObject file, Transform target)
        {
            return PlayInternal(file, null, target, false, default);
        }

        public AudioHandle PlayLoop(string id)
        {
            return PlayLoop(AudioKey.FromId(id));
        }

        public AudioHandle PlayLoop(AudioKey key)
        {
            return PlayInternal(key, null, null, true, default).Handle;
        }

        public AudioHandle PlayMusic(string id, AudioTransition transition = default)
        {
            return PlayMusic(AudioKey.FromId(id), transition);
        }

        public AudioHandle PlayMusic(AudioKey key, AudioTransition transition)
        {
            if (TryResolve(key, out var file, out _))
            {
                StopBus(file.Bus, transition);
            }

            return PlayInternal(key, null, null, true, transition).Handle;
        }

        public AudioHandle PlayMusic(MusicAudioFile file, AudioTransition transition)
        {
            if (file != null)
            {
                StopBus(file.Bus, transition);
            }

            return PlayInternal(file, null, null, true, transition).Handle;
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
            voice.FadeOutStartVolume = voice.FadeVolume;
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

            if (busVolumes.TryGetValue(bus.Value, out var current)) return current;

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
            busVolumes[bus.Value] = clamped;
            if (persist && definition.PersistVolume)
            {
                preferenceStore.SetFloat(definition.PreferenceKey, clamped);
            }

            ApplyBusVolume(definition, clamped);
            foreach (var voice in voices.Values)
            {
                if (voice.Bus == bus) ApplyVoiceVolume(voice);
            }
        }

        public void SetMuted(AudioBusId bus, bool muted, bool persist = true)
        {
            mutedBuses[bus.Value] = muted;
            SetVolume(bus, GetVolume(bus), persist);
        }

        internal void Tick(float deltaTime)
        {
            if (!IsInitialized)
            {
                return;
            }

            finishedVoiceIds.Clear();
            foreach (var voice in voices.Values)
            {
                if (voice.Source == null)
                {
                    finishedVoiceIds.Add(voice.Handle.Id);
                    continue;
                }

                if (voice.FollowTarget != null)
                {
                    voice.Source.transform.position = voice.FollowTarget.position;
                }

                if (voice.HasLoopRegion && voice.Source.clip != null && voice.Source.time >= voice.LoopEndSeconds)
                {
                    voice.Source.time = voice.LoopStartSeconds;
                }
                else if (voice.HasPlaybackEnd && voice.Source.clip != null && voice.Source.time >= voice.PlaybackEndSeconds)
                {
                    if (voice.Looping)
                    {
                        voice.Source.time = Mathf.Min(voice.LoopStartSeconds, voice.PlaybackEndSeconds);
                    }
                    else
                    {
                        finishedVoiceIds.Add(voice.Handle.Id);
                        continue;
                    }
                }

                if (voice.FadeInRemaining > 0f)
                {
                    voice.FadeInRemaining = Mathf.Max(0f, voice.FadeInRemaining - deltaTime);
                    var progress = 1f - voice.FadeInRemaining / Mathf.Max(0.0001f, voice.FadeInDuration);
                    voice.FadeVolume = Mathf.Clamp01(progress);
                    ApplyVoiceVolume(voice);
                }

                if (voice.RemainingFadeOut > 0f)
                {
                    voice.RemainingFadeOut -= deltaTime;
                    var t = Mathf.Clamp01(voice.RemainingFadeOut / Mathf.Max(0.0001f, deltaTime + voice.RemainingFadeOut));
                    voice.FadeVolume = voice.FadeOutStartVolume * t;
                    ApplyVoiceVolume(voice);
                    if (voice.RemainingFadeOut <= 0f)
                    {
                        finishedVoiceIds.Add(voice.Handle.Id);
                    }
                    continue;
                }

                if (!voice.Looping && !voice.Source.isPlaying)
                {
                    finishedVoiceIds.Add(voice.Handle.Id);
                }
            }

            for (var i = 0; i < finishedVoiceIds.Count; i++)
            {
                if (voices.TryGetValue(finishedVoiceIds[i], out var voice))
                {
                    ReleaseVoice(voice);
                }
            }
        }

        private AudioPlayResult PlayInternal(AudioKey key, Vector3? position, Transform target, bool forceLoop, AudioTransition transition)
        {
            if (!IsInitialized) return AudioPlayResult.Fail(profile == null ? AudioPlayStatus.MissingProfile : AudioPlayStatus.MissingService, "DreamyAudio is not initialized.");
            if (!TryResolve(key, out var file, out var failure)) return failure;
            return PlayResolved(key, file, position, target, forceLoop, transition);
        }

        private AudioPlayResult PlayInternal(AudioFileObject file, Vector3? position, Transform target, bool forceLoop, AudioTransition transition)
        {
            if (!IsInitialized)
            {
                return AudioPlayResult.Fail(profile == null ? AudioPlayStatus.MissingProfile : AudioPlayStatus.MissingService, "DreamyAudio is not initialized.");
            }

            if (file == null)
            {
                return AudioPlayResult.Fail(AudioPlayStatus.MissingKey, "Audio file is null.");
            }

            var key = GetKeyForFile(file);
            return PlayResolved(key, file, position, target, forceLoop, transition);
        }

        private AudioPlayResult PlayResolved(AudioKey key, AudioFileObject file, Vector3? position, Transform target, bool forceLoop, AudioTransition transition)
        {
            if (mutedBuses.TryGetValue(file.Bus.Value, out var muted) && muted)
            {
                return AudioPlayResult.Fail(AudioPlayStatus.Muted, $"Audio bus '{file.Bus}' is muted.");
            }

            var clock = Time.unscaledTime;
            if (file.CooldownSeconds > 0f && lastPlayTimes.TryGetValue(key.ToString(), out var lastTime) && clock - lastTime < file.CooldownSeconds)
            {
                return AudioPlayResult.Fail(AudioPlayStatus.Cooldown, $"Audio key '{key}' is on cooldown.");
            }

            if (file.MaxInstances > 0 && instanceCounts.TryGetValue(key.ToString(), out var count) && count >= file.MaxInstances)
            {
                return AudioPlayResult.Fail(AudioPlayStatus.InstanceLimit, $"Audio key '{key}' reached max instances.");
            }

            var clip = file.SelectClip();
            if (clip == null)
            {
                return AudioPlayResult.Fail(AudioPlayStatus.MissingClip, $"Audio key '{key}' has no playable clip.");
            }

            if (!pool.TryRent(out var source))
            {
                return AudioPlayResult.Fail(AudioPlayStatus.PoolLimit, "AudioSource pool reached its limit.");
            }

            ConfigureSource(source, file, clip, position, target);
            var handle = new AudioHandle(nextHandleId++);
            var voice = new ActiveVoice
            {
                Handle = handle,
                Key = key,
                Bus = file.Bus,
                Source = source,
                Looping = forceLoop || file.Loop,
                HasLoopRegion = (forceLoop || file.Loop) && file.HasLoopRegion && Mathf.Min(file.LoopEndSeconds, clip.length) > file.LoopStartSeconds,
                LoopStartSeconds = file.HasLoopRegion ? file.LoopStartSeconds : file.StartSeconds,
                LoopEndSeconds = Mathf.Min(file.LoopEndSeconds, clip.length),
                HasPlaybackEnd = file.EndSeconds > file.StartSeconds && file.EndSeconds < clip.length,
                PlaybackEndSeconds = file.EndSeconds > file.StartSeconds ? Mathf.Min(file.EndSeconds, clip.length) : clip.length,
                Priority = file.Priority,
                FollowTarget = target,
                TargetVolume = file.Volume
            };

            source.loop = voice.Looping && !voice.HasLoopRegion && !voice.HasPlaybackEnd;
            if (file.StartSeconds > 0f)
            {
                source.time = Mathf.Min(file.StartSeconds, Mathf.Max(0f, clip.length - 0.001f));
            }

            source.Play();
            voices[handle.Id] = voice;
            IncrementInstance(key);
            lastPlayTimes[key.ToString()] = clock;

            var fadeInSeconds = transition.Seconds > 0f ? transition.Seconds : file.FadeInSeconds;
            if (fadeInSeconds > 0f)
            {
                voice.FadeInDuration = fadeInSeconds;
                voice.FadeInRemaining = fadeInSeconds;
                voice.FadeVolume = 0f;
            }

            ApplyVoiceVolume(voice);
            return AudioPlayResult.Played(handle);
        }

        private AudioKey GetKeyForFile(AudioFileObject file)
        {
            for (var i = 0; i < profile.Libraries.Count; i++)
            {
                var library = profile.Libraries[i];
                if (library == null) continue;
                foreach (var candidate in library.EnumerateFiles())
                {
                    if (candidate == file) return new AudioKey(library.LibraryId, file.Key);
                }
            }

            return new AudioKey("direct", file.Key);
        }

        private bool TryResolve(AudioKey key, out AudioFileObject file, out AudioPlayResult failure)
        {
            file = null;
            if (!key.IsValid)
            {
                failure = AudioPlayResult.Fail(AudioPlayStatus.MissingKey, $"Invalid audio key '{key}'.");
                return false;
            }
            for (var i = 0; i < profile.Libraries.Count; i++)
            {
                var library = profile.Libraries[i];
                if (library != null && library.LibraryId == key.LibraryId && library.TryGetFile(key.Key, out file))
                {
                    failure = default;
                    return true;
                }
            }
            failure = AudioPlayResult.Fail(AudioPlayStatus.MissingKey, $"Audio key '{key}' was not found.");
            return false;
        }

        private void ConfigureSource(AudioSource source, AudioFileObject file, AudioClip clip, Vector3? position, Transform target)
        {
            source.clip = clip;
            source.outputAudioMixerGroup = file.MixerGroupOverride != null ? file.MixerGroupOverride : profile.TryGetBus(file.Bus, out var bus) ? bus.MixerGroup : null;
            source.volume = file.Volume;
            source.pitch = file.Pitch + Random.Range(-file.RandomPitch, file.RandomPitch);
            source.priority = file.Priority;
            source.ignoreListenerPause = file.TimeMode == AudioTimeMode.IgnoreListenerPause;
            source.bypassEffects = file.BypassEffects;
            source.bypassListenerEffects = file.BypassListenerEffects;
            source.bypassReverbZones = file.BypassReverbZones;
            source.spatialBlend = file.Spatial.SpatialBlend;
            source.minDistance = file.Spatial.MinDistance;
            source.maxDistance = file.Spatial.MaxDistance;
            source.rolloffMode = file.Spatial.RolloffMode;
            source.dopplerLevel = file.Spatial.DopplerLevel;
            source.spread = file.Spatial.Spread;

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

        private void ApplyBusVolume(AudioBusDefinition bus, float normalizedVolume)
        {
            if (bus.MixerGroup == null || string.IsNullOrWhiteSpace(bus.ExposedVolumeParameter))
            {
                return;
            }

            if (mutedBuses.TryGetValue(bus.Id.Value, out var muted) && muted) normalizedVolume = 0f;
            var db = normalizedVolume <= 0.0001f ? -80f : Mathf.Log10(normalizedVolume) * 20f;
            if (bus.MixerGroup.audioMixer.SetFloat(bus.ExposedVolumeParameter, db))
                mixerControlledBuses.Add(bus.Id.Value);
            else
                mixerControlledBuses.Remove(bus.Id.Value);
        }

        private void ApplyVoiceVolume(ActiveVoice voice)
        {
            if (voice.Source == null) return;
            var gain = GetVolume(voice.Bus);
            if (mixerControlledBuses.Contains(voice.Bus.Value)
                && profile.TryGetBus(voice.Bus, out var bus)
                && voice.Source.outputAudioMixerGroup == bus.MixerGroup)
            {
                gain = 1f;
            }
            if (mutedBuses.TryGetValue(voice.Bus.Value, out var muted) && muted) gain = 0f;
            voice.Source.volume = voice.TargetVolume * voice.FadeVolume * gain;
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

using UnityEngine;

namespace Dreamy.Audio
{
    public static class DreamyAudio
    {
        private static IAudioService service;

        public static IAudioService Service => service ?? (service = new AudioService());
        public static bool IsInitialized => Service.IsInitialized;

        public static void SetService(IAudioService audioService)
        {
            service = audioService;
        }

        public static void Initialize(DreamyAudioProfile profile)
        {
            Service.Initialize(profile);
        }

        public static AudioPlayResult Play(AudioKey key) => Service.Play(key);
        public static AudioPlayResult Play(AudioKey key, Vector3 position) => Service.Play(key, position);
        public static AudioPlayResult PlayAttached(AudioKey key, Transform target) => Service.PlayAttached(key, target);
        public static AudioHandle PlayLoop(AudioKey key) => Service.PlayLoop(key);
        public static AudioHandle PlayMusic(AudioKey key, AudioTransition transition = default) => Service.PlayMusic(key, transition);
        public static bool Stop(AudioHandle handle, AudioTransition transition = default) => Service.Stop(handle, transition);
        public static void StopBus(AudioBusId bus, AudioTransition transition = default) => Service.StopBus(bus, transition);
        public static float GetVolume(AudioBusId bus) => Service.GetVolume(bus);
        public static void SetVolume(AudioBusId bus, float volume, bool persist = true) => Service.SetVolume(bus, volume, persist);
        public static void SetMuted(AudioBusId bus, bool muted, bool persist = true) => Service.SetMuted(bus, muted, persist);
    }
}

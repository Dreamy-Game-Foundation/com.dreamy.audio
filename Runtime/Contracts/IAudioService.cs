using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Audio
{
    public interface IAudioService
    {
        bool IsInitialized { get; }
        IReadOnlyList<AudioBusDefinition> Buses { get; }

        void Initialize(DreamyAudioProfile profile);
        AudioPlayResult Play(AudioKey key);
        AudioPlayResult Play(AudioKey key, Vector3 position);
        AudioPlayResult PlayAttached(AudioKey key, Transform target);
        AudioHandle PlayLoop(AudioKey key);
        AudioHandle PlayMusic(AudioKey key, AudioTransition transition);
        bool Stop(AudioHandle handle, AudioTransition transition = default);
        void StopBus(AudioBusId bus, AudioTransition transition = default);
        void PauseBus(AudioBusId bus);
        void ResumeBus(AudioBusId bus);
        float GetVolume(AudioBusId bus);
        void SetVolume(AudioBusId bus, float normalizedVolume, bool persist = true);
        void SetMuted(AudioBusId bus, bool muted, bool persist = true);
    }
}

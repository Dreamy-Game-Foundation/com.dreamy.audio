using UnityEngine;

namespace Dreamy.Audio
{
    [CreateAssetMenu(menuName = "Dreamy/Audio/Music File", fileName = "MusicAudioFile")]
    public sealed class MusicAudioFile : AudioFileObject
    {
        public AudioHandle Play(AudioTransition transition = default)
        {
            return DreamyAudio.PlayMusic(this, transition);
        }

        public AudioPlayResult PlayAt(Vector3 position)
        {
            return DreamyAudio.Play(this, position);
        }

        public AudioPlayResult PlayAttached(Transform target)
        {
            return DreamyAudio.PlayAttached(this, target);
        }
    }
}

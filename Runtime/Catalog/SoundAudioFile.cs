using UnityEngine;

namespace Dreamy.Audio
{
    [CreateAssetMenu(menuName = "Dreamy/Audio/Sound File", fileName = "SoundAudioFile")]
    public sealed class SoundAudioFile : AudioFileObject
    {
        public AudioPlayResult Play()
        {
            return DreamyAudio.Play(this);
        }

        public AudioPlayResult Play(Vector3 position)
        {
            return DreamyAudio.Play(this, position);
        }

        public AudioPlayResult PlayAttached(Transform target)
        {
            return DreamyAudio.PlayAttached(this, target);
        }
    }
}

using UnityEngine;

namespace Dreamy.Audio
{
    public sealed class AudioBootstrap : MonoBehaviour
    {
        [SerializeField] private DreamyAudioProfile profile;
        [SerializeField] private bool initializeOnAwake = true;

        private void Awake()
        {
            if (initializeOnAwake)
            {
                DreamyAudio.Initialize(profile);
            }
        }
    }
}

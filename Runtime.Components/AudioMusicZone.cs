using UnityEngine;

namespace Dreamy.Audio.Components
{
    public sealed class AudioMusicZone : MonoBehaviour
    {
        [SerializeField] private AudioKey musicKey = new AudioKey("core", "music.main");
        [SerializeField, Min(0f)] private float fadeSeconds = 1f;
        [SerializeField] private LayerMask layerMask = ~0;
        [SerializeField] private string requiredTag = "Player";

        private void OnTriggerEnter(Collider other)
        {
            if (!Matches(other.gameObject))
            {
                return;
            }

            DreamyAudio.PlayMusic(musicKey, new AudioTransition(fadeSeconds));
        }

        private bool Matches(GameObject other)
        {
            if ((layerMask.value & (1 << other.layer)) == 0)
            {
                return false;
            }

            return string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag);
        }
    }
}

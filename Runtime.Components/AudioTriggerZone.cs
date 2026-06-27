using UnityEngine;

namespace Dreamy.Audio.Components
{
    public sealed class AudioTriggerZone : AudioTriggerBase
    {
        [SerializeField] private bool playOnEnter = true;
        [SerializeField] private bool playOnExit;
        [SerializeField] private LayerMask layerMask = ~0;
        [SerializeField] private string requiredTag;

        private void OnTriggerEnter(Collider other)
        {
            if (playOnEnter && Matches(other.gameObject))
            {
                TryPlay(other.transform.position);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (playOnExit && Matches(other.gameObject))
            {
                TryPlay(other.transform.position);
            }
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

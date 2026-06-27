using UnityEngine;

namespace Dreamy.Audio.Components
{
    public sealed class AudioCollisionTrigger : AudioTriggerBase
    {
        [SerializeField] private bool playOnEnter = true;
        [SerializeField] private bool playOnExit;
        [SerializeField] private LayerMask layerMask = ~0;
        [SerializeField] private string requiredTag;

        private void OnCollisionEnter(Collision collision)
        {
            if (playOnEnter && Matches(collision.gameObject))
            {
                TryPlay(collision.GetContact(0).point);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (playOnExit && Matches(collision.gameObject))
            {
                TryPlay(transform.position);
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

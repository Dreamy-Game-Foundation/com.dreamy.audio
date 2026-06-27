using UnityEngine;

namespace Dreamy.Audio.Components
{
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class AudioParticleTrigger : AudioTriggerBase
    {
        public void PlayParticleAudio()
        {
            TryPlay(transform.position);
        }

        private void OnParticleCollision(GameObject other)
        {
            TryPlay(transform.position);
        }
    }
}

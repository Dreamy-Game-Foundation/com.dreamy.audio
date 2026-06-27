using UnityEngine;

namespace Dreamy.Audio.Components
{
    public abstract class AudioTriggerBase : MonoBehaviour
    {
        [SerializeField] private AudioKey audioKey = new AudioKey("core", "ui.click");
        [SerializeField, Min(0f)] private float cooldownSeconds;

        private float lastPlayTime = float.NegativeInfinity;

        public AudioKey AudioKey => audioKey;

        protected bool TryPlay()
        {
            if (Time.unscaledTime - lastPlayTime < cooldownSeconds)
            {
                return false;
            }

            lastPlayTime = Time.unscaledTime;
            return DreamyAudio.Play(audioKey).Succeeded;
        }

        protected bool TryPlay(Vector3 position)
        {
            if (Time.unscaledTime - lastPlayTime < cooldownSeconds)
            {
                return false;
            }

            lastPlayTime = Time.unscaledTime;
            return DreamyAudio.Play(audioKey, position).Succeeded;
        }
    }
}

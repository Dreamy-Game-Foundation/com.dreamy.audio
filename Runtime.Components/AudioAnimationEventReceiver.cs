using UnityEngine;

namespace Dreamy.Audio.Components
{
    public sealed class AudioAnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private string defaultCatalogId = "core";

        public void PlayAudio(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            DreamyAudio.Play(new AudioKey(defaultCatalogId, key), transform.position);
        }
    }
}

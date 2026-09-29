using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamy.Audio.Components
{
    public sealed class AudioAnimationEventReceiver : MonoBehaviour
    {
        [FormerlySerializedAs("defaultCatalogId")]
        [SerializeField] private string defaultLibraryId = "core";

        public void PlayAudio(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            DreamyAudio.Play(new AudioKey(defaultLibraryId, key), transform.position);
        }
    }
}

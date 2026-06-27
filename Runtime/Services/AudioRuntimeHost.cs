using UnityEngine;

namespace Dreamy.Audio
{
    internal sealed class AudioRuntimeHost : MonoBehaviour
    {
        public AudioService Service { get; set; }

        private void Update()
        {
            Service?.Tick(Time.unscaledDeltaTime);
        }
    }
}

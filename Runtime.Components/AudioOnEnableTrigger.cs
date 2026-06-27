using UnityEngine;

namespace Dreamy.Audio.Components
{
    public sealed class AudioOnEnableTrigger : AudioTriggerBase
    {
        private void OnEnable()
        {
            TryPlay();
        }
    }
}

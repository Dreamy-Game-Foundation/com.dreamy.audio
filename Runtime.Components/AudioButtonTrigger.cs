using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Audio.Components
{
    [RequireComponent(typeof(Button))]
    public sealed class AudioButtonTrigger : AudioTriggerBase
    {
        [SerializeField] private Button button;

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            TryPlay();
        }
    }
}

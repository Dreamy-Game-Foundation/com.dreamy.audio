using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Audio
{
    public sealed class AudioSourcePool
    {
        private readonly Transform root;
        private readonly int maxCount;
        private readonly Stack<AudioSource> available = new Stack<AudioSource>();
        private int createdCount;

        public AudioSourcePool(Transform root, int initialCount, int maxCount)
        {
            this.root = root;
            this.maxCount = Mathf.Max(1, maxCount);

            for (var i = 0; i < initialCount; i++)
            {
                available.Push(CreateSource());
            }
        }

        public int CreatedCount => createdCount;
        public int AvailableCount => available.Count;

        public bool TryRent(out AudioSource source)
        {
            while (available.Count > 0)
            {
                source = available.Pop();
                if (source != null)
                {
                    source.gameObject.SetActive(true);
                    return true;
                }
            }

            if (createdCount >= maxCount)
            {
                source = null;
                return false;
            }

            source = CreateSource();
            source.gameObject.SetActive(true);
            return true;
        }

        public void Return(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.Stop();
            source.clip = null;
            source.loop = false;
            source.transform.SetParent(root, false);
            source.transform.localPosition = Vector3.zero;
            source.gameObject.SetActive(false);
            available.Push(source);
        }

        private AudioSource CreateSource()
        {
            createdCount++;
            var go = new GameObject($"DreamyAudioSource_{createdCount:000}");
            go.transform.SetParent(root, false);
            go.SetActive(false);
            return go.AddComponent<AudioSource>();
        }
    }
}

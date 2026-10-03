using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Dreamy.Audio.Tests
{
    public sealed class AudioRuntimeSmokeTests
    {
        [Test]
        public void DreamyAudio_Service_IsAvailable()
        {
            Assert.That(DreamyAudio.Service, Is.Not.Null);
        }

        [TestCase("music")]
        [TestCase("sfx")]
        public void SetVolume_UpdatesPlayingSourcesWithoutMixer_AndRestoresVolume(string busId)
        {
            var profile = ScriptableObject.CreateInstance<DreamyAudioProfile>();
            var file = ScriptableObject.CreateInstance<SoundAudioFile>();
            var clip = AudioClip.Create("Volume regression", 44100, 1, 44100, false);
            var service = new AudioService(new MemoryPreferences());
            GameObject root = null;
            try
            {
                SetField(file, "bus", busId);
                SetField(file, "clips", new List<AudioClip> { clip });
                SetField(file, "volume", 0.6f);
                SetField(file, "loop", true);
                service.Initialize(profile);
                root = (GameObject)typeof(AudioService).GetField("root", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(service);
                var bus = new AudioBusId(busId);
                service.SetVolume(bus, 0.5f, false);
                Assert.That(service.Play(file).Status, Is.EqualTo(AudioPlayStatus.Played));
                AudioSource source = null;
                foreach (var candidate in root.GetComponentsInChildren<AudioSource>())
                    if (candidate.clip == clip) source = candidate;
                Assert.That(source, Is.Not.Null);
                Assert.That(source.volume, Is.EqualTo(0.3f).Within(0.0001f));
                service.SetVolume(bus, 0f, false);
                Assert.That(source.volume, Is.Zero);
                service.SetVolume(bus, 0.5f, false);
                Assert.That(source.volume, Is.EqualTo(0.3f).Within(0.0001f));
                service.SetMuted(bus, true, false);
                Assert.That(source.volume, Is.Zero);
                service.SetMuted(bus, false, false);
                Assert.That(source.volume, Is.EqualTo(0.3f).Within(0.0001f));
                Assert.That(service.GetVolume(bus), Is.EqualTo(0.5f));
            }
            finally
            {
                if (root != null) Object.DestroyImmediate(root);
                Object.DestroyImmediate(file);
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(clip);
            }
        }

        private static void SetField(AudioFileObject file, string name, object value) =>
            typeof(AudioFileObject).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(file, value);

        private sealed class MemoryPreferences : IAudioPreferenceStore
        {
            private readonly Dictionary<string, float> values = new Dictionary<string, float>();
            public bool TryGetFloat(string key, out float value) => values.TryGetValue(key, out value);
            public void SetFloat(string key, float value) => values[key] = value;
        }
    }
}

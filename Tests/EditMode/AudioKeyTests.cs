using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace Dreamy.Audio.Tests
{
    public sealed class AudioKeyTests
    {
        [Test]
        public void AudioKey_WithLibraryAndKey_IsValid()
        {
            var key = new AudioKey("core", "ui.click");

            Assert.That(key.IsValid, Is.True);
            Assert.That(AudioKey.TryParse(key.Id, out var parsed), Is.True);
            Assert.That(parsed, Is.EqualTo(key));
            Assert.That(key.ToString(), Is.EqualTo("core/ui.click"));
        }

        [Test]
        public void AudioKey_WithWhitespace_IsInvalid()
        {
            var key = new AudioKey("core", "bad key");

            Assert.That(key.IsValid, Is.False);
        }

        [Test]
        public void AudioKey_BackingFields_AreWritableForUnitySerialization()
        {
            var fields = typeof(AudioKey).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(fields, Has.Some.Matches<FieldInfo>(field => field.Name == "libraryId" && !field.IsInitOnly));
            Assert.That(fields, Has.Some.Matches<FieldInfo>(field => field.Name == "key" && !field.IsInitOnly));
        }
        [Test]
        public void NewProfile_HasCasualDefaultBuses()
        {
            var profile = ScriptableObject.CreateInstance<DreamyAudioProfile>();

            Assert.That(profile.Buses, Has.Count.EqualTo(5));
            Assert.That(profile.TryGetBus(AudioBusId.Music, out var music), Is.True);
            Assert.That(music.DefaultVolume, Is.EqualTo(0.8f));
            Assert.That(profile.TryGetBus(AudioBusId.Sfx, out _), Is.True);
            Assert.That(profile.TryGetBus(AudioBusId.Ui, out _), Is.True);
            Assert.That(profile.TryGetBus(AudioBusId.Voice, out _), Is.True);
            Assert.That(profile.TryGetBus(AudioBusId.Ambience, out var ambience), Is.True);
            Assert.That(ambience.DefaultVolume, Is.EqualTo(0.7f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void AddMissingCasualDefaultBuses_DoesNotReplaceExistingBus()
        {
            var profile = ScriptableObject.CreateInstance<DreamyAudioProfile>();
            var originalMusic = profile.Buses[0];

            profile.AddMissingCasualDefaultBuses();

            Assert.That(profile.Buses, Has.Count.EqualTo(5));
            Assert.That(profile.Buses[0], Is.SameAs(originalMusic));
            Object.DestroyImmediate(profile);
        }
    }
}

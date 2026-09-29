using Dreamy.Audio.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Dreamy.Audio.Tests
{
    public sealed class AudioKeyGeneratorTests
    {
        [Test]
        public void ToIdentifier_ConvertsGroupedKeyToPascalName()
        {
            Assert.That(AudioKeyGenerator.ToIdentifier("ui.click-primary"), Is.EqualTo("UiClickPrimary"));
        }

        [Test]
        public void GenerateLibrary_EmitsStringIds()
        {
            var library = ScriptableObject.CreateInstance<AudioLibrary>();
            var sound = ScriptableObject.CreateInstance<SoundAudioFile>();
            var music = ScriptableObject.CreateInstance<MusicAudioFile>();
            TestSerialized.Set(library, "libraryId", "core");
            TestSerialized.Set(library, "soundEnumName", "CoreSounds");
            TestSerialized.Set(library, "musicEnumName", "CoreMusic");
            TestSerialized.Set(sound, "key", "ui.click");
            TestSerialized.Set(music, "key", "theme.main");
            TestSerialized.Set(library, "sounds", new System.Collections.Generic.List<SoundAudioFile> { sound });
            TestSerialized.Set(library, "music", new System.Collections.Generic.List<MusicAudioFile> { music });

            var generated = AudioKeyGenerator.Generate(library);

            Assert.That(generated, Does.Contain("namespace Dreamy.Audio.Generated"));
            Assert.That(generated, Does.Contain("public static class AudioLibraryIds"));
            Assert.That(generated, Does.Not.Contain("public enum"));
            Assert.That(generated, Does.Contain("UiClick"));
            Assert.That(generated, Does.Contain("ThemeMain"));
            Assert.That(generated, Does.Contain("public const string UiClick"));
            Assert.That(generated, Does.Contain("\"core/ui.click\""));
            Object.DestroyImmediate(library);
            Object.DestroyImmediate(sound);
            Object.DestroyImmediate(music);
        }
        [Test]
        public void TryValidateLibrary_RejectsDuplicateRuntimeId()
        {
            var library = ScriptableObject.CreateInstance<AudioLibrary>();
            var sound = ScriptableObject.CreateInstance<SoundAudioFile>();
            var music = ScriptableObject.CreateInstance<MusicAudioFile>();
            TestSerialized.Set(library, "libraryId", "core");
            TestSerialized.Set(sound, "key", "ui.click");
            TestSerialized.Set(music, "key", "ui.click");
            TestSerialized.Set(library, "sounds", new System.Collections.Generic.List<SoundAudioFile> { sound });
            TestSerialized.Set(library, "music", new System.Collections.Generic.List<MusicAudioFile> { music });

            var valid = AudioKeyGenerator.TryValidateLibrary(library, out var error);

            Assert.That(valid, Is.False);
            Assert.That(error, Is.EqualTo("Duplicate runtime ID: core/ui.click"));
            Object.DestroyImmediate(library);
            Object.DestroyImmediate(sound);
            Object.DestroyImmediate(music);
        }
    }
}

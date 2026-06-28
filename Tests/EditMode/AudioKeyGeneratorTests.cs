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
        public void Generate_IncludesCatalogAndKey()
        {
            var catalog = ScriptableObject.CreateInstance<DreamyAudioCatalog>();
            TestSerialized.Set(catalog, "catalogId", "core");
            TestSerialized.Set(catalog, "events", new System.Collections.Generic.List<AudioEventDefinition>
            {
                TestSerialized.CreateEvent("ui.click", AudioBusId.Sfx, TestSerialized.CreateVariant())
            });

            var generated = AudioKeyGenerator.Generate(catalog);

            Assert.That(generated, Does.Contain("UiClick"));
            Assert.That(generated, Does.Contain("new AudioKey(\"core\", \"ui.click\")"));
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void GenerateLibrary_EmitsSoundAndMusicEnums()
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

            Assert.That(generated, Does.Contain("public enum CoreSounds"));
            Assert.That(generated, Does.Contain("public enum CoreMusic"));
            Assert.That(generated, Does.Contain("UiClick"));
            Assert.That(generated, Does.Contain("ThemeMain"));
            Assert.That(generated, Does.Contain("new AudioKey(\"core\", \"ui.click\")"));
            Object.DestroyImmediate(library);
            Object.DestroyImmediate(sound);
            Object.DestroyImmediate(music);
        }
    }
}

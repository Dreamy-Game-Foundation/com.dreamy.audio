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
    }
}

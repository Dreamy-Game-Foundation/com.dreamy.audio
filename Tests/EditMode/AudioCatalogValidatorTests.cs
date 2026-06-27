using Dreamy.Audio.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Dreamy.Audio.Tests
{
    public sealed class AudioCatalogValidatorTests
    {
        [Test]
        public void ValidateProfile_WithDuplicateKey_ReturnsError()
        {
            var profile = ScriptableObject.CreateInstance<DreamyAudioProfile>();
            var catalog = ScriptableObject.CreateInstance<DreamyAudioCatalog>();
            TestSerialized.Set(catalog, "catalogId", "core");
            TestSerialized.Set(catalog, "events", new System.Collections.Generic.List<AudioEventDefinition>
            {
                TestSerialized.CreateEvent("ui.click", AudioBusId.Sfx, TestSerialized.CreateVariant()),
                TestSerialized.CreateEvent("ui.click", AudioBusId.Sfx, TestSerialized.CreateVariant())
            });
            TestSerialized.Set(profile, "buses", new System.Collections.Generic.List<AudioBusDefinition>
            {
                TestSerialized.CreateBus("sfx")
            });
            TestSerialized.Set(profile, "catalogs", new System.Collections.Generic.List<DreamyAudioCatalog> { catalog });

            var issues = AudioCatalogValidator.Validate(profile);

            Assert.That(issues.Exists(issue => issue.Code == "AUDIO_DUPLICATE_KEY"), Is.True);
            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void ValidateProfile_WithMissingClip_ReturnsError()
        {
            var profile = ScriptableObject.CreateInstance<DreamyAudioProfile>();
            var catalog = ScriptableObject.CreateInstance<DreamyAudioCatalog>();
            TestSerialized.Set(catalog, "catalogId", "core");
            TestSerialized.Set(catalog, "events", new System.Collections.Generic.List<AudioEventDefinition>
            {
                TestSerialized.CreateEvent("ui.click", AudioBusId.Sfx, new AudioVariant())
            });
            TestSerialized.Set(profile, "buses", new System.Collections.Generic.List<AudioBusDefinition>
            {
                TestSerialized.CreateBus("sfx")
            });
            TestSerialized.Set(profile, "catalogs", new System.Collections.Generic.List<DreamyAudioCatalog> { catalog });

            var issues = AudioCatalogValidator.Validate(profile);

            Assert.That(issues.Exists(issue => issue.Code == "AUDIO_MISSING_CLIP"), Is.True);
            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(profile);
        }
    }
}

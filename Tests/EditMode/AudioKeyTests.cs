using NUnit.Framework;

namespace Dreamy.Audio.Tests
{
    public sealed class AudioKeyTests
    {
        [Test]
        public void AudioKey_WithCatalogAndKey_IsValid()
        {
            var key = new AudioKey("core", "ui.click");

            Assert.That(key.IsValid, Is.True);
            Assert.That(key.ToString(), Is.EqualTo("core:ui.click"));
        }

        [Test]
        public void AudioKey_WithWhitespace_IsInvalid()
        {
            var key = new AudioKey("core", "bad key");

            Assert.That(key.IsValid, Is.False);
        }
    }
}

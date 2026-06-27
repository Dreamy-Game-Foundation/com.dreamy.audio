using NUnit.Framework;

namespace Dreamy.Audio.Tests
{
    public sealed class AudioRuntimeSmokeTests
    {
        [Test]
        public void DreamyAudio_Service_IsAvailable()
        {
            Assert.That(DreamyAudio.Service, Is.Not.Null);
        }
    }
}

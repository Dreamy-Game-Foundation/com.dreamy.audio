namespace Dreamy.Audio
{
    public interface IAudioPreferenceStore
    {
        bool TryGetFloat(string key, out float value);
        void SetFloat(string key, float value);
    }
}

namespace Dreamy.Audio
{
    public readonly struct AudioPlayResult
    {
        public AudioPlayResult(AudioPlayStatus status, AudioHandle handle, string message)
        {
            Status = status;
            Handle = handle;
            Message = message;
        }

        public AudioPlayStatus Status { get; }
        public AudioHandle Handle { get; }
        public string Message { get; }
        public bool Succeeded => Status == AudioPlayStatus.Played;

        public static AudioPlayResult Played(AudioHandle handle)
        {
            return new AudioPlayResult(AudioPlayStatus.Played, handle, string.Empty);
        }

        public static AudioPlayResult Fail(AudioPlayStatus status, string message)
        {
            return new AudioPlayResult(status, AudioHandle.Invalid, message);
        }
    }
}

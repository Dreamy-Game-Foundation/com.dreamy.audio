namespace Dreamy.Audio
{
    public enum AudioEventType
    {
        OneShot,
        Loop,
        Music,
        Ambience,
        Voice
    }

    public enum AudioVariantSelectionMode
    {
        Random,
        WeightedRandom,
        Sequential
    }

    public enum AudioTimeMode
    {
        Scaled,
        Unscaled,
        IgnoreListenerPause
    }

    public enum AudioPlayStatus
    {
        Played,
        Rejected,
        MissingService,
        MissingProfile,
        MissingKey,
        MissingClip,
        Muted,
        Cooldown,
        InstanceLimit,
        PoolLimit
    }
}

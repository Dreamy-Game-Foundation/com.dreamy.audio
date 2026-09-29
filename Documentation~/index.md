# Dreamy Audio

## Quick start

1. Create a `DreamyAudioProfile` and an `AudioLibrary` through `Tools/Dreamy/Audio/Create`.
2. Open `Tools/Dreamy/Audio/Audio Library`, then drag AudioClips or folders into Sound or Music groups.
3. Assign the Libraries directly to the Profile.
4. Add `AudioBootstrap` to a startup scene or initialize `DreamyAudio` from game bootstrap code.
5. Call `DreamyAudio.Play(new AudioKey("sfx", "ui.click"))`, `DreamyAudio.Play(soundFile)`, or `DreamyAudio.PlayMusic(musicFile)`.

Generated string constants should live in the consuming project, not inside this package. Use the Library window to export them.

## Tools

- `Tools/Dreamy/Audio`: profile and library creation, visual authoring, preview, validation, and string-ID generation.

# Dreamy Audio

## Quick start

1. Create a `DreamyAudioProfile` through `Tools/Dreamy/Audio`.
2. Create a `DreamyAudioCatalog`.
3. Either add audio events directly to the catalog, or create an `AudioLibrary`.
4. For library-driven setup, select `AudioClip` assets and run `Tools/Dreamy/Audio File Wizard` to create `SoundAudioFile` or `MusicAudioFile` assets.
5. Assign the library to the catalog and the catalog to the profile.
6. Add `AudioBootstrap` to a startup scene or initialize `DreamyAudio` from game bootstrap code.
7. Call `DreamyAudio.Play(new AudioKey("core", "ui.click"))`, `DreamyAudio.Play(soundFile)`, or `DreamyAudio.PlayMusic(musicFile)`.

Generated key constants and enums should live in the consuming project, not inside this package. Use `Tools/Dreamy/Audio` to generate catalog keys or library sound/music enums.

## Tools

- `Tools/Dreamy/Audio`: profile, catalog, library creation, validation, key generation, enum generation.
- `Tools/Dreamy/Audio File Wizard`: batch converts selected clips or folders into audio file assets.
- `Tools/Dreamy/Audio Playback Tool`: Play Mode preview for direct file assets, library entries, or typed keys.

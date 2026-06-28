# Dreamy Audio

Dreamy Audio is a Unity Package Manager package for catalog-driven audio playback in DreamyBase projects.

The package provides runtime playback APIs, typed audio keys, mixer bus volume controls, AudioSource pooling, editor validation, gameplay trigger components, audio file assets, audio libraries, enum generation, a file wizard, and a runtime playback tool.

## Editor workflow

1. Open `Tools/Dreamy/Audio` to create a profile, catalog, and library.
2. Select `AudioClip` assets or folders, then open `Tools/Dreamy/Audio File Wizard`.
3. Generate `SoundAudioFile` or `MusicAudioFile` assets and optionally add them to an `AudioLibrary`.
4. Add the library to a `DreamyAudioCatalog`, assign the catalog to a `DreamyAudioProfile`, then initialize with `AudioBootstrap`.
5. Generate keys or enums into the consuming project under `Assets/Generated/DreamyAudio`.
6. Use `Tools/Dreamy/Audio Playback Tool` while in Play Mode to preview keys or file assets.

See `AUDIO_PACKAGE_PLAN.md` for the full implementation roadmap.

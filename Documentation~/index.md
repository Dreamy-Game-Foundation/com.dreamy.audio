# Dreamy Audio

## Quick start

1. Create a `DreamyAudioProfile` through `Tools/Dreamy/Audio`.
2. Create a `DreamyAudioCatalog`.
3. Add audio events and clips to the catalog.
4. Assign the catalog to the profile.
5. Add `AudioBootstrap` to a startup scene or initialize `DreamyAudio` from game bootstrap code.
6. Call `DreamyAudio.Play(new AudioKey("core", "ui.click"))`.

Generated key constants should live in the consuming project, not inside this package.

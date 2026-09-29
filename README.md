# Dreamy Audio

Dreamy Audio is a Unity 6 audio package built around a simple flow:

`DreamyAudioProfile -> AudioLibrary -> SoundAudioFile / MusicAudioFile`

It provides visual audio authoring, editor preview, trim/loop/fade playback, AudioSource pooling, mixer-bus volume control, and stable string IDs.

## Setup

1. Create a profile with **Tools/Dreamy/Audio/Create/Profile**.
2. Create a library with **Tools/Dreamy/Audio/Create/Library**.
3. Open **Tools/Dreamy/Audio/Audio Library** and select the library.
4. Add Sound or Music groups, then drag AudioClips or folders into a group.
5. The new Profile already includes `music`, `sfx`, `ui`, `voice`, and `ambience`; add the library to its **Libraries** list.
6. Add `AudioBootstrap` to the startup scene and assign the Profile.
7. Use **EXPORT AUDIO LIBRARY IDS** to create `Assets/DreamyGenerated/Audio/DreamyAudioLibraryIds.cs`.

The **Casual Audio Preset** sample provides a ready-to-use Profile, SFX/Music Libraries, groups, and common buses.

## Authoring

The Audio Library window supports:

- Drag and drop for AudioClips, folders, and existing audio-file assets.
- Sound/Music groups, rename/delete, drag reordering, and **Copy ID**.
- Duplicate-ID warnings before export.
- Persistent last-selected Library and output folder.

Select an audio-file asset to edit its playback:

- Multiple clips with **Random**, **Weighted Random**, or **Sequential** selection.
- **Never Repeat** for variation.
- Non-destructive playback range and **Intro Then Loop** points.
- Preview, pause, restart, fade in/out, random pitch, cooldown, instance limit, spatial settings, and mixer routing.

A runtime ID is always `library-id/audio.key`, for example `sfx/ui.click`. IDs may use letters, digits, `_`, `-`, and `.`.

## Runtime

```csharp
using Dreamy.Audio;
using Dreamy.Audio.Generated;

var result = DreamyAudio.Play(AudioLibraryIds.Sounds.UiClick);
DreamyAudio.Play("sfx/ui.click");
DreamyAudio.Play("sfx/enemy.hit", hitPoint);
DreamyAudio.PlayAttached(hitAudioFile, enemyTransform);

var music = DreamyAudio.PlayMusic(AudioLibraryIds.Music.Menu, new AudioTransition(0.25f));
DreamyAudio.Stop(music, new AudioTransition(0.5f));
```

`AudioPlayResult` exposes `Succeeded`, `Status`, and `Message`. Common failures include `MissingProfile`, `MissingKey`, `MissingClip`, `Muted`, `Cooldown`, `InstanceLimit`, and `PoolLimit`.

## Buses

Configure buses on `DreamyAudioProfile`. The casual preset includes `music`, `sfx`, `ui`, `voice`, and `ambience`.

```csharp
DreamyAudio.SetVolume(AudioBusId.Music, 0.65f);
DreamyAudio.SetMuted(AudioBusId.Sfx, true);
DreamyAudio.StopBus(AudioBusId.Music, new AudioTransition(0.25f));
```

Bus volume can persist through PlayerPrefs. Assign an AudioMixerGroup and exposed parameter on a bus when using Unity AudioMixer routing.

## Generated IDs

Exported constants use the `Dreamy.Audio.Generated` namespace and are written under `Assets`, outside package assemblies. Export is blocked when a Library contains duplicate IDs.

## Migration from Catalog

Catalog, EventDefinition, and Variant have been removed. Move each old Library into `DreamyAudioProfile.Libraries` before upgrading. Old `DreamyAudioCatalog` assets become Missing Script after the upgrade.

`AudioKey.catalogId` and `AudioAnimationEventReceiver.defaultCatalogId` are migrated to library IDs through `FormerlySerializedAs` when Unity reloads the asset.

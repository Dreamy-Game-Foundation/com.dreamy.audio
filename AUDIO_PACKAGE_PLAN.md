# Dreamy Audio Package - Ke hoach trien khai

## 1. Trang thai tai lieu

| Truong | Gia tri |
|---|---|
| Package | `com.dreamy.audio` |
| Target | DreamyBase / Unity game projects |
| Unity baseline | Unity 6, dong bo cach lam voi `com.dreamy.localization` |
| Trang thai | Ready for review before implementation |
| Repo tham khao | https://github.com/jackyyang09/Simple-Unity-Audio-Manager |
| Ten repo tham khao | Jacky's Simple Audio Manager, aka JSAM |
| Latest release tham khao | `3.1.1 Fixes`, GitHub hien thi latest ngay 2025-02-24 |
| License tham khao | MIT |

## 2. Muc tieu

Xay dung mot Unity Package Manager package `com.dreamy.audio` de chuan hoa audio runtime va editor workflow cho cac game Dreamy.

Package can dat duoc cac muc tieu sau:

- Goi sound, music, ambience, voice bang key an toan thay vi keo tha nhieu `AudioSource` trong scene.
- Co audio catalog dang `ScriptableObject`, co the sinh typed keys va validate asset.
- Quan ly volume theo bus: master, music, sfx, ambience, voice, ui.
- Ho tro fade, crossfade, loop point, pitch, random variant, cooldown, priority, pooling, 2D/3D spatial audio.
- Co component drag-and-drop cho UI button, collision, trigger, particle, animation event.
- Co editor tooling de tao catalog, scan missing clip, preview audio, validate config va migrate tu cach dung AudioSource thu cong.
- Co API runtime gon cho game jam/prototype nhung van du manh cho production.
- Co test fixture Unity 6, EditMode/PlayMode tests va package validation truoc khi release.

## 3. Danh gia repo tham khao JSAM

JSAM la mot he thong audio Unity theo huong decentralized, de dung va co kha nang scale. README cua repo neu ro cac tinh nang chinh:

- Them va play sound/music truc quan.
- Dieu khien rieng master volume, sound volume va music volume.
- Music va sound fading.
- Built-in loop point authoring, dua tren Audio Tools Library for .NET.
- Spatialized 3D sound/audio.
- Audio thay doi theo time scale.
- Tuong thich Unity built-in Audio Effects.
- Drag-and-drop playback components cho collision, trigger, particle emission va particle death.
- Co documentation trong editor/code de de mo rong.

### Nhung y tuong nen hoc tu JSAM

| JSAM concept | Ap dung cho Dreamy |
|---|---|
| Decentralized play API | Game code goi facade/service, khong can scene singleton bat buoc |
| Sound/music separation | Mo rong thanh bus model: master/music/sfx/ui/voice/ambience |
| Fade/crossfade | Thanh API co cancellation, stale request guard va transition policy |
| Loop point authoring | Tao editor loop marker, waveform preview neu kha thi, fallback sang manual seconds/samples |
| 3D spatial audio | Dinh nghia per-event spatial profile va one-shot attach target |
| Time-scale behavior | Per-event option: scaled, unscaled, dsp-time, pause-aware |
| Collision/trigger/particle components | Them component adapters production-ready, co rate limit/cooldown |
| In-editor documentation | Dung tooltip, custom inspector, validation issue co fix suggestion |

### Cac diem Dreamy nen cai tien so voi JSAM

| Area | Rủi ro khi copy y nguyen | Dreamy improvement |
|---|---|---|
| Data ownership | De bi phan tan thanh nhieu asset/file enum | Mot `DreamyAudioCatalog` la source of truth, optional sub-catalog theo feature |
| String key | Dung string raw de loi typo runtime | Typed `AudioKey`, generated constants, validator cho missing keys |
| Pooling | Neu moi play tao AudioSource se ton allocation/CPU | `AudioSourcePool` co warmup, cap, priority stealing va diagnostics |
| Mixer | Chi volume float rieng le se kho mix production | Unity `AudioMixer` group mapping, snapshot transition, exposed parameter policy |
| Async asset | Clip lon hoac Addressables co the load tre | Loader abstraction sync/async, preload groups, unload policy |
| Mobile | Nhieu simultaneous voices de drop FPS/audio crackle | Voice budget, distance culling, cooldown, max instances per key |
| Editor safety | Asset thay doi truc tiep de kho review | Validation/dry-run report, deterministic generated files |
| Testability | Singleton static kho test | Service contracts + facade mong, dependency injection optional |
| Localization | Voice/audio theo ngon ngu can tich hop | Optional bridge voi `com.dreamy.localization` hoac locale-aware variant resolver |
| Observability | Kho debug event nao dang play | Runtime debug window, active voices, pool use, rejected play reason |

## 4. Pham vi version 1

### 4.1 Runtime bat buoc

- `IAudioService` public contract.
- `DreamyAudio` static facade delegate vao service duy nhat.
- `AudioBootstrap` optional MonoBehaviour.
- `DreamyAudioProfile` cho mixer, bus, pool, default policies.
- `DreamyAudioCatalog` chua audio events.
- `AudioKey` serialized value object: group + key hoac catalog id + key.
- Play one-shot 2D.
- Play one-shot 3D at position.
- Play one-shot attached to transform.
- Play looping handle, stop by handle.
- Play music with fade/crossfade.
- Play ambience loop with fade.
- Pause/resume/stop by bus.
- Master/music/sfx/ui/voice/ambience volume get/set/persist.
- Mute/unmute per bus.
- Pitch and volume randomization per event.
- Variant selection: random, weighted, sequential, shuffle bag.
- Cooldown and max concurrent instance per event.
- Global voice budget and priority-based stealing.
- Time behavior: scaled, unscaled, ignore listener pause, respect pause.
- Pooling for AudioSource.
- Handle stale async/fade callbacks safely.
- Structured result: played, rejected, missing, loading, budget capped, muted.
- Runtime logging hook and debug counters.

### 4.2 Runtime optional cho v1 neu con thoi gian

- Addressables clip loading.
- Locale-aware voice variant resolver.
- Side-chain ducking API for voice-over over music.
- Beat/bar sync using DSP time.
- Snapshot stack for temporary gameplay states.
- Occlusion/low-pass extension point.

### 4.3 Editor bat buoc

- `Tools/Dreamy/Audio` editor window.
- Create/repair `DreamyAudioProfile`.
- Create/repair default Unity `AudioMixer`.
- Audio catalog editor with searchable key list.
- Drag clip/folder import into catalog.
- Generate typed key file.
- Validate catalog:
  - duplicate key.
  - missing clip.
  - empty variant list.
  - invalid mixer bus.
  - loop event without loop clip/policy.
  - 3D event with invalid spatial settings.
  - volume/pitch out of allowed range.
  - unbounded max instance on spam-prone event.
- Preview event trong editor.
- Scan scenes/prefabs cho direct `AudioSource.Play`, loose AudioSource va missing Dreamy component migration opportunities.
- Export validation JSON for CI.

### 4.4 Components bat buoc

- `AudioButtonTrigger`: play on `Button.onClick`.
- `AudioCollisionTrigger`: play on collision enter/stay/exit with cooldown.
- `AudioTriggerZone`: play on trigger enter/stay/exit with layer/tag filter.
- `AudioParticleTrigger`: play on particle birth/death/events when Unity API allows.
- `AudioAnimationEventReceiver`: call from AnimationEvent with key id.
- `AudioOnEnableTrigger`: simple UI/scene feedback.
- `AudioMusicZone`: switch ambience/music by zone with fade.

### 4.5 Non-goals v1

- Khong viet mot DAW/editor waveform day du.
- Khong thay the Unity AudioMixer.
- Khong viet runtime audio decoder rieng.
- Khong auto sua prefab/scene/source code neu user chua bam Apply.
- Khong dua FMOD/Wwise vao dependency mac dinh.
- Khong bat buoc Addressables cho moi project.
- Khong copy source code JSAM vao package. Chi tham khao concept va workflow.

## 5. Kien truc de xuat

### 5.1 Assembly boundaries

| Assembly | Trach nhiem | Dependencies |
|---|---|---|
| `Dreamy.Audio.Runtime` | Contracts, key, catalog, service, pooling, mixer control, playback handles | UnityEngine, Unity audio modules |
| `Dreamy.Audio.Runtime.Components` | Button/collision/trigger/particle/animation adapters | Runtime, UGUI optional |
| `Dreamy.Audio.Editor` | Catalog editor, validators, key generator, scanner, preview, setup | Runtime, UnityEditor |
| `Dreamy.Audio.Tests.EditMode` | Pure logic tests, validators, key generation | Runtime, Editor, NUnit |
| `Dreamy.Audio.Tests.PlayMode` | Playback lifecycle, pooling, fade, handles, components | Runtime, Components, Unity Test Framework |

Neu UI dependency gay conflict, tach them `Dreamy.Audio.Runtime.UI` de component Button nam rieng voi `com.unity.ugui`.

### 5.2 Package layout

```text
com.dreamy.audio/
├── package.json
├── README.md
├── CHANGELOG.md
├── LICENSE.md
├── Third Party Notices.md
├── AUDIO_PACKAGE_PLAN.md
├── Documentation~/
│   ├── index.md
│   ├── runtime-api.md
│   ├── catalog.md
│   ├── editor-workflow.md
│   ├── migration.md
│   └── ci-validation.md
├── Runtime/
│   ├── Dreamy.Audio.Runtime.asmdef
│   ├── Configuration/
│   ├── Contracts/
│   ├── Keys/
│   ├── Catalog/
│   ├── Playback/
│   ├── Pooling/
│   ├── Mixer/
│   ├── Loading/
│   └── Diagnostics/
├── Runtime.Components/
│   ├── Dreamy.Audio.Runtime.Components.asmdef
│   ├── AudioButtonTrigger.cs
│   ├── AudioCollisionTrigger.cs
│   ├── AudioTriggerZone.cs
│   ├── AudioParticleTrigger.cs
│   ├── AudioAnimationEventReceiver.cs
│   ├── AudioOnEnableTrigger.cs
│   └── AudioMusicZone.cs
├── Editor/
│   ├── Dreamy.Audio.Editor.asmdef
│   ├── Catalog/
│   ├── Setup/
│   ├── Generation/
│   ├── Scanning/
│   ├── Validation/
│   ├── Preview/
│   ├── Reporting/
│   └── Window/
├── Tests/
│   ├── EditMode/
│   └── PlayMode/
├── Samples~/
│   ├── Basic Playback/
│   ├── Music And Ambience/
│   ├── 3D Spatial Audio/
│   └── Trigger Components/
└── TestProject~/
```

### 5.3 Runtime contracts

```csharp
public interface IAudioService
{
    bool IsInitialized { get; }
    IReadOnlyList<AudioBusId> Buses { get; }

    void Initialize(DreamyAudioProfile profile);

    AudioPlayResult Play(AudioKey key);
    AudioPlayResult Play(AudioKey key, Vector3 position);
    AudioPlayResult PlayAttached(AudioKey key, Transform target);

    AudioHandle PlayLoop(AudioKey key);
    AudioHandle PlayMusic(AudioKey key, AudioTransition transition);
    AudioHandle PlayAmbience(AudioKey key, AudioTransition transition);

    bool Stop(AudioHandle handle, AudioTransition transition = default);
    void StopBus(AudioBusId bus, AudioTransition transition = default);
    void PauseBus(AudioBusId bus);
    void ResumeBus(AudioBusId bus);

    float GetVolume(AudioBusId bus);
    void SetVolume(AudioBusId bus, float normalizedVolume, bool persist = true);
    void SetMuted(AudioBusId bus, bool muted, bool persist = true);
}
```

`DreamyAudio` static facade chi goi vao `IAudioService`. Logic nam trong service de test duoc.

### 5.4 Data model

#### `DreamyAudioProfile`

- Schema version.
- Default catalog references.
- AudioMixer reference.
- Bus definitions:
  - id.
  - display name.
  - mixer group.
  - exposed volume parameter.
  - default volume.
  - persistent key.
  - voice budget.
- Pool settings:
  - initial source count.
  - max source count.
  - max virtual voices.
  - stale handle cleanup interval.
- Music policy:
  - default fade in/out.
  - crossfade curve.
  - one active music track or layered mode.
- Ambience policy:
  - one active ambience per tag/zone.
  - blend mode.
- Global logging policy.
- Editor generation settings.

#### `DreamyAudioCatalog`

- Schema version.
- Catalog id.
- Generated namespace.
- Event list.
- Optional sub-catalog links.

#### `AudioEventDefinition`

- Key.
- Display name.
- Bus.
- Event type: one-shot, loop, music, ambience, voice.
- Variants:
  - AudioClip reference.
  - weight.
  - volume multiplier.
  - pitch multiplier.
  - locale tag optional.
  - platform tag optional.
- Volume range.
- Pitch range.
- Fade in/out.
- Loop settings:
  - loop whole clip.
  - intro + loop clip.
  - loop start/end seconds.
  - loop start/end samples.
- Spatial settings:
  - 2D/3D blend.
  - min/max distance.
  - rolloff mode.
  - doppler.
  - spread.
- Mixer override optional.
- Time behavior.
- Priority.
- Max instances per key.
- Cooldown.
- Preload group.
- Tags.
- Notes.

### 5.5 Playback lifecycle

1. Caller sends `AudioKey`.
2. Service validates initialized state and resolves catalog event.
3. Bus policy checks mute, volume, voice budget.
4. Event policy checks cooldown and max instance.
5. Variant resolver chooses clip.
6. Loader returns clip synchronously or async depending on policy.
7. Pool rents `AudioSource`.
8. Source is configured from event + bus + override.
9. Playback starts with optional fade.
10. Service returns `AudioPlayResult` or `AudioHandle`.
11. Completion/fade/stop returns source to pool.
12. Diagnostics record result and reason.

### 5.6 Pooling policy

- Prewarm pool in `Initialize`.
- Separate pools optional by AudioMixerGroup only if profiling proves useful.
- Never instantiate unlimited sources.
- If pool cap hit:
  - reject low-priority event, or
  - steal lowest-priority oldest source if event priority allows.
- One-shot handles can be lightweight, loop/music handles must be stable.
- Source cleanup must survive scene unload and destroyed attached target.

### 5.7 Mixer and volume policy

- Prefer Unity `AudioMixer` for final volume control.
- Store user-facing volume in linear 0..1.
- Convert to dB with a safe floor, for example mute floor around `-80dB`.
- Persist per bus through injectable preference store, default `PlayerPrefs`.
- Do not let each event bypass bus volume.
- Support temporary snapshot transition for gameplay state, pause, slow motion, underwater, low health.

### 5.8 Async/loading policy

V1 can start with direct `AudioClip` references. Thiet ke phai de mo:

- `IAudioClipProvider` abstraction.
- Direct reference provider default.
- Addressables provider optional later.
- Preload group: none, startup, scene, manual.
- Ref counting or retain policy for loaded clips.
- Missing asset returns structured failure, khong throw trong gameplay path.

### 5.9 Localization/voice bridge

Khong bat buoc cho v1, nhung nen de hook:

- Event variant co `localeCode`.
- `IAudioLocaleProvider` returns current locale.
- Resolver thu exact locale, parent language, default variant.
- Neu project co `com.dreamy.localization`, bridge package hoac optional asmdef co the sync voi locale change.
- Voice-over can co ducking policy de giam music/ambience khi dang noi.

## 6. Editor workflow

### 6.1 Tools/Dreamy/Audio window

Tabs:

- Setup:
  - tao profile.
  - tao mixer.
  - gan bus mac dinh.
  - check package dependencies.
- Catalog:
  - search key.
  - add event.
  - drag folder import.
  - bulk assign bus.
  - preview.
- Validate:
  - rule list.
  - severity.
  - fix suggestions.
  - export JSON.
- Generate:
  - generate typed keys.
  - dry-run diff.
  - write generated file.
- Scanner:
  - scan scenes/prefabs.
  - find direct AudioSource patterns.
  - show migration hints.
- Debug:
  - active service state in play mode.
  - pool use.
  - active voices.
  - recent rejected events.

### 6.2 Typed key generation

Generate file vi du:

```csharp
namespace Dreamy.Audio.Generated
{
    public static class AudioKeys
    {
        public static readonly AudioKey UiClick = new AudioKey("core", "ui.click");
        public static readonly AudioKey MusicMainMenu = new AudioKey("core", "music.main_menu");
    }
}
```

Guardrails:

- Generated file nam trong project `Assets/Generated/DreamyAudio/`, khong nam trong package.
- Deterministic sorting.
- Preserve namespace setting.
- Validation fail neu key invalid C# identifier ma khong co sanitized alias.
- Runtime van nhan serialized `AudioKey`, khong phu thuoc vao generated constants.

### 6.3 Scanner

Scanner nen bao cao, khong auto-edit:

- `AudioSource.Play`.
- `AudioSource.PlayOneShot`.
- `AudioSource.clip =`.
- `GetComponent<AudioSource>()` trong scripts configured path.
- Scene/prefab co AudioSource khong gan qua Dreamy component.
- UI Button co sound pattern cu.
- AnimationEvent goi method audio cu.

Muc tieu scanner la giup migrate dan dan, khong pha scene.

## 7. Testing plan

### 7.1 EditMode tests

| Area | Cases |
|---|---|
| `AudioKey` | equality, invalid group/key, serialization-friendly values |
| Catalog validation | duplicate key, missing clip, invalid bus, invalid range, cooldown/max instance |
| Variant resolver | weighted, sequential, shuffle bag, locale fallback |
| Volume math | linear to dB, mute floor, clamp |
| Key generator | deterministic output, sanitized names, duplicate aliases |
| Scanner | detects fixture direct AudioSource usages, respects excludes |
| Config migration | schema version upgrade and unknown future version failure |

### 7.2 PlayMode tests

| Area | Cases |
|---|---|
| Initialization | idempotent, missing profile failure, default profile setup |
| One-shot | source rented, plays, returns to pool |
| Loop handle | start, stop, fade out, double stop safe |
| Music crossfade | old handle stops, new handle plays, final state deterministic |
| Bus volume | set/get/persist, mute/unmute |
| Cooldown | repeated spam rejects until cooldown expires |
| Max instance | caps instances per key |
| Priority | low priority rejected or stolen according to policy |
| Attached playback | follows target and stops/continues by policy when target destroyed |
| Components | button, trigger, collision call service once with filters |
| Scene unload | no leaked active voices or event subscriptions |

### 7.3 Fixture project

Dung pattern da thanh cong o `com.dreamy.localization`:

- `TestProject~/Packages/manifest.json` co dependency package by local path.
- Them `"testables": ["com.dreamy.audio"]`.
- Include `com.unity.test-framework`.
- Chay EditMode/PlayMode trong Unity 6.
- Luu `editmode-results.xml` va `playmode-results.xml` de review.

## 8. Phase plan

## Phase 0 - Research, scaffold, package foundation

### Work

- Tao `com.dreamy.audio` UPM scaffold.
- Tao `package.json`, README, changelog, license, docs folder.
- Tao asmdefs: Runtime, Runtime.Components, Editor, Tests.
- Tao empty tests va `TestProject~`.
- Spike Unity AudioMixer API, AudioSource pooling, play/fade behavior tren Unity 6.
- Xac nhan dependencies: `com.unity.ugui` neu Button component nam trong core components.

### Deliverables

- Package install duoc vao fixture Unity 6.
- Runtime assembly khong reference `UnityEditor`.
- Test runner discover package tests.

### Acceptance criteria

- Clean compile.
- EditMode empty/smoke tests pass.
- Package Validation Suite khong co blocker.

### Risk Assessment

| Risk | Likelihood (1-5) | Impact (1-5) | Score | Mitigation |
|---|---:|---:|---:|---|
| Unity package fixture khong discover tests | 3 | 4 | 12 | Dung `testables` trong fixture manifest ngay tu Phase 0 |
| UGUI dependency lam package core nang | 2 | 3 | 6 | Tach UI/Button component sang assembly rieng neu can |
| AudioMixer setup khac giua Unity versions | 2 | 4 | 8 | Spike API va luu exact Unity baseline vao README |

### Timeline

| Phase | Effort | Notes |
|---|---|---|
| Phase 0 | S, ~1 ngay | Blocker cho tat ca phase sau |

## Phase 1 - Runtime core, catalog, service API

### Work

- Implement `AudioKey`, `AudioBusId`, `AudioHandle`, `AudioPlayResult`.
- Implement `DreamyAudioProfile` va `DreamyAudioCatalog`.
- Implement `IAudioService`, `AudioService`, `DreamyAudio` facade, `AudioBootstrap`.
- Implement direct clip provider.
- Implement bus volume/mute/persistence.
- Implement basic one-shot 2D/3D/attached playback.
- Implement structured failures va logging.

### Deliverables

- Runtime API co the dung trong gameplay.
- Catalog asset co the tao thu cong.
- Basic playback sample.

### Acceptance criteria

- Missing key khong crash.
- Reinitialize deterministic.
- Set volume/mute anh huong playback moi.
- Static facade khong chua state rieng ngoai service reference.

### Risk Assessment

| Risk | Likelihood (1-5) | Impact (1-5) | Score | Mitigation |
|---|---:|---:|---:|---|
| API qua rong, kho on dinh sau khi game dung | 3 | 4 | 12 | Freeze minimal API sau Phase 1 review |
| Singleton static gay kho test | 3 | 4 | 12 | Static facade delegate vao injectable service |
| Missing clip/key bi swallow lam kho debug | 3 | 3 | 9 | Bat buoc `AudioPlayResult` co reason va debug log policy |

### Timeline

| Phase | Effort | Notes |
|---|---|---|
| Phase 1 | M, ~3 ngay | Critical path |

## Phase 2 - Pooling, fade, music, ambience

### Work

- Implement `AudioSourcePool`.
- Implement source rent/return lifecycle.
- Implement one-shot completion cleanup.
- Implement loop handle stop/fade.
- Implement music crossfade.
- Implement ambience fade and zone-ready APIs.
- Implement cooldown, max instances, priority, voice budget.
- Implement time behavior: scaled/unscaled/pause-aware.

### Deliverables

- Production-ready playback lifecycle.
- Music/ambience sample.
- Pool diagnostics.

### Acceptance criteria

- No unlimited AudioSource creation.
- Source returns to pool after one-shot.
- Stop handle twice safe.
- Crossfade final state deterministic.
- Cooldown/max instance tests pass.
- Priority cap behavior documented and tested.

### Risk Assessment

| Risk | Likelihood (1-5) | Impact (1-5) | Score | Mitigation |
|---|---:|---:|---:|---|
| Fade callback out of order gay music state sai | 3 | 5 | 15 | Dung request version/stale guard va PlayMode concurrency tests |
| Pool stealing cat sound quan trong | 3 | 4 | 12 | Priority policy + never-steal flag cho music/voice |
| Pool cleanup leak source khi scene unload | 3 | 5 | 15 | Central ownership va scene unload tests |

### Timeline

| Phase | Effort | Notes |
|---|---|---|
| Phase 2 | M, ~3 ngay | Depends Phase 1 |

## Phase 3 - Components and gameplay adapters

### Work

- Implement `AudioButtonTrigger`.
- Implement collision/trigger components with layer/tag filters.
- Implement particle trigger adapter.
- Implement animation event receiver.
- Implement on-enable trigger.
- Implement music/ambience zone.
- Add custom inspectors and helpful warnings.

### Deliverables

- Drag-and-drop runtime components tuong tu JSAM nhung co policy/cooldown/filter.
- Trigger component samples.

### Acceptance criteria

- Components do not allocate every event in normal path.
- Disabled component does not play.
- Filters respected.
- Cooldown prevents spam.
- Missing service/key shows editor warning and runtime structured failure.

### Risk Assessment

| Risk | Likelihood (1-5) | Impact (1-5) | Score | Mitigation |
|---|---:|---:|---:|---|
| Collision/particle event spam gay audio flood | 4 | 4 | 16 | Default cooldown/max instance va validation warning |
| Button assembly dependency keo UGUI vao core | 2 | 3 | 6 | Tach UI assembly neu can |
| AnimationEvent string key loi typo | 3 | 3 | 9 | Receiver support dropdown/key id va generated constants trong docs |

### Timeline

| Phase | Effort | Notes |
|---|---|---|
| Phase 3 | M, ~3 ngay | Can pooling/cooldown tu Phase 2 |

## Phase 4 - Editor tooling, validation, key generation

### Work

- Implement `Tools/Dreamy/Audio` window.
- Implement setup/repair.
- Implement catalog custom editor.
- Implement event preview.
- Implement validation rule registry.
- Implement generated key writer.
- Implement scanner for scene/prefab/source findings.
- Implement JSON report export.

### Deliverables

- Editor workflow du de team non-programmer tao va validate audio.
- CI-ready validation report.
- Generated keys.

### Acceptance criteria

- Applying generator twice with same catalog produces no diff.
- Validation blocks duplicate/missing critical errors.
- Scanner does not modify assets.
- Preview stops audio after editor preview lifecycle.
- JSON report stable for CI.

### Risk Assessment

| Risk | Likelihood (1-5) | Impact (1-5) | Score | Mitigation |
|---|---:|---:|---:|---|
| Editor preview de lai AudioSource hoac clip dang play | 3 | 3 | 9 | Central preview owner + cleanup on disable/domain reload |
| Key generator ghi de user code | 2 | 5 | 10 | Generated root rieng + dry-run diff + header warning |
| Scanner source false positive nhieu | 4 | 3 | 12 | Classify confidence, configurable include/exclude, no auto-edit |

### Timeline

| Phase | Effort | Notes |
|---|---|---|
| Phase 4 | L, ~1 tuan | Can data model on dinh |

## Phase 5 - Loading extensions, localization bridge, advanced mix

### Work

- Implement `IAudioClipProvider` extension point.
- Add optional Addressables provider neu project can.
- Add locale provider interface and resolver.
- Add ducking API for voice-over.
- Add snapshot stack for gameplay states.
- Add preload groups and manual load/unload.

### Deliverables

- Extension-ready package cho production.
- Optional integration doc voi localization.
- Advanced music/voice workflow.

### Acceptance criteria

- Direct clip provider van la default don gian.
- Locale fallback deterministic.
- Ducking returns to previous mixer state correctly.
- Preload/unload reports loaded state and failures.

### Risk Assessment

| Risk | Likelihood (1-5) | Impact (1-5) | Score | Mitigation |
|---|---:|---:|---:|---|
| Addressables lam package phuc tap som | 3 | 4 | 12 | De optional/provider sau khi direct flow stable |
| Localization dependency gay circular package coupling | 2 | 5 | 10 | Dung interface bridge hoac optional asmdef, khong hard dependency mac dinh |
| Snapshot stack restore sai khi nested states | 3 | 4 | 12 | Stack token handle va tests cho nested enter/exit |

### Timeline

| Phase | Effort | Notes |
|---|---|---|
| Phase 5 | M-L, ~3-5 ngay | Co the day sau v1 neu can release nhanh |

## Phase 6 - Hardening, docs, samples, release candidate

### Work

- Complete EditMode/PlayMode tests.
- Add samples: basic, music/ambience, 3D, trigger components.
- Write docs: install, API, catalog, editor, migration, CI.
- Run package validation.
- Pilot integrate vao mot Dreamy game.
- Fix API rough edges before `1.0.0`.

### Deliverables

- Release candidate.
- Pilot report.
- Production docs.

### Acceptance criteria

- Clean install in Unity 6.
- Tests pass in fixture.
- Samples import without package edits.
- One pilot project can replace common direct AudioSource usages.
- No known event/source leak.
- Package Validation Suite passes.

### Risk Assessment

| Risk | Likelihood (1-5) | Impact (1-5) | Score | Mitigation |
|---|---:|---:|---:|---|
| Fixture pass nhung real project fail | 3 | 5 | 15 | Bat buoc pilot integration truoc stable release |
| Public API doi sau khi team da dung | 3 | 4 | 12 | RC period va API review |
| Docs thieu migration cases | 3 | 3 | 9 | Lay scanner findings tu pilot de bo sung migration doc |

### Timeline

| Phase | Effort | Notes |
|---|---|---|
| Phase 6 | M, ~3 ngay + pilot | Final release gate |

## 9. Timeline tong hop

| Phase | Estimate | Dependency |
|---|---|---|
| Phase 0 | S, ~1 ngay | None |
| Phase 1 | M, ~3 ngay | Phase 0 |
| Phase 2 | M, ~3 ngay | Phase 1 |
| Phase 3 | M, ~3 ngay | Phase 2 |
| Phase 4 | L, ~1 tuan | Phase 1 data model stable |
| Phase 5 | M-L, ~3-5 ngay | Phase 2/4 |
| Phase 6 | M, ~3 ngay + pilot | All prior phases |
| Total | Khoang 4-6 tuan engineering | Critical path: 0, 1, 2, 4, 6 |

Neu can MVP nhanh, co the release `0.1.0` sau Phase 2 + basic docs, chua co scanner/Addressables/localization bridge.

## 10. Definition of Done

Package duoc xem la hoan thanh cho `1.0.0` khi:

- Install clean trong Unity 6.
- Runtime assemblies khong reference `UnityEditor`.
- `DreamyAudio.Play(...)`, music crossfade, ambience, 3D one-shot, attached one-shot hoat dong on PlayMode.
- Pooling khong tao source vo han.
- Bus volume/mute/persist hoat dong qua mixer.
- Cooldown, max instance, priority voice budget co tests.
- Editor catalog co validation va preview.
- Generated keys deterministic.
- Scanner chi report, khong auto modify.
- EditMode va PlayMode tests pass trong `TestProject~`.
- Package Validation Suite pass.
- Docs/samples cover install, runtime API, catalog, editor, migration.
- Mot pilot Dreamy project tich hop duoc ma khong can sua package local.

## 11. Guardrails khi implement

- Khong copy JSAM source code; chi hoc feature va workflow.
- Khong de generated project assets trong package folder.
- Khong expose editor type trong runtime API.
- Khong goi blocking wait tren Addressables/async clip load trong gameplay.
- Khong silently fallback missing key/clip; phai co structured result/log.
- Khong instantiate unlimited AudioSource.
- Khong auto-edit scene/prefab/source tu scanner.
- Khong hard-code bus/key trong service core.
- Khong release `1.0.0` truoc pilot project.
- Khong lam static singleton chua toan bo logic; facade phai mong.

## 12. De xuat execution order

1. Review plan nay va chot MVP scope: direct clip only hay can Addressables ngay.
2. Implement Phase 0 scaffold + fixture.
3. Implement Phase 1 runtime minimal API.
4. API review ngan truoc khi implement pooling/music/editor.
5. Implement Phase 2 pooling/fade/music.
6. Implement Phase 3 components.
7. Implement Phase 4 editor validation/generator.
8. Chay pilot trong mot project Dreamy.
9. Quyet dinh Phase 5 dua vao `0.2.0` hay `1.0.0`.

Implementation handoff:

```text
omg-cook com.dreamy.audio/AUDIO_PACKAGE_PLAN.md
```

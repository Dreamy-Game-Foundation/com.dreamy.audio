# Dreamy Audio

Package thuộc Dreamy Game Studio. Hướng dẫn dưới đây mô tả cấu trúc, cách cài vào project và tích hợp ở root/scene.

## Cài package

Dùng Unity 6000.0 trở lên. Sandbox đã tham chiếu package bằng `file:../LocalPackages/com.dreamy.audio`. Project khác dùng Package Manager > + > Install package from disk và chọn package.json, hoặc Git URL của repository nội bộ. Cài cả dependency Dreamy/Git vào manifest của game; version dependency không tự cấu hình registry riêng.

Dependency trực tiếp theo package.json:

- `com.unity.ugui` (2.0.0)
- `com.unity.modules.audio` (1.0.0)
- `com.unity.modules.physics` (1.0.0)
- `com.unity.modules.particlesystem` (1.0.0)

## Cấu trúc và asmdef

| Assembly | Reference | Phạm vi |
| --- | --- | --- |
| `Dreamy.Audio.Editor` | Dreamy.Audio.Runtime, Dreamy.Audio.Runtime.Components | Chỉ Editor |
| `Dreamy.Audio.Runtime` |  | Runtime |
| `Dreamy.Audio.Runtime.Components` | Dreamy.Audio.Runtime, Unity.ugui, UnityEngine.PhysicsModule, UnityEngine.ParticleSystemModule | Runtime |

Trong asmdef của game, thêm assembly chứa API trực tiếp sử dụng. Code bootstrap reference thêm Core/DataConfig/Datasave/Economy theo nhu cầu; code async reference UniTask. Code gọi type sample reference assembly sample. Giữ Editor reference trong asmdef Editor-only.

## Cấu trúc và tạo dữ liệu

Runtime/Catalog và Configuration chứa AudioLibrary, file âm thanh và DreamyAudioProfile; Playback/Pooling/Services xử lý phát, pool và bus. Runtime.Components chứa bootstrap/trigger cho scene. Editor chứa cửa sổ authoring. Luồng dữ liệu: Profile → Libraries → SoundAudioFile/MusicAudioFile → AudioClip.

1. Tạo Profile tại Tools/Dreamy/Audio/Create/Profile và Library tại Tools/Dreamy/Audio/Create/Library.
2. Mở Tools/Dreamy/Audio/Audio Library, tạo nhóm Sound/Music, kéo clip hoặc folder vào nhóm.
3. Thêm Library vào Libraries của Profile; chỉnh bus, clip variation, trim/loop/fade và giới hạn playback.
4. Dùng EXPORT AUDIO LIBRARY IDS để tạo constant trong Assets/DreamyGenerated/Audio. Kiểm tra ID trùng trước export.

## Cài audio ở GameInstaller

Profile là field được gán trong Inspector của root. Nếu root tự initialize, chọn luồng này thay cho một AudioBootstrap khác chạy song song.

```csharp
using Dreamy.Audio;
using Dreamy.Core;

DreamyAudio.Initialize(audioProfile);
ServiceLocator.Register<IAudioService>(DreamyAudio.Service);
```

Reference Dreamy.Core.Runtime nếu dùng ServiceLocator. Với scene đơn giản, thêm AudioBootstrap và gán Profile. Khi teardown root, unregister IAudioService theo lifecycle game.

```csharp
DreamyAudio.Play("sfx/ui.click");
var music = DreamyAudio.PlayMusic("music/menu", new AudioTransition(0.25f));
DreamyAudio.Stop(music, new AudioTransition(0.5f));
DreamyAudio.SetVolume(AudioBusId.Music, 0.65f);
DreamyAudio.SetMuted(AudioBusId.Sfx, true);
```

Các ID là ví dụ; phải có file tương ứng trong Library. ID có dạng libraryId/key. AudioPlayResult cho biết Succeeded, Status và Message; kiểm tra khi không phát được. Bus có thể lưu volume qua PlayerPrefs; gán AudioMixerGroup và exposed parameter nếu dùng mixer.

Import Casual Audio Preset, gán clip vào Library và Profile cho bootstrap. Basic Playback hướng dẫn thiết lập tối thiểu. ID export nằm trong assembly của Assets, nên code dùng constant phải reference assembly chứa file được sinh.

Khi chuyển từ phiên bản Catalog cũ, đưa Library vào Profile.Libraries. Catalog/EventDefinition/Variant đã bị loại bỏ; asset DreamyAudioCatalog cũ cần được xử lý khi nâng cấp.
## Import sample

Mở Window > Package Manager, chọn Dreamy Audio > Samples > Import. Unity chép vào Assets/Samples/Dreamy Audio/0.1.0/. Chuyển cả folder nếu tùy biến, giữ .meta và reference prefab; không giữ bản script/asmdef hoặc Resources document trùng.

- **Casual Audio Preset**: nguồn `Samples~/Casual Audio Preset`.

- **Basic Playback**: nguồn `Samples~/Basic Playback`.

## Asset và Addressables

Profile trong ví dụ được root reference trực tiếp. Không cần đưa nó vào group để phát âm thanh. Nếu game muốn tải Profile bằng Addressables, đưa asset vào group, đặt address ổn định và load bằng loader của game trước DreamyAudio.Initialize. Audio không tự tra string ID âm thanh như một Addressables key; ID library/key thuộc Library.

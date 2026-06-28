using System.Collections.Generic;
using Dreamy.Audio;

namespace Dreamy.Audio.Editor
{
    public static class AudioCatalogValidator
    {
        public static List<AudioValidationIssue> Validate(DreamyAudioProfile profile)
        {
            var issues = new List<AudioValidationIssue>();
            if (profile == null)
            {
                issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_PROFILE_NULL", "Audio profile is null."));
                return issues;
            }

            if (profile.Catalogs.Count == 0)
            {
                issues.Add(new AudioValidationIssue(AudioValidationSeverity.Warning, "AUDIO_NO_CATALOG", "Audio profile has no catalogs."));
            }

            var catalogIds = new HashSet<string>();
            var keys = new HashSet<string>();
            for (var i = 0; i < profile.Catalogs.Count; i++)
            {
                var catalog = profile.Catalogs[i];
                if (catalog == null)
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_NULL_CATALOG", $"Catalog slot {i} is null."));
                    continue;
                }

                if (!AudioKey.IsValidPart(catalog.CatalogId))
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_INVALID_CATALOG_ID", $"Catalog '{catalog.name}' has invalid id '{catalog.CatalogId}'."));
                }

                if (!catalogIds.Add(catalog.CatalogId))
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_DUPLICATE_CATALOG_ID", $"Duplicate catalog id '{catalog.CatalogId}'."));
                }

                ValidateCatalog(profile, catalog, keys, issues);
                ValidateLibraries(profile, catalog, keys, issues);
            }

            return issues;
        }

        public static List<AudioValidationIssue> ValidateCatalog(DreamyAudioProfile profile, DreamyAudioCatalog catalog)
        {
            var issues = new List<AudioValidationIssue>();
            ValidateCatalog(profile, catalog, new HashSet<string>(), issues);
            return issues;
        }

        private static void ValidateCatalog(DreamyAudioProfile profile, DreamyAudioCatalog catalog, HashSet<string> keys, List<AudioValidationIssue> issues)
        {
            for (var i = 0; i < catalog.Events.Count; i++)
            {
                var audioEvent = catalog.Events[i];
                if (audioEvent == null)
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_NULL_EVENT", $"{catalog.CatalogId}: event slot {i} is null."));
                    continue;
                }

                var fullKey = $"{catalog.CatalogId}:{audioEvent.Key}";
                if (!AudioKey.IsValidPart(audioEvent.Key))
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_INVALID_KEY", $"Audio key '{fullKey}' is invalid."));
                }

                if (!keys.Add(fullKey))
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_DUPLICATE_KEY", $"Duplicate audio key '{fullKey}'."));
                }

                if (profile != null && !profile.TryGetBus(audioEvent.Bus, out _))
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_INVALID_BUS", $"Audio key '{fullKey}' references missing bus '{audioEvent.Bus}'."));
                }

                if (audioEvent.Variants.Count == 0)
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_NO_VARIANTS", $"Audio key '{fullKey}' has no variants."));
                    continue;
                }

                var hasClip = false;
                for (var j = 0; j < audioEvent.Variants.Count; j++)
                {
                    var variant = audioEvent.Variants[j];
                    if (variant != null && variant.Clip != null)
                    {
                        hasClip = true;
                    }
                }

                if (!hasClip)
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_MISSING_CLIP", $"Audio key '{fullKey}' has no playable clips."));
                }

                if (audioEvent.CooldownSeconds <= 0f && audioEvent.MaxInstances == 0 && audioEvent.EventType == AudioEventType.OneShot)
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Warning, "AUDIO_UNBOUNDED_ONESHOT", $"Audio key '{fullKey}' has no cooldown or max instance cap."));
                }
            }
        }

        public static List<AudioValidationIssue> ValidateLibrary(DreamyAudioProfile profile, AudioLibrary library)
        {
            var issues = new List<AudioValidationIssue>();
            ValidateLibrary(profile, library, library != null ? library.LibraryId : string.Empty, new HashSet<string>(), issues);
            return issues;
        }

        private static void ValidateLibraries(DreamyAudioProfile profile, DreamyAudioCatalog catalog, HashSet<string> keys, List<AudioValidationIssue> issues)
        {
            for (var i = 0; i < catalog.Libraries.Count; i++)
            {
                var library = catalog.Libraries[i];
                if (library == null)
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_NULL_LIBRARY", $"{catalog.CatalogId}: library slot {i} is null."));
                    continue;
                }

                ValidateLibrary(profile, library, catalog.CatalogId, keys, issues);
            }
        }

        private static void ValidateLibrary(DreamyAudioProfile profile, AudioLibrary library, string catalogId, HashSet<string> keys, List<AudioValidationIssue> issues)
        {
            if (library == null)
            {
                issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_LIBRARY_NULL", "Audio library is null."));
                return;
            }

            ValidateFiles(profile, catalogId, "sound", library.Sounds, keys, issues);
            ValidateFiles(profile, catalogId, "music", library.Music, keys, issues);
        }

        private static void ValidateFiles<T>(DreamyAudioProfile profile, string catalogId, string kind, IReadOnlyList<T> files, HashSet<string> keys, List<AudioValidationIssue> issues) where T : AudioFileObject
        {
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (file == null)
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_NULL_FILE", $"{catalogId}: {kind} file slot {i} is null."));
                    continue;
                }

                var fullKey = $"{catalogId}:{file.Key}";
                if (!AudioKey.IsValidPart(file.Key))
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_INVALID_FILE_KEY", $"Audio file key '{fullKey}' is invalid."));
                }

                if (!keys.Add(fullKey))
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_DUPLICATE_KEY", $"Duplicate audio key '{fullKey}'."));
                }

                if (profile != null && !profile.TryGetBus(file.Bus, out _))
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_INVALID_BUS", $"Audio file '{fullKey}' references missing bus '{file.Bus}'."));
                }

                var hasClip = false;
                for (var j = 0; j < file.Clips.Count; j++)
                {
                    if (file.Clips[j] != null)
                    {
                        hasClip = true;
                        break;
                    }
                }

                if (!hasClip)
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Error, "AUDIO_MISSING_CLIP", $"Audio file '{fullKey}' has no playable clips."));
                }

                if (file.CooldownSeconds <= 0f && file.MaxInstances == 0 && file.EventType == AudioEventType.OneShot)
                {
                    issues.Add(new AudioValidationIssue(AudioValidationSeverity.Warning, "AUDIO_UNBOUNDED_ONESHOT", $"Audio file '{fullKey}' has no cooldown or max instance cap."));
                }
            }
        }
    }
}

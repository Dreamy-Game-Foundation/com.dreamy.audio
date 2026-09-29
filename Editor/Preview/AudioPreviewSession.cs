using System;
using System.Reflection;
using Dreamy.Audio;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    /// <summary>One editor-preview owner shared by the Library and asset inspectors.</summary>
    [InitializeOnLoad]
    internal static class AudioPreviewSession
    {
        private static readonly MethodInfo PlayPreviewClip = typeof(AudioImporter).Assembly
            .GetType("UnityEditor.AudioUtil")?.GetMethod("PlayPreviewClip", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, new[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);
        private static readonly MethodInfo StopAllPreviewClips = typeof(AudioImporter).Assembly
            .GetType("UnityEditor.AudioUtil")?.GetMethod("StopAllPreviewClips", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        private static AudioFileObject file;
        private static AudioClip clip;
        private static double segmentStartedAt;
        private static float segmentStart;
        private static float segmentEnd;
        private static bool paused;

        static AudioPreviewSession()
        {
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
        }

        public static event Action Changed;

        public static bool IsPlaying => clip != null && !paused;
        public static bool IsPaused => clip != null && paused;
        public static AudioClip Clip => clip;

        public static float CurrentTime
        {
            get
            {
                if (clip == null)
                {
                    return 0f;
                }

                return paused ? segmentStart : Mathf.Min(segmentEnd, segmentStart + (float)(EditorApplication.timeSinceStartup - segmentStartedAt));
            }
        }

        public static void Play(AudioFileObject target, AudioClip targetClip = null, float? startAt = null)
        {
            if (target == null)
            {
                Stop();
                return;
            }

            targetClip ??= target.SelectClip();
            if (targetClip == null || PlayPreviewClip == null)
            {
                Stop();
                return;
            }

            file = target;
            clip = targetClip;
            segmentStart = Mathf.Clamp(startAt ?? target.StartSeconds, target.StartSeconds, target.GetPlaybackEndSeconds(targetClip));
            segmentEnd = GetSegmentEnd(target, targetClip, segmentStart);
            paused = false;
            segmentStartedAt = EditorApplication.timeSinceStartup;
            StopAllPreviewClips?.Invoke(null, null);
            PlayPreviewClip.Invoke(null, new object[] { targetClip, Mathf.FloorToInt(segmentStart * targetClip.frequency), false });
            Changed?.Invoke();
        }

        public static void TogglePause()
        {
            if (clip == null)
            {
                return;
            }

            if (paused)
            {
                paused = false;
                segmentStartedAt = EditorApplication.timeSinceStartup;
                PlayPreviewClip?.Invoke(null, new object[] { clip, Mathf.FloorToInt(segmentStart * clip.frequency), false });
            }
            else
            {
                segmentStart = CurrentTime;
                paused = true;
                StopAllPreviewClips?.Invoke(null, null);
            }

            Changed?.Invoke();
        }

        public static void Stop()
        {
            StopAllPreviewClips?.Invoke(null, null);
            file = null;
            clip = null;
            paused = false;
            Changed?.Invoke();
        }

        private static void Update()
        {
            if (clip == null || paused)
            {
                return;
            }

            if (CurrentTime < segmentEnd)
            {
                Changed?.Invoke();
                return;
            }

            if (file != null && file.Loop)
            {
                var nextStart = file.HasLoopRegion
                    ? Mathf.Clamp(file.LoopStartSeconds, file.StartSeconds, segmentEnd)
                    : file.StartSeconds;
                Play(file, clip, nextStart);
                return;
            }

            Stop();
        }

        private static float GetSegmentEnd(AudioFileObject target, AudioClip targetClip, float currentStart)
        {
            var end = target.GetPlaybackEndSeconds(targetClip);
            if (target.HasLoopRegion && currentStart >= target.LoopStartSeconds - 0.001f)
            {
                end = Mathf.Min(end, target.LoopEndSeconds);
            }

            return Mathf.Max(currentStart + 0.001f, end);
        }
    }
}

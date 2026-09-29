using System;
using System.Reflection;
using Dreamy.Audio;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    internal static class AudioTimelineControl
    {
        private const float Height = 72f;
        private static readonly Color Background = new Color(0.075f, 0.09f, 0.12f);
        // Match Unity's familiar warm audio-preview treatment instead of a custom blue/green palette.
        private static readonly Color Wave = new Color(1f, 0.72f, 0.16f, 0.82f);
        private static readonly Color Trim = new Color(1f, 0.58f, 0.08f, 1f);
        private static readonly Color Loop = new Color(1f, 0.9f, 0.32f, 1f);
        private const float MarkerHitWidth = 9f;
        private static int activeMarker;
        private static readonly Type AudioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
        private static readonly MethodInfo GetWaveFormFast = AudioUtilType?.GetMethod("GetWaveFormFast", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(AudioClip), typeof(int), typeof(int), typeof(int), typeof(float), typeof(float) }, null);
        private static readonly MethodInfo GetWaveForm = AudioUtilType?.GetMethod("GetWaveForm", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(AudioClip), typeof(AudioImporter), typeof(int), typeof(float), typeof(float) }, null);
        private static AudioClip cachedWaveClip;
        private static int cachedWaveWidth;
        private static int cachedWaveHeight;
        private static Texture2D cachedWaveTexture;
        private static AudioClip cachedFallbackClip;
        private static int cachedFallbackColumns;
        private static float[] cachedFallbackSamples;

        public static void Draw(AudioClip clip, SerializedProperty start, SerializedProperty end, SerializedProperty loop, SerializedProperty loopMode, SerializedProperty loopStart, SerializedProperty loopEnd)
        {
            var rect = GUILayoutUtility.GetRect(1f, Height, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, Background);
            if (clip == null || clip.length <= 0f)
            {
                GUI.Label(rect, "Drop an AudioClip to edit its playback range.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            DrawWave(rect, clip);
            var playbackEnd = end.floatValue > start.floatValue ? Mathf.Min(end.floatValue, clip.length) : clip.length;
            DrawMarker(rect, start.floatValue, clip.length, Trim, "IN");
            DrawMarker(rect, playbackEnd, clip.length, Trim, "OUT");
            if (loop.boolValue && loopMode.enumValueIndex == (int)AudioLoopMode.IntroThenLoop)
            {
                DrawMarker(rect, loopStart.floatValue, clip.length, Loop, "LOOP");
                DrawMarker(rect, loopEnd.floatValue, clip.length, Loop, "END");
            }

            if (AudioPreviewSession.Clip == clip)
            {
                DrawMarker(rect, AudioPreviewSession.CurrentTime, clip.length, Color.white, null);
            }

            HandleMarkers(rect, clip, start, end, loop, loopMode, loopStart, loopEnd);
        }

        private static void DrawWave(Rect rect, AudioClip clip)
        {
            var width = Mathf.Clamp(Mathf.RoundToInt(rect.width), 32, 1024);
            var height = Mathf.Clamp(Mathf.RoundToInt(rect.height), 16, 256);
            var texture = GetWaveTexture(clip, width, height);
            if (texture != null)
            {
                var previous = GUI.color;
                GUI.color = Wave;
                GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
                GUI.color = previous;
                return;
            }

            DrawBoundedFallback(rect, clip, width);
        }

        private static Texture2D GetWaveTexture(AudioClip clip, int width, int height)
        {
            if (cachedWaveClip == clip && cachedWaveWidth == width && cachedWaveHeight == height) return cachedWaveTexture;
            cachedWaveClip = clip;
            cachedWaveWidth = width;
            cachedWaveHeight = height;
            cachedWaveTexture = null;
            try
            {
                if (GetWaveFormFast != null)
                {
                    cachedWaveTexture = GetWaveFormFast.Invoke(null, new object[] { clip, 0, 0, clip.samples, (float)width, (float)height }) as Texture2D;
                }
                else if (GetWaveForm != null)
                {
                    var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip)) as AudioImporter;
                    cachedWaveTexture = GetWaveForm.Invoke(null, new object[] { clip, importer, 0, (float)width, (float)height }) as Texture2D;
                }
            }
            catch (Exception)
            {
                cachedWaveTexture = null;
            }
            return cachedWaveTexture;
        }

        private static void DrawBoundedFallback(Rect rect, AudioClip clip, int columns)
        {
            if (cachedFallbackClip != clip || cachedFallbackSamples == null || cachedFallbackColumns != columns)
            {
                cachedFallbackClip = clip;
                cachedFallbackColumns = columns;
                cachedFallbackSamples = new float[columns];
                var probeFrames = Mathf.Min(128, Mathf.Max(1, clip.samples / columns));
                var source = new float[probeFrames * Mathf.Max(1, clip.channels)];
                for (var x = 0; x < columns; x++)
                {
                    var center = Mathf.Clamp(x * clip.samples / columns, 0, Mathf.Max(0, clip.samples - probeFrames));
                    if (!clip.GetData(source, center)) continue;
                    var peak = 0f;
                    for (var sample = 0; sample < source.Length; sample++) peak = Mathf.Max(peak, Mathf.Abs(source[sample]));
                    cachedFallbackSamples[x] = peak;
                }
            }

            var centerY = rect.center.y;
            Handles.BeginGUI();
            Handles.color = Wave;
            for (var x = 0; x < columns; x++)
            {
                var amplitude = cachedFallbackSamples[x] * rect.height * .43f;
                var position = rect.x + x * rect.width / columns;
                Handles.DrawLine(new Vector3(position, centerY - amplitude), new Vector3(position, centerY + amplitude));
            }
            Handles.EndGUI();
        }

        private static void DrawMarker(Rect rect, float seconds, float length, Color color, string label)
        {
            var x = rect.x + rect.width * Mathf.Clamp01(seconds / length);
            EditorGUI.DrawRect(new Rect(x - 1f, rect.y, 2f, rect.height), color);
            if (!string.IsNullOrEmpty(label))
            {
                var labelRect = new Rect(Mathf.Clamp(x + 3f, rect.x, rect.xMax - 40f), rect.y + 2f, 38f, 15f);
                GUI.Label(labelRect, label, EditorStyles.miniLabel);
            }
        }

        private static void HandleMarkers(Rect rect, AudioClip clip, SerializedProperty start, SerializedProperty end, SerializedProperty loop, SerializedProperty loopMode, SerializedProperty loopStart, SerializedProperty loopEnd)
        {
            var e = Event.current;
            var controlId = GUIUtility.GetControlID(FocusType.Passive, rect);
            var playbackEnd = end.floatValue > start.floatValue ? Mathf.Min(end.floatValue, clip.length) : clip.length;
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                activeMarker = FindNearestMarker(e.mousePosition.x, rect, clip.length, start.floatValue, playbackEnd, loop, loopMode, loopStart, loopEnd);
                GUIUtility.hotControl = controlId;
                e.Use();
                return;
            }

            if (GUIUtility.hotControl != controlId) return;
            if (e.type == EventType.MouseDrag)
            {
                var time = Mathf.Clamp01((e.mousePosition.x - rect.x) / rect.width) * clip.length;
                switch (activeMarker)
                {
                    case 0:
                        start.floatValue = Mathf.Min(time, playbackEnd - .001f);
                        break;
                    case 1:
                        end.floatValue = time >= start.floatValue + .001f && time < clip.length - .001f ? time : 0f;
                        break;
                    case 2:
                        loopStart.floatValue = Mathf.Clamp(time, start.floatValue, loopEnd.floatValue - .001f);
                        break;
                    case 3:
                        loopEnd.floatValue = Mathf.Clamp(time, loopStart.floatValue + .001f, playbackEnd);
                        break;
                }
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                GUIUtility.hotControl = 0;
                activeMarker = -1;
                e.Use();
            }
        }

        private static int FindNearestMarker(float mouseX, Rect rect, float length, float trimStart, float trimEnd, SerializedProperty loop, SerializedProperty loopMode, SerializedProperty loopStart, SerializedProperty loopEnd)
        {
            var candidates = new[]
            {
                rect.x + rect.width * Mathf.Clamp01(trimStart / length),
                rect.x + rect.width * Mathf.Clamp01(trimEnd / length),
                loop.boolValue && loopMode.enumValueIndex == (int)AudioLoopMode.IntroThenLoop ? rect.x + rect.width * Mathf.Clamp01(loopStart.floatValue / length) : float.MaxValue,
                loop.boolValue && loopMode.enumValueIndex == (int)AudioLoopMode.IntroThenLoop ? rect.x + rect.width * Mathf.Clamp01(loopEnd.floatValue / length) : float.MaxValue,
            };
            var nearest = 0;
            var distance = float.MaxValue;
            for (var i = 0; i < candidates.Length; i++)
            {
                var candidateDistance = Mathf.Abs(mouseX - candidates[i]);
                if (candidateDistance < distance)
                {
                    nearest = i;
                    distance = candidateDistance;
                }
            }

            if (Event.current.shift) return 1;
            return distance <= MarkerHitWidth ? nearest : (mouseX < candidates[0] + (candidates[1] - candidates[0]) * .5f ? 0 : 1);
        }
    }
}

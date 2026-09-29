using Dreamy.Audio;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    [CustomEditor(typeof(AudioFileObject), true)]
    internal sealed class DreamyAudioFileEditor : UnityEditor.Editor
    {
        private SerializedProperty key;
        private SerializedProperty displayName;
        private SerializedProperty category;
        private SerializedProperty bus;
        private SerializedProperty eventType;
        private SerializedProperty clips;
        private SerializedProperty clipWeights;
        private SerializedProperty volume;
        private SerializedProperty pitch;
        private SerializedProperty randomPitch;
        private SerializedProperty fadeIn;
        private SerializedProperty fadeOut;
        private SerializedProperty start;
        private SerializedProperty end;
        private SerializedProperty loop;
        private SerializedProperty loopMode;
        private SerializedProperty loopStart;
        private SerializedProperty loopEnd;
        private SerializedProperty selectionMode;
        private SerializedProperty neverRepeat;
        private SerializedProperty timeMode;
        private SerializedProperty priority;
        private SerializedProperty maxInstances;
        private SerializedProperty cooldown;
        private SerializedProperty spatial;
        private SerializedProperty mixerGroup;
        private SerializedProperty bypassEffects;
        private SerializedProperty bypassListenerEffects;
        private SerializedProperty bypassReverbZones;
        private bool advanced;

        private void OnEnable()
        {
            key = serializedObject.FindProperty("key"); displayName = serializedObject.FindProperty("displayName");
            category = serializedObject.FindProperty("category"); bus = serializedObject.FindProperty("bus");
            eventType = serializedObject.FindProperty("eventType"); clips = serializedObject.FindProperty("clips"); clipWeights = serializedObject.FindProperty("clipWeights");
            volume = serializedObject.FindProperty("volume"); pitch = serializedObject.FindProperty("pitch"); randomPitch = serializedObject.FindProperty("randomPitch");
            fadeIn = serializedObject.FindProperty("fadeInSeconds"); fadeOut = serializedObject.FindProperty("fadeOutSeconds");
            start = serializedObject.FindProperty("startSeconds"); end = serializedObject.FindProperty("endSeconds");
            loop = serializedObject.FindProperty("loop"); loopMode = serializedObject.FindProperty("loopMode"); loopStart = serializedObject.FindProperty("loopStartSeconds"); loopEnd = serializedObject.FindProperty("loopEndSeconds");
            selectionMode = serializedObject.FindProperty("selectionMode"); neverRepeat = serializedObject.FindProperty("neverRepeat");
            timeMode = serializedObject.FindProperty("timeMode"); priority = serializedObject.FindProperty("priority"); maxInstances = serializedObject.FindProperty("maxInstances"); cooldown = serializedObject.FindProperty("cooldownSeconds");
            spatial = serializedObject.FindProperty("spatial"); mixerGroup = serializedObject.FindProperty("mixerGroupOverride");
            bypassEffects = serializedObject.FindProperty("bypassEffects"); bypassListenerEffects = serializedObject.FindProperty("bypassListenerEffects"); bypassReverbZones = serializedObject.FindProperty("bypassReverbZones");
            AudioPreviewSession.Changed += Repaint;
        }

        private void OnDisable()
        {
            AudioPreviewSession.Changed -= Repaint;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var file = (AudioFileObject)target;
            var music = file is MusicAudioFile;
            EditorGUILayout.LabelField(music ? "Dreamy Music" : "Dreamy Sound", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Author playback visually. Trim and loop points are non-destructive: the original AudioClip is never changed.", MessageType.None);

            EditorGUILayout.PropertyField(displayName, new GUIContent("Name"));
            EditorGUILayout.PropertyField(key, new GUIContent("ID", "Stable string used at runtime. Keep it short and unique within the Library."));
            EditorGUILayout.PropertyField(category, new GUIContent("Group", "A visual group in the Audio Library window."));
            // Bus routing remains available but is intentionally kept in Advanced for the everyday Library workflow.

            DrawClipList();
            var activeClip = FirstClip();
            EditorGUILayout.Space(4f);
            AudioTimelineControl.Draw(activeClip, start, end, loop, loopMode, loopStart, loopEnd);
            DrawTransport(file, activeClip);
            DrawPlaybackRange(activeClip);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Mix", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(volume);
            EditorGUILayout.PropertyField(pitch);
            if (!music) EditorGUILayout.PropertyField(randomPitch);
            EditorGUILayout.PropertyField(fadeIn, new GUIContent("Fade In", "Seconds to fade in when playback begins."));
            EditorGUILayout.PropertyField(fadeOut, new GUIContent("Fade Out", "Seconds to fade out when this voice is stopped."));

            advanced = EditorGUILayout.Foldout(advanced, "Advanced", true);
            if (advanced)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(bus, new GUIContent("Bus", "Mixer route. Defaults should be SFX for Sound and Music for Music."));
                EditorGUILayout.PropertyField(eventType);
                EditorGUILayout.PropertyField(selectionMode);
                if (selectionMode.enumValueIndex == (int)AudioVariantSelectionMode.WeightedRandom)
                {
                    EditorGUILayout.PropertyField(clipWeights, new GUIContent("Clip Weights", "One non-negative weight per Audio Clip. Missing weights default to 1."), true);
                }
                EditorGUILayout.PropertyField(neverRepeat);
                EditorGUILayout.PropertyField(timeMode);
                EditorGUILayout.PropertyField(priority);
                EditorGUILayout.PropertyField(maxInstances);
                EditorGUILayout.PropertyField(cooldown);
                EditorGUILayout.PropertyField(spatial, true);
                EditorGUILayout.PropertyField(mixerGroup);
                EditorGUILayout.PropertyField(bypassEffects);
                EditorGUILayout.PropertyField(bypassListenerEffects);
                EditorGUILayout.PropertyField(bypassReverbZones);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawClipList()
        {
            EditorGUILayout.LabelField("Audio Clips", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(clips, true);
            var drop = GUILayoutUtility.GetRect(1f, 34f, GUILayout.ExpandWidth(true));
            GUI.Box(drop, "Drop AudioClips or folders here", EditorStyles.helpBox);
            var e = Event.current;
            if (!drop.Contains(e.mousePosition) || e.type != EventType.DragPerform) return;
            DragAndDrop.AcceptDrag();
            foreach (var clip in AudioEditorUtility.GetSelectedClips()) AddClip(clip);
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is AudioClip clip) AddClip(clip);
            }
            e.Use();
        }

        private void DrawTransport(AudioFileObject file, AudioClip clip)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = clip != null;
                if (GUILayout.Button("Restart")) AudioPreviewSession.Play(file, clip);
                var owns = AudioPreviewSession.Clip == clip;
                if (GUILayout.Button(owns && AudioPreviewSession.IsPlaying ? "Stop" : "Preview"))
                {
                    if (owns && AudioPreviewSession.IsPlaying) AudioPreviewSession.Stop(); else AudioPreviewSession.Play(file, clip);
                }
                GUI.enabled = owns;
                if (GUILayout.Button(AudioPreviewSession.IsPaused ? "Resume" : "Pause")) AudioPreviewSession.TogglePause();
                GUI.enabled = true;
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(clip == null ? "No clip" : $"{AudioPreviewSession.CurrentTime:0.00}s / {clip.length:0.00}s", EditorStyles.miniLabel, GUILayout.Width(100f));
            }
        }

        private void DrawPlaybackRange(AudioClip clip)
        {
            if (clip == null) return;
            var max = clip.length;
            var trimStart = Mathf.Clamp(start.floatValue, 0f, max);
            var trimEnd = end.floatValue > trimStart ? Mathf.Clamp(end.floatValue, trimStart + .001f, max) : max;
            EditorGUILayout.MinMaxSlider(new GUIContent("Playback Range", "Drag start/end directly on the timeline, or refine them here."), ref trimStart, ref trimEnd, 0f, max);
            using (new EditorGUILayout.HorizontalScope())
            {
                start.floatValue = EditorGUILayout.FloatField("Start", trimStart);
                var endValue = EditorGUILayout.FloatField("End", trimEnd);
                end.floatValue = endValue >= max - .001f ? 0f : Mathf.Max(start.floatValue + .001f, endValue);
            }

            EditorGUILayout.PropertyField(loop, new GUIContent("Loop", "Repeat the playback range."));
            if (!loop.boolValue) return;
            EditorGUILayout.PropertyField(loopMode, new GUIContent("Loop Style"));
            if (loopMode.enumValueIndex != (int)AudioLoopMode.IntroThenLoop) return;
            var loopA = Mathf.Clamp(loopStart.floatValue, start.floatValue, trimEnd);
            var loopB = loopEnd.floatValue > loopA ? Mathf.Clamp(loopEnd.floatValue, loopA + .001f, trimEnd) : trimEnd;
            EditorGUILayout.MinMaxSlider(new GUIContent("Loop Region", "The intro plays once; this smaller region repeats."), ref loopA, ref loopB, start.floatValue, trimEnd);
            loopStart.floatValue = loopA;
            loopEnd.floatValue = loopB;
        }

        private AudioClip FirstClip()
        {
            for (var i = 0; i < clips.arraySize; i++)
            {
                if (clips.GetArrayElementAtIndex(i).objectReferenceValue is AudioClip clip) return clip;
            }
            return null;
        }

        private void AddClip(AudioClip clip)
        {
            if (clip == null) return;
            for (var i = 0; i < clips.arraySize; i++) if (clips.GetArrayElementAtIndex(i).objectReferenceValue == clip) return;
            clips.InsertArrayElementAtIndex(clips.arraySize);
            clips.GetArrayElementAtIndex(clips.arraySize - 1).objectReferenceValue = clip;
            if (clipWeights != null)
            {
                clipWeights.InsertArrayElementAtIndex(clipWeights.arraySize);
                clipWeights.GetArrayElementAtIndex(clipWeights.arraySize - 1).floatValue = 1f;
            }
        }
    }
}

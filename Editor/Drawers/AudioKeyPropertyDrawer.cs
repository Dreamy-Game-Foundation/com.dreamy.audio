using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    [CustomPropertyDrawer(typeof(AudioKey))]
    public sealed class AudioKeyPropertyDrawer : PropertyDrawer
    {
        private const float FieldSpacing = 4f;
        private const float LibraryWidthRatio = 0.38f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var contentPosition = EditorGUI.PrefixLabel(position, label);
            var libraryProperty = property.FindPropertyRelative("libraryId");
            var keyProperty = property.FindPropertyRelative("key");

            if (libraryProperty == null || keyProperty == null)
            {
                EditorGUI.LabelField(contentPosition, "Invalid AudioKey backing fields");
                EditorGUI.EndProperty();
                return;
            }

            var libraryWidth = Mathf.Floor((contentPosition.width - FieldSpacing) * LibraryWidthRatio);
            var keyWidth = contentPosition.width - libraryWidth - FieldSpacing;
            var libraryRect = new Rect(contentPosition.x, contentPosition.y, libraryWidth, contentPosition.height);
            var keyRect = new Rect(libraryRect.xMax + FieldSpacing, contentPosition.y, keyWidth, contentPosition.height);

            libraryProperty.stringValue = EditorGUI.TextField(libraryRect, libraryProperty.stringValue);
            keyProperty.stringValue = EditorGUI.TextField(keyRect, keyProperty.stringValue);

            EditorGUI.EndProperty();
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace Dreamy.Audio.Editor
{
    [CustomPropertyDrawer(typeof(AudioKey))]
    public sealed class AudioKeyPropertyDrawer : PropertyDrawer
    {
        private const float FieldSpacing = 4f;
        private const float CatalogWidthRatio = 0.38f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var contentPosition = EditorGUI.PrefixLabel(position, label);
            var catalogProperty = property.FindPropertyRelative("catalogId");
            var keyProperty = property.FindPropertyRelative("key");

            if (catalogProperty == null || keyProperty == null)
            {
                EditorGUI.LabelField(contentPosition, "Invalid AudioKey backing fields");
                EditorGUI.EndProperty();
                return;
            }

            var catalogWidth = Mathf.Floor((contentPosition.width - FieldSpacing) * CatalogWidthRatio);
            var keyWidth = contentPosition.width - catalogWidth - FieldSpacing;
            var catalogRect = new Rect(contentPosition.x, contentPosition.y, catalogWidth, contentPosition.height);
            var keyRect = new Rect(catalogRect.xMax + FieldSpacing, contentPosition.y, keyWidth, contentPosition.height);

            catalogProperty.stringValue = EditorGUI.TextField(catalogRect, catalogProperty.stringValue);
            keyProperty.stringValue = EditorGUI.TextField(keyRect, keyProperty.stringValue);

            EditorGUI.EndProperty();
        }
    }
}

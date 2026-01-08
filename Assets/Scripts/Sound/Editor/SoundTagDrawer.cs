#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Draw Sound.SoundTag as a mask field in the Inspector to ensure multi-select appears.
[CustomPropertyDrawer(typeof(Sound.SoundTag))]
public class SoundTagDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Unity may represent enums as SerializedPropertyType.Enum, but backing value is int
        EditorGUI.BeginProperty(position, label, property);
        if (property.propertyType == SerializedPropertyType.Enum || property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
        EditorGUI.EndProperty();
    }
}
#endif


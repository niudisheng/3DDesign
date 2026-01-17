using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(SoundGroup))]
public class SoundGroupEditor : Editor
{
    SerializedProperty audioSourceProp;
    SerializedProperty volumeProp;
    SerializedProperty clipsProp;
    SerializedProperty loopProp;
    SerializedProperty soundTagProp;
    SerializedProperty pitchProp;
    SerializedProperty distanceProp;

    ReorderableList clipsList;

    private void OnEnable()
    {
        audioSourceProp = serializedObject.FindProperty("AudioSource");
        volumeProp = serializedObject.FindProperty("volume");
        clipsProp = serializedObject.FindProperty("clips");
        loopProp = serializedObject.FindProperty("loop");
        soundTagProp = serializedObject.FindProperty("soundTag");
        pitchProp = serializedObject.FindProperty("pitch");
        distanceProp = serializedObject.FindProperty("distanceSoundSettings");

        clipsList = new ReorderableList(serializedObject, clipsProp, true, true, true, true);
        clipsList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Clips");
        };

        clipsList.elementHeight = EditorGUIUtility.singleLineHeight + 6;
        clipsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var elem = clipsProp.GetArrayElementAtIndex(index);
            rect.y += 3;
            Rect fieldRect = new Rect(rect.x, rect.y, rect.width - 50, EditorGUIUtility.singleLineHeight);
            Rect btnRect = new Rect(rect.x + rect.width - 45, rect.y, 45, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(fieldRect, elem, GUIContent.none);
            var clip = elem.objectReferenceValue as AudioClip;

            EditorGUI.BeginDisabledGroup(clip == null);
            if (GUI.Button(btnRect, "▶"))
            {
                // If in play mode play through AudioSource; otherwise try Editor preview
                if (Application.isPlaying)
                {
                    AudioSource.PlayClipAtPoint(clip, Camera.main ? Camera.main.transform.position : Vector3.zero);
                }
                else
                {
                    // Editor preview (may be no-op on some versions)
                    EditorAudioPreview.Play(clip);
                }
            }
            EditorGUI.EndDisabledGroup();
        };

        clipsList.onAddCallback = (ReorderableList list) =>
        {
            clipsProp.arraySize++;
            serializedObject.ApplyModifiedProperties();
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(audioSourceProp, new GUIContent("Audio Source"));
        EditorGUILayout.Slider(volumeProp, 0f, 1f, new GUIContent("Volume"));
        EditorGUILayout.PropertyField(loopProp, new GUIContent("Loop"));
        EditorGUILayout.PropertyField(soundTagProp, new GUIContent("Sound Tag"));

        GUILayout.Space(6);
        clipsList.DoLayoutList();
        GUILayout.Space(6);

        EditorGUILayout.PropertyField(pitchProp, new GUIContent("Pitch Settings"), true);
        EditorGUILayout.PropertyField(distanceProp, new GUIContent("Distance Sound"), true);

        serializedObject.ApplyModifiedProperties();
    }
}

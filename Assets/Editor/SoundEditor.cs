using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Sound))]
public class SoundEditor : Editor
{
    SerializedProperty audioSourceProp;
    SerializedProperty volumeProp;
    SerializedProperty clipProp;
    SerializedProperty pitchProp;
    SerializedProperty loopProp;
    SerializedProperty soundTagProp;
    SerializedProperty distanceProp;

    private void OnEnable()
    {
        audioSourceProp = serializedObject.FindProperty("AudioSource");
        volumeProp = serializedObject.FindProperty("volume");
        clipProp = serializedObject.FindProperty("clip");
        pitchProp = serializedObject.FindProperty("pitch");
        loopProp = serializedObject.FindProperty("loop");
        soundTagProp = serializedObject.FindProperty("soundTag");
        distanceProp = serializedObject.FindProperty("distanceSoundSettings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header
        EditorGUILayout.LabelField("Sound", EditorStyles.boldLabel);

        // Main group
        EditorGUILayout.BeginVertical("box");
        if (audioSourceProp != null)
            EditorGUILayout.PropertyField(audioSourceProp, new GUIContent("Audio Source"));
        else
            EditorGUILayout.LabelField("Audio Source (missing)");

        EditorGUILayout.BeginHorizontal();
        if (volumeProp != null)
        {
            EditorGUILayout.LabelField("Volume", GUILayout.Width(50));
            volumeProp.floatValue = EditorGUILayout.Slider(volumeProp.floatValue, 0f, 1f);
        }
        else
        {
            EditorGUILayout.LabelField("Volume (missing)", GUILayout.Width(200));
        }

        if (loopProp != null)
        {
            EditorGUILayout.PropertyField(loopProp, new GUIContent("Loop"), GUILayout.Width(60));
        }
        else
        {
            EditorGUILayout.LabelField("Loop (missing)", GUILayout.Width(60));
        }
        EditorGUILayout.EndHorizontal();

        // Clip + preview
        EditorGUILayout.BeginHorizontal();
        if (clipProp != null)
            EditorGUILayout.PropertyField(clipProp, new GUIContent("Clip"));
        else
            EditorGUILayout.LabelField("Clip (missing)");

        var clip = clipProp != null ? clipProp.objectReferenceValue as AudioClip : null;
        EditorGUI.BeginDisabledGroup(clip == null);
        if (GUILayout.Button("▶", GUILayout.Width(30)))
        {
            if (clip != null)
            {
                if (Application.isPlaying)
                {
                    AudioSource.PlayClipAtPoint(clip, Camera.main ? Camera.main.transform.position : Vector3.zero);
                }
                else
                {
                    EditorAudioPreview.Play(clip);
                }
            }
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        // SoundTag as toolbar (single select)
        if (soundTagProp != null)
        {
            string[] tagNames = System.Enum.GetNames(typeof(Sound.SoundTag));
            int current = soundTagProp.enumValueIndex;
            int chosen = GUILayout.Toolbar(current, tagNames);
            if (chosen != current)
            {
                soundTagProp.enumValueIndex = chosen;
            }
        }
        else
        {
            EditorGUILayout.LabelField("Sound Tag (missing)");
        }

        EditorGUILayout.EndVertical();

        // Pitch and Distance groups
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        if (pitchProp != null)
            EditorGUILayout.PropertyField(pitchProp, new GUIContent("Pitch Settings"), true);
        else
            EditorGUILayout.LabelField("Pitch Settings (missing)");

        if (distanceProp != null)
            EditorGUILayout.PropertyField(distanceProp, new GUIContent("Distance Sound"), true);
        else
            EditorGUILayout.LabelField("Distance Sound (missing)");
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}

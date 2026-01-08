#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Helper to play audio preview in editor without hard dependency on EditorUtility.PlayPreviewClip signature.
/// Uses reflection to call PlayPreviewClip if available; otherwise fallbacks to using internal AudioUtil if present.
/// This avoids compile errors in some Unity versions where PlayPreviewClip may be missing or internal.
/// </summary>
public static class EditorAudioPreview
{
    private static MethodInfo playPreviewClipMethod;
    private static Type audioUtilType;
    private static MethodInfo audioUtilPlayMethod;

    static EditorAudioPreview()
    {
        // Try EditorUtility.PlayPreviewClip(AudioClip)
        var editorUtilityType = typeof(EditorUtility);
        playPreviewClipMethod = editorUtilityType.GetMethod("PlayPreviewClip", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(AudioClip) }, null);

        if (playPreviewClipMethod == null)
        {
            // Try UnityEditor.AudioUtil.PlayPreviewClip(AudioClip) (internal), different Unity versions
            audioUtilType = Assembly.GetAssembly(editorUtilityType).GetType("UnityEditor.AudioUtil");
            if (audioUtilType != null)
            {
                audioUtilPlayMethod = audioUtilType.GetMethod("PlayPreviewClip", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(AudioClip) }, null);
            }
        }
    }

    public static void Play(AudioClip clip)
    {
        if (clip == null) return;

        try
        {
            if (playPreviewClipMethod != null)
            {
                playPreviewClipMethod.Invoke(null, new object[] { clip });
                return;
            }

            if (audioUtilPlayMethod != null)
            {
                audioUtilPlayMethod.Invoke(null, new object[] { clip });
                return;
            }

            // Last resort: create a temporary audio source in editor play mode
            if (Application.isPlaying)
            {
                AudioSource.PlayClipAtPoint(clip, Camera.main ? Camera.main.transform.position : Vector3.zero);
            }
            else
            {
                Debug.LogWarning("No editor audio preview available on this Unity version.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to play preview clip via reflection: " + e.Message);
        }
    }
}
#endif

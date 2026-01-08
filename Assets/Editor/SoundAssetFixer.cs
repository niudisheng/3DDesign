#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class SoundAssetFixer
{
    [MenuItem("Tools/Sound/Fix Missing DistanceSettings on Sounds")]
    public static void FixAllSoundDistanceSettings()
    {
        string[] guids = AssetDatabase.FindAssets("t:Sound");
        int fixedCount = 0;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var sound = AssetDatabase.LoadAssetAtPath<Sound>(path);
            if (sound == null) continue;

            // If the nested settings are null, initialize and mark dirty
            var field = typeof(Sound).GetField("distanceSoundSettings");
            if (field == null) continue;
            var val = field.GetValue(sound);
            if (val == null)
            {
                field.SetValue(sound, new Sound.DistanceSoundSettings());
                EditorUtility.SetDirty(sound);
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"SoundAssetFixer: fixed {fixedCount} Sound assets (distanceSoundSettings initialized).");
    }
}
#endif

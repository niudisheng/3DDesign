using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New Sound", menuName = "Sound/Sound")]
public class Sound : ScriptableObject
{
    public string AudioSource;
    [Range(0f, 1f)] public float volume=1f;
    public AudioClip clip;
    public bool loop=false;
    public PitchSettings pitch;

    // Ensure the nested settings are serialized and default-initialized so Inspector won't show them as missing
    public DistanceSoundSettings distanceSoundSettings = new DistanceSoundSettings();

    // Make SoundTag a normal single-select enum (Inspector will show a single dropdown)
    public enum SoundTag
    {
        None = 0,
        SFX  = 1,
        BGM  = 2,
        UI   = 3,
    }

    // Use normal enum assignment syntax; default to SFX
    public SoundTag soundTag = SoundTag.SFX;

    // Convenience helper for single-select enum: check equality
    public bool HasTag(SoundTag tag)
    {
        return soundTag == tag;
    }

    public float getRandomPitch()
    {
        if (pitch == null) return 1f;
        return Random.Range(pitch.min, pitch.max);
    }

    // Mark as serializable so Unity can show/edit it in the Inspector
    [Serializable]
    public class DistanceSoundSettings
    {
        public bool enableDistanceSound=false;
        public float maxDistance=10f;
    }

    // Defensive initialization for existing assets that were created before this field existed
    private void OnEnable()
    {
        if (distanceSoundSettings == null)
            distanceSoundSettings = new DistanceSoundSettings();
    }
}

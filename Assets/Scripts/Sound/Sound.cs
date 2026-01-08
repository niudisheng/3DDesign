using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New Sound", menuName = "Sound/Sound")]
public class Sound : ScriptableObject
{
    public string AudioSource;
    [Range(0f, 1f)] public float volume=1f;
    public AudioClip clip;
    public PitchSettings pitch;
    public bool loop=false;

    // Make SoundTag a bitmask enum so it can be multi-selected in the Inspector
    [System.Flags]
    public enum SoundTag
    {
        None = 0,
        SFX  = 1,
        BGM  = 2,
        UI   = 3,
    }

    // Use normal enum assignment syntax; default to SFX
    public SoundTag soundTag = SoundTag.SFX;

    // Convenience helper to check whether one or more tags are set.
    public bool HasTag(SoundTag tag)
    {
        return (soundTag & tag) == tag;
    }

    public float getRandomPitch()
    {
        if (pitch == null) return 1f;
        return Random.Range(pitch.min, pitch.max);
    }
}

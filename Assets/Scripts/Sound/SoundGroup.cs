using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New Sound Group", menuName = "Sound/Sound Group")]
public class SoundGroup : ScriptableObject
{
    public string AudioSource;
    [Range(0f, 1f)] public float volume;
    public AudioClip[] clips;
    public bool loop=false;
    public Sound.SoundTag soundTag=Sound.SoundTag.None;
    public PitchSettings pitch;
    public DistanceSoundSettings distanceSoundSettings;
    public float getRandomPitch()
    {
        return Random.Range(pitch.min, pitch.max);
    }
}


[Serializable]
public class PitchSettings
{
    [Range(-3.0f, 3.0f)] public float max=1;
    [Range(-3.0f, 3.0f)] public float min=1;
}
[Serializable]
public class DistanceSoundSettings
{
    public bool enableDistanceSound=false;
    public float maxDistance=10f;

    
}
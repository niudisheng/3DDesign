using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New Sound Group", menuName = "Sound/Sound Group")]
public class SoundGroup : ScriptableObject
{
    public string AudioSource;
    [Range(0f, 1f)] public float volume;
    public AudioClip[] clips;
    public PitchSettings pitch;
    public bool loop=false;
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
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

    public float getRandomPitch()
    {
        return Random.Range(pitch.min, pitch.max);
    }
}

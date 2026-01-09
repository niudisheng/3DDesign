using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    public static PlayerSound Instance;
    public Sound[] AttackSound;
    public SoundGroup WalkSoundGroup;
    public Sound GetSword;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayAttackSound(int index)
    {
        if (index < 0 || index >= AttackSound.Length) return;
        SoundManager.Instance.PlaySound(AttackSound[index]);
    }

    public void PlayWalkSound()
    {
        SoundManager.Instance.PlaySoundGroup(WalkSoundGroup);
    }
    public void PlayGetSwordSound()
    {
        SoundManager.Instance.PlaySound(GetSword);
    }
}
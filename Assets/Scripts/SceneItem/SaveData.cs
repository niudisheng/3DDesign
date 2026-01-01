using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

public class SaveData
{
    
    public Vector3 playerPosition;

    public SaveData(Vector3 newSaveData)
    {
        playerPosition = newSaveData;
    }

    public static SaveData SavePlayerState(GameObject player)
    {
        Vector3 position =  player.transform.position;
        SaveData newSaveData = new SaveData(position);
        return newSaveData;
        
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BayatGames.SaveGameFree.Types;
using BayatGames.SaveGameFree.Serializers;
using BayatGames.SaveGameFree;

public class GameObjectSavePosition : MonoBehaviour
{
    public Transform target;
    public bool loadOnStart = true;
    public string identifier = "exampleSavePosition.dat";

    // Use this for initialization
    void Start()
    {
        SaveGame.Serializer = new SaveGameBinarySerializer();
        if (loadOnStart)
        {
            Load();
        }
    }

    //Save on exit
    void OnApplicationQuit()
    {
        Save();
    }

    public void Save()
    {
        SaveGame.Save<Vector3Save>(identifier, target.position, SaveGamePath.DataPath);
    }

    public void Load()
    {
        target.position = SaveGame.Load<Vector3Save>(
            identifier,
            Vector3.zero,
            SaveGamePath.DataPath);
    }
}

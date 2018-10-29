using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BayatGames.SaveGameFree.Types;
using BayatGames.SaveGameFree.Serializers;
using BayatGames.SaveGameFree;

public class GameObjectSaveRotation : MonoBehaviour {

    public Transform target;
    public bool loadOnStart = true;
    public string identifier = "exampleSavePositionRotation.dat";

    // Use this for initialization
    void Start () {
        SaveGame.Serializer = new SaveGameBinarySerializer();
        if (loadOnStart)
        {
            Load();
        }
    }
	
    public void Save()
    {
        SaveGame.Save<Vector3Save>(identifier, target.position, SaveGamePath.DataPath);
        SaveGame.Save<QuaternionSave>(identifier, target.rotation, SaveGamePath.DataPath);
    }

    public void Load()
    {
        target.position = SaveGame.Load<Vector3Save>(
            identifier,
            Vector3.zero,
            SaveGamePath.DataPath);
        target.rotation = SaveGame.Load<QuaternionSave>(
                identifier,
                Quaternion.identity,
                SaveGamePath.DataPath);
    }
}

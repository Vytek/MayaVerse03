using UnityEngine;

/// <summary>
/// This is used to spawn a prefab on Start(). You might want to use this script to instantiate CLIU (read the documentation PDF for more information, section "Getting Started".
/// </summary>
public class CommandLineSpawnOnStart : MonoBehaviour {

    public GameObject prefab;

	// Use this for initialization
	void Start () {
        Instantiate(prefab);
	}
}

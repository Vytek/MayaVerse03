using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StartGame : MonoBehaviour {

	public GameObject ThisGameObject;

	// Use this for initialization
	void Start () {
		//Count
		Debug.Log ("Mumbers GameObjects: " + GameObjectTracker.AllGameObjects.Count);
        Debug.Log ("Mumbers PlayerGameObjects: " + PlayerGameObjectTracker.AllPlayerGameObjects.Count);
        //Index a GameObject in List
        //Debug.Log ("GameObject Name: " + GameObjectTracker.AllGameObjects [1].gameObject.name);
		//Search with foreach
		foreach (GameObject GO in GameObjectTracker.AllGameObjects)
		{
			Debug.Log ("GameObject Name: " + GO.name);
		}
        //Search with IndexOf?? Perhaps: https://answers.unity.com/questions/442220/searching-a-list-of-gameobjects-by-name.html
        //GameObjectTracker.AllGameObjects.
        GameObject temp = GameObjectTracker.AllGameObjects.Where(obj => obj.GetComponent<NetworkObject>().objectID == 2).SingleOrDefault();
        Debug.Log("GameObject Number 2 Name: "+temp.name);
    }
}

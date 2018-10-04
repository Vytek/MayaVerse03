using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CommandTerminal;

public static class ListCommand
{
	[RegisterCommand(Help = "List all Players and Objects in MayaVerse")]
	static void CommandList(CommandArg[] args)
	{
		//Count
		Terminal.Log ("Mumbers GameObjects: " + GameObjectTracker.AllGameObjects.Count);
		Debug.Log ("Mumbers PlayerGameObjects: " + PlayerGameObjectTracker.AllPlayerGameObjects.Count);
		//Index a GameObject in List
		//Debug.Log ("GameObject Name: " + GameObjectTracker.AllGameObjects [1].gameObject.name);
		//Search with foreach
		foreach (GameObject GO in GameObjectTracker.AllGameObjects)
		{
			Terminal.Log ("GameObject Name: " + GO.name);
		}
		//Search with IndexOf?? Perhaps: https://answers.unity.com/questions/442220/searching-a-list-of-gameobjects-by-name.html
		//GameObjectTracker.AllGameObjects.
		GameObject temp = GameObjectTracker.AllGameObjects.Where(obj => obj.GetComponent<NetworkObject>().objectID == 2).SingleOrDefault();
		Terminal.Log("GameObject Number 2 Name: "+temp.name);
	}
}
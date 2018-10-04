using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CommandTerminal;

public static class MoveXCommand
{
	[RegisterCommand(Help = "Move on X an Object in MayaVerse", MinArgCount = 2, MaxArgCount = 2)]
	static void CommandMoveX(CommandArg[] args)
	{
		float x = args[0].Float;
		string GameObjectName = args[1].String;

		if (Terminal.IssuedError) return; // Error will be handled by Terminal

		GameObject temp = GameObjectTracker.AllGameObjects.Where (obj => obj.name == GameObjectName).SingleOrDefault();
		LeanTween.moveX (temp, x, 1f);
		//DebugLog
		Terminal.Log("Object: "+GameObjectName+" moved!");
	}
}

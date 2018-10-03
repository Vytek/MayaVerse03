using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CommandTerminal;

public static class MoveZCommand
{
    [RegisterCommand(Help = "Move on Z an Object in MayaVerse", MinArgCount = 2, MaxArgCount = 2)]
    static void CommandMoveZ(CommandArg[] args)
    {
        float z = args[0].Float;
        string GameObjectName = args[1].String;

        if (Terminal.IssuedError) return; // Error will be handled by Terminal

        GameObject temp = GameObjectTracker.AllGameObjects.Where(obj => obj.name == GameObjectName).SingleOrDefault();
        LeanTween.moveZ(temp, z, 1f);
        //DebugLog
        Terminal.Log("Object: " + GameObjectName + " moved!");
    }
}

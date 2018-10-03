using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CommandTerminal;

public static class MoveYCommand
{
    [RegisterCommand(Help = "Move on Y an Object in MayaVerse", MinArgCount = 2, MaxArgCount = 2)]
    static void CommandMoveY(CommandArg[] args)
    {
        float y = args[0].Float;
        string GameObjectName = args[1].String;

        if (Terminal.IssuedError) return; // Error will be handled by Terminal

        GameObject temp = GameObjectTracker.AllGameObjects.Where(obj => obj.name == GameObjectName).SingleOrDefault();
        LeanTween.moveY(temp, y, 1f);
        //DebugLog
        Terminal.Log("Object: " + GameObjectName + " moved!");
    }
}
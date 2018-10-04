using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommandTerminal;

public static class DebugTestCommand
{
    [RegisterCommand(Help = "Outputs message")]
    static void CommandHello(CommandArg[] args)
    {
        Terminal.Log("Hello world!");
    }
}
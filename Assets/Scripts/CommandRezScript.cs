using UnityEngine;
using CommandTerminal;

public static class RezCommand
{
    [RegisterCommand(Help = "Rez Object in MayaVerse")]
    static void CommandRez(CommandArg[] args)
    {
        Terminal.Log("Hello world!");
    }
}

using UnityEngine;
using CommandTerminal;

public static class RezCommand
{
    [RegisterCommand(Help = "Rez Object in MayaVerse", MinArgCount = 4, MaxArgCount = 4)]
    static void CommandRez(CommandArg[] args)
    {
        float x = args[0].Float;
        float y = args[1].Float;
        float z = args[2].Float;
        string vIPFSHashFromCommand = args[2].String;

        if (Terminal.IssuedError) return; // Error will be handled by Terminal

        Vector3 NewPosition = new Vector3(x, y, z);
        //Call actual instance
        NetworkManager.instance.RezObject(NewPosition, Quaternion.identity, 0, vIPFSHashFromCommand);
        //
        Terminal.Log("Rez Object: "+vIPFSHashFromCommand);
    }
}

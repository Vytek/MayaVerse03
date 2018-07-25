using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(CommandLineModuleSettings))]
public abstract class CommandLineModule : MonoBehaviour {

    public Dictionary<string, System.Action<string[]>> commands = new Dictionary<string, System.Action<string[]>>();

    public void Execute(string[] args)
    {
        try
        {
            string command = args[1].ToLower();
            if (args[0].ToLower() == "h" || args[0].ToLower() == "-h" || args[0].ToLower() == "help" ||
                command == "h" || command == "-h" || command == "help")
            {
                Help();
            }
            else if (commands.ContainsKey(command))
            {
                commands[command](args);
            }
        }
        catch (System.Exception e)
        {
            CommandLineCore.PrintError(e.ToString());
        }        
    }

    public virtual void Help()
    {
        StringBuilder builder = new StringBuilder();
        int counter = 0;
        foreach (var item in commands)
        {
            builder.Append(item.Key);
            counter++;
            if (counter < commands.Count)
            {
                builder.Append("\n");
            }
        }
        CommandLineCore.Print(builder.ToString());
    }
}

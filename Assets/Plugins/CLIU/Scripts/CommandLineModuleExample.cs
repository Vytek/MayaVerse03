using UnityEngine;
using System.Text;

/// <summary>
/// If for some reason the Module Builder fails to build the script file, copy the contents of this file into a new script file.
/// </summary>
public class CommandLineModuleExample : CommandLineModule {
    void Start()
    {
        commands.Add("example", SomeMethod); //The example command will call the SomeMethod method.
        commands.Add("printargs", PrintArgs); //The printargs command will call the PrintArgs method.
    }

    public override void Help()
    {
        StringBuilder helpMessage = new StringBuilder();

        helpMessage.AppendLine("example");
        helpMessage.Append("printargs string:messageToPrint");

        CommandLineCore.Print(helpMessage.ToString());
    }

    private void SomeMethod(string[] args)
    {
        Debug.Log("SomeMethod() was called");
        CommandLineCore.Print("This is an example message.");
    }

    private void PrintArgs(string[] args)
    {
        foreach (var item in args)
        {
            CommandLineCore.Print(item);
        }
    }
}


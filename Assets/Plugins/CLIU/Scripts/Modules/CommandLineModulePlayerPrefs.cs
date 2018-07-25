using System.Text;
using UnityEngine;

public class CommandLineModulePlayerPrefs : CommandLineModule {

    private void Start()
    {
        commands.Add("getint", GetInt);
        commands.Add("getfloat", GetFloat);
        commands.Add("getstring", GetString);
        commands.Add("setint", SetInt);
        commands.Add("setfloat", SetFloat);
        commands.Add("setstring", SetString);
        commands.Add("saveprefs", SaveAll);
        commands.Add("deleteallprefs", DeleteAll);
    }

    public override void Help()
    {
        StringBuilder helpMessage = new StringBuilder();

        helpMessage.Append("getint string:key\n");
        helpMessage.Append("getfloat string:key\n");
        helpMessage.Append("getstring string:key\n");
        helpMessage.Append("setint string:keyWithPipes int:value\n");
        helpMessage.Append("setfloat string:keyWithPipes float:value\n");
        helpMessage.Append("setstring string:keyWithPipes string:value\n");
        helpMessage.Append("saveprefs\n");
        helpMessage.Append("deleteallprefs");

        CommandLineCore.Print(helpMessage.ToString());
    }

    private void SetInt(string[] args)
    {
        PlayerPrefs.SetInt(CommandLineCore.StringWithPipes(args[2]), int.Parse(args[3]));
    }

    private void SetFloat(string[] args)
    {
        PlayerPrefs.SetFloat(CommandLineCore.StringWithPipes(args[2]), float.Parse(args[3]));
    }

    private void SetString(string[] args)
    {
        PlayerPrefs.SetString(CommandLineCore.StringWithPipes(args[2]), CommandLineCore.StringWithPipes(args[3]));
    }

    private void GetInt(string[] args)
    {
        CommandLineCore.Print(PlayerPrefs.GetInt(CommandLineCore.StringWithPipes(args[2])).ToString());
    }

    private void GetFloat(string[] args)
    {
        CommandLineCore.Print(PlayerPrefs.GetFloat(CommandLineCore.StringWithPipes(args[2])).ToString());
    }

    private void GetString(string[] args)
    {
        CommandLineCore.Print(PlayerPrefs.GetString(CommandLineCore.StringWithPipes(args[2])));
    }

    private void SaveAll(string[] args)
    {
        PlayerPrefs.Save();
        CommandLineCore.PrintSuccess("PlayerPrefs successfully saved!");
    }

    private void DeleteAll(string[] args)
    {
        PlayerPrefs.DeleteAll();
        CommandLineCore.PrintSuccess("All PlayerPrefs were successfully deleted!");
    }
}


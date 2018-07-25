using System.Text;
using UnityEngine;

public class CommandLineModuleObjects : CommandLineModule {

    private void Start()
    {
        commands.Add("call", Call);
        commands.Add("callall", CallAll);
        commands.Add("callbytag", CallTag);
        commands.Add("destroy", DestroyObj);
        commands.Add("destroytag", DestroyTag);
        commands.Add("instantiate", InstantiateObj);
    }

    public override void Help()
    {
        StringBuilder helpMessage = new StringBuilder();

        helpMessage.Append("call string:methodName string:gameObjectName\n");
        helpMessage.Append("callbytag string:methodName string:gameObjectTag\n");
        helpMessage.Append("callall string:methodName\n");
        helpMessage.Append("destroy string:gameObjectName\n");
        helpMessage.Append("destroytag string:gameObjectsTag\n");
        helpMessage.Append("instantiate string:pathInAResourcesFolder");

        CommandLineCore.Print(helpMessage.ToString());
    }

    public void CallAll(string[] args)
    {
        GameObject[] allObjectsOnScene = FindObjectsOfType<GameObject>();
        for (int i = 0; i < allObjectsOnScene.Length; i++)
        {
            try
            {
                allObjectsOnScene[i].SendMessage(args[2]);
            }
            catch (System.Exception)
            {
            }
        }
    }

    public void Call(string[] args)
    {
        GameObject obj = GameObject.Find(CommandLineCore.StringWithSpaces(args, 3));
        obj.SendMessage(args[2]);
    }

    public void CallTag(string[] args)
    {
        GameObject objTag = GameObject.FindGameObjectWithTag(CommandLineCore.StringWithSpaces(args, 3));
        objTag.SendMessage(args[2]);
    }

    public void DestroyObj(string[] args)
    {
        Destroy(GameObject.Find(CommandLineCore.StringWithSpaces(args, 2)));
    }

    public void DestroyTag(string[] args)
    {
        GameObject[] objsTag = (GameObject.FindGameObjectsWithTag(CommandLineCore.StringWithSpaces(args, 2)));
        foreach (var item in objsTag)
        {
            Destroy(item);
        }
    }

    public void InstantiateObj(string[] args)
    {
        Instantiate(Resources.Load(CommandLineCore.StringWithSpaces(args, 2)));
    }
}

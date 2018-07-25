using UnityEngine;
using UnityEditor;
using System.IO;

public class CommandLineModuleBuilder : EditorWindow {

    string moduleName = "ModuleName"; //Prefab Name
    string moduleCode = "example";
    string moduleScriptName = "CommandLineModuleExampleName";
    Texture2D logo;

    [MenuItem("Tools/CLIU/Module Builder")]
    public static void OpenModuleBuilder()
    {
        CommandLineModuleBuilder window = GetWindow<CommandLineModuleBuilder>("Module Builder");
        window.minSize = new Vector2(415, 440);
    }

    private void OnEnable()
    {
        logo = (Texture2D)Resources.Load("Textures/CLIU-Logo", typeof(Texture2D));
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(logo);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Label("Prefab Builder", EditorStyles.largeLabel);
        GUILayout.Space(10);
        moduleName = EditorGUILayout.TextField("Module Name:", moduleName);
        moduleCode = EditorGUILayout.TextField("Internal Code:", moduleCode);
        GUILayout.Space(10);
        if (GUILayout.Button("Build Prefab"))
        {
            BuildModule();
        }
        GUILayout.Space(20);
        GUILayout.Label("Module's Script Builder", EditorStyles.largeLabel);
        GUILayout.Space(10);
        moduleScriptName = EditorGUILayout.TextField("Script Name:", moduleScriptName);
        if (GUILayout.Button("Build Script"))
        {
            BuildModuleClass();
        }
        GUILayout.Space(20);
        EditorGUILayout.HelpBox("Remember to attach the script created to the prefab of your module!", MessageType.Info);
        GUILayout.Space(10);
        GUILayout.Label("Made by Lucas Sampaio Dias\n(lucassampaiodias@gmail.com)", EditorStyles.centeredGreyMiniLabel);
    }

    void BuildModule()
    {
        GameObject module = new GameObject(moduleName);
        CommandLineModuleSettings settings = module.AddComponent<CommandLineModuleSettings>();
        settings.moduleInternalCode = moduleCode;
        PrefabUtility.CreatePrefab("Assets/Plugins/CLIU/Resources/CLIU Modules/" + moduleName + ".prefab", module);
        DestroyImmediate(module);
    }

    string BuildModuleClass()
    {
        string name = moduleScriptName;
        name = name.Replace(" ", "_");
        name = name.Replace("-", "_");
        string copyPath = "/CommandLine/Scripts/Modules";

        if(File.Exists(copyPath) == false){
            using (StreamWriter outfile = new StreamWriter("Assets/Plugins/CLIU/Scripts/Modules/" + name + ".cs"))
            {
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using System.Text;");
                outfile.WriteLine("");
                outfile.WriteLine("public class " + name + " : CommandLineModule {");
                outfile.WriteLine("    void Start()");
                outfile.WriteLine("    {");
                outfile.WriteLine("        commands.Add(\"example\", SomeMethod); //The example command will call the SomeMethod method.");
                outfile.WriteLine("        commands.Add(\"printargs\", PrintArgs); //The printargs command will call the PrintArgs method.");
                outfile.WriteLine("    }");
                outfile.WriteLine("");
                outfile.WriteLine("    public override void Help()");
                outfile.WriteLine("    {");
                outfile.WriteLine("        StringBuilder helpMessage = new StringBuilder();");
                outfile.WriteLine("");
                outfile.WriteLine("        helpMessage.AppendLine(\"example\");");
                outfile.WriteLine("        helpMessage.Append(\"printargs string:messageToPrint\");");
                outfile.WriteLine("");
                outfile.WriteLine("        CommandLineCore.Print(helpMessage.ToString());");
                outfile.WriteLine("    }");
                outfile.WriteLine("");
                outfile.WriteLine("    private void SomeMethod(string[] args)");
                outfile.WriteLine("    {");
                outfile.WriteLine("        Debug.Log(\"SomeMethod() was called\");");
                outfile.WriteLine("        CommandLineCore.Print(\"This is an example message.\");");
                outfile.WriteLine("    }");
                outfile.WriteLine("");
                outfile.WriteLine("    private void PrintArgs(string[] args)");
                outfile.WriteLine("    {");
                outfile.WriteLine("        foreach (var item in args)");
                outfile.WriteLine("        {");
                outfile.WriteLine("            CommandLineCore.Print(item);");
                outfile.WriteLine("        }");
                outfile.WriteLine("    }");
                outfile.WriteLine("}");
                outfile.WriteLine("");
            }
        }
        AssetDatabase.Refresh();
        Debug.Log(name + ".cs created at \"" + copyPath + "\". Be sure to attach it to the prefab of your module!");
        return name;
    }
}

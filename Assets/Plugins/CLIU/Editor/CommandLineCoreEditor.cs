using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CommandLineCore))]
public class CommandLineCoreEditor : Editor {

    private Texture2D logo;

    public override void OnInspectorGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        logo = (Texture2D)Resources.Load("Textures/CLIU-Logo", typeof(Texture2D));
        GUILayout.Label(logo);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        DrawDefaultInspector();
        GUILayout.Space(20);
        if (GUILayout.Button("Reset Settings") && EditorUtility.DisplayDialog("Reset Settings?", 
            "Are you sure you want to reset ALL CLIU settings?\n\nSettings will be set to the default values if you press \"Yes\".", "Yes", "No"))
        {
            try
            {
                FindObjectOfType<CommandLineCore>().ResetSettings();
                Debug.Log("CLIU settings were reset");
            }
            catch (System.Exception)
            {
                throw;
            }
            
        }
        GUILayout.Space(10);
        GUILayout.Label("Made by Lucas Sampaio Dias\n(lucassampaiodias@gmail.com)", EditorStyles.centeredGreyMiniLabel);
    }
}

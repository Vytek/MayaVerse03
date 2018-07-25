using UnityEngine;
using UnityEditor;

public class CommandLineEditorButtons : Editor {

    static GameObject cliuWindow;
    static GameObject cliuFullscreen;

    [MenuItem("Tools/CLIU/Instantiate or Destroy CLIU")]
    static void OpenCLIU()
    {
        cliuWindow = GameObject.Find("CLIU");

        if (cliuWindow == null)
        {
            cliuWindow = PrefabUtility.InstantiatePrefab(Resources.Load("CLIU")) as GameObject;
        }
        else
        {
            try
            {
                if (EditorApplication.isPlaying)
                {
                    Destroy(cliuWindow);
                }
                else
                {
                    DestroyImmediate(cliuWindow);
                }

                cliuWindow = null;
            }
            catch (System.Exception) { }
        }
    }

    [MenuItem("Tools/CLIU/Instantiate or Destroy CLIU (Fullscreen)")]
    static void OpenCLIUFullscreen()
    {
        cliuFullscreen = GameObject.Find("CLIU (Fullscreen)");

        if (cliuFullscreen == null)
        {
            cliuFullscreen = PrefabUtility.InstantiatePrefab(Resources.Load("CLIU (Fullscreen)")) as GameObject;
        }
        else
        {
            try
            {
                if (EditorApplication.isPlaying)
                {
                    Destroy(cliuFullscreen);
                }
                else
                {
                    DestroyImmediate(cliuFullscreen);
                }

                cliuFullscreen = null;
            }
            catch (System.Exception) { }
        }
    }
}

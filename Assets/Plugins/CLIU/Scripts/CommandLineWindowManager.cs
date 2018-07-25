using UnityEngine;

/// <summary>
/// This script handles what screens should appear or disappear based on certain events
/// </summary>
public class CommandLineWindowManager : MonoBehaviour {

    public GameObject window;
    public CommandLineCore commandLineCore;
    public GameObject buttonOpenWindow;
	
	void Update () {
        if (commandLineCore.openWindowHotkey != "" && commandLineCore.openWindowHotkey != null)
        {
            if (!window.activeSelf && Input.GetKeyDown(commandLineCore.openWindowHotkey))
            {
                OpenCLIUWindow();
            }
        }        

        if (commandLineCore.closeWindowHotkey != "" && commandLineCore.closeWindowHotkey != null)
        {
            if (window.activeSelf && Input.GetKeyDown(commandLineCore.closeWindowHotkey))
            {
                CloseCLIUWindow();
            }
        }

        if (commandLineCore.resetWindowHotkey != "" && commandLineCore.resetWindowHotkey != null)
        {
            if (Input.GetKeyDown(commandLineCore.resetWindowHotkey))
            {
                commandLineCore.RunCommand(new string[1] { "reset" });
            }
        }
    }

    public void OpenCLIUWindow()
    {
        window.SetActive(true);
        buttonOpenWindow.SetActive(false);
    }

    public void CloseCLIUWindow()
    {
        window.SetActive(false);
        if (!commandLineCore.hideOpenWindowButton)
        {
            buttonOpenWindow.SetActive(true);
        }
    }
}

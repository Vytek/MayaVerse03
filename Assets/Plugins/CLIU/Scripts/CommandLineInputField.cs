using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This script manages the Input Field of the CLIU window. Using the public methods of this script is NOT recommended.
/// </summary>
public class CommandLineInputField : MonoBehaviour {

    public InputField inputField;
    public CommandLineCore cliCore;
    public EventSystem myEventSystem;
    public ScrollRect scroll;
    public Text output;
    private List<string> previousCommands;
    int commandPos = -1;
    bool caretFound = false;
    GameObject caret;

    private void Awake()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            myEventSystem.gameObject.SetActive(true);
        }

        inputField.onEndEdit.AddListener(val =>
        {
            if (Application.isMobilePlatform)
            {
                SendCommandToCore();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SendCommandToCore();
                inputField.ActivateInputField();
            }                
        });

        previousCommands = new List<string>();
        cliCore.initPos = transform.parent.position;
    }

    private void OnEnable()
    {
        if (!Application.isMobilePlatform)
        {
            inputField.ActivateInputField();
        }        
    }

    void Update () {
        if (!caretFound)
        {
            caret = GameObject.Find("CLIU-InputField Input Caret");
            if (caret != null)
            {
                caret.transform.SetSiblingIndex(1);
                caretFound = true;
            }            
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            commandPos--;
            if (commandPos < 0)
            {
                commandPos = 0;
            }

            try
            {
                if (previousCommands.Count > 0)
                {
                    inputField.text = previousCommands[Mathf.Clamp(commandPos, 0, previousCommands.Count - 1)];
                }                
            }
            catch (System.Exception)
            {
                throw;
            }
            inputField.MoveTextEnd(false);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            commandPos++;

            if (commandPos > previousCommands.Count - 1)
            {
                commandPos = previousCommands.Count;
                inputField.text = "";
            }
            else
            {
                try
                {
                    inputField.text = previousCommands[Mathf.Clamp(commandPos, 0, previousCommands.Count - 1)];
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
            inputField.MoveTextEnd(false);
        }
    }

    public void SendCommandToCore()
    {
        if (inputField.text != "")
        {
            SaveCommand(inputField.text);
            PrintInputOnView(inputField.text);

            string[] args = inputField.text.Split(' ');

            cliCore.RunCommand(args);
            inputField.text = "";
        }
    }

    private void SaveCommand(string fullCommand)
    {
        previousCommands.Add(fullCommand);
        commandPos = previousCommands.Count;
    }

    private void PrintInputOnView(string message)
    {
        output.text = output.text + "\n> " + message;
    }

    public void PrintOutputOnView(string message)
    {
        output.text = output.text + "\n" + message + "\n";
    }

    public void PrintColoredOutputOnView(string message, string colorTag) //RichText tag, like <color=#008000ff>message</color>
    {
        output.text = output.text + "\n<color=" + colorTag + ">" + message + "</color>\n";
    }

    public void Clear()
    {
        output.text = "";
    }
}

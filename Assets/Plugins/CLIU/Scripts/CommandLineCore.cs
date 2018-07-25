using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This is the core (or brain) of CLIU. This is also the class which you should use for public methods like Print().
/// </summary>
[ExecuteInEditMode]
public class CommandLineCore : MonoBehaviour {

#region Settings
    [Header("Basic Settings")]
    [Tooltip("Leave empty if you don't want a hotkey.")]
    public string openWindowHotkey = "]";
    [Tooltip("Leave empty if you don't want a hotkey.")]
    public string closeWindowHotkey = "escape";
    [Tooltip("The transparency of the CLIU window.")]
    [Range(0, 1)]
    public float defaultOpacity = 1;
    [Tooltip("If enabled, the CLIU Window will automatically close when the game starts.")]
    public bool startHidden = true;
    [Tooltip("If enabled, the CLIU Window will NOT appear while the game is not running.")]
    public bool hideIfNotRunning = false;
    [Tooltip("If enabled, the 'Open CLIU' button will never appear on the screen.")]
    public bool hideOpenWindowButton = false;
    [Tooltip("If enabled, you'll be able to drag the CLIU Window around by holding the white bar.")]
    public bool draggableWindow = true;
    [Header("Advanced Settings")]
    [Tooltip("Reset the CLIU window to its initial position. Leave empty if you don't want a hotkey.")]
    public string resetWindowHotkey = "";
    [Tooltip("If disabled, loading other scenes will NOT destroy the CLIU gameObject.")]
    public bool destroyOnSceneLoad = false;    
    [Tooltip("If enabled, the GUI will NEVER appear on your game at all. This setting overrides the 'Open Window Hotkey', 'Start Hidden' and 'Hide Open Window Button' settings.\n\nTo keep using CLIU with the window hidden use RunCommand() via script.")]
    public bool noGUI = false;
    [Tooltip("If enabled, the GUI will NEVER appear on your game builds, but will still appear while you are using Unity. This setting overrides the 'Open Window Hotkey', 'Start Hidden' and 'Hide Open Window Button' settings.\n\nTo keep using CLIU with the window hidden use RunCommand() via script.")]
    public bool noGUIOutsideUnity = false;
    [Tooltip("If disabled, the placeholder text for the input will NOT change while a module is on focus.")]
    public bool showFocusedModule = true;
    [Tooltip("When CLIU initializes, the module speficied will be automatically focused. Leave empty if you don't want the automatic focus to happen.")]
    public string focusByDefault = "";
    [Tooltip("If enabled, CLIU will NEVER be able to remove the focus from a module.")]
    public bool preventUnfocus = false;
#endregion

    [HideInInspector]
    public Vector3 initPos;

    private GameObject modulesParent;
    private GameObject window;
    private CommandLineInputField inputField;
    private CommandLineModule[] commandLineModules;
    private List<CommandLineModuleSettings> moduleSettings;
    private List<string> moduleNames = new List<string>();
    private GameObject buttonOpenWindow;
    private CommandLineWindowManager windowManager;
    private Text placeholderText;
    private string initialPlaceholderText;
    private string focusedModule = "";
    private CanvasGroup canvasGroup;

    private void Start()
    {
        if (Application.isPlaying)
        {
            InitiateCore();
            InitiateModules();
            SettingBasedProcedures();
        }        
    }

    /// <summary>
    /// This is the Core main method. Core interpret args and execute the appropriate operations.
    /// </summary>
    /// <param name="args">Get the command on args[0] and its (optional) parameters on args[1], args[2], and so on. Example: { "call", "PlayerGameObject", "RespawnMethod" } OR { "help", "time" }</param>
    public void RunCommand(string[] args)
    {
        try
        {
            //If there is no focused module (all commands delivered directly to the module)
            if (focusedModule == "")
            {
                string firstArg = args[0].ToLower();

                //Search for a match on the Core commands. Don't name your custom commands with these names (like "hide" or "exit")
                if (firstArg == "help" || firstArg == "h" || firstArg == "-h")
                {
                    ShowHelp(args);
                }
                else if (firstArg == "modules" || firstArg == "m")
                {
                    ShowModulesLoaded();
                }
                else if (firstArg == "hide" || firstArg == "close")
                {
                    windowManager.CloseCLIUWindow();
                }
                else if (firstArg == "exit")
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                    Application.Quit();
                }
                else if (firstArg == "clear")
                {
                    inputField.Clear();
                }
                else if (firstArg == "reset")
                {
                    ResetWindow();
                }
                else if (firstArg == "focus")
                {
                    if (args.Length > 1)
                    {
                        SetFocus(args[1].ToLower());
                    }
                }
                else if (firstArg == "alpha")
                {
                    window.GetComponent<CanvasGroup>().alpha = float.Parse(args[1]);
                }

                //If it isn't a Core command, Core sends a command to the Execute() of a specific module. Example: "time help"
                else if (args.Length > 1 && moduleNames.Contains(firstArg))
                {
                    SendCommandTo(firstArg, "Execute", args);
                }

                //If everything above fails, Core sends a command to the Execute() of ALL modules. Example: "timescale 0"
                else
                {
                    SendCommandToAllModules("Execute", args);
                }
            }
            //If there is a focused module
            else
            {
                if (args[0].ToLower() == "focus")
                {
                    if (args.Length > 1)
                    {
                        SetFocus(args[1].ToLower());
                    }
                    else if (!preventUnfocus)
                    {
                        RemoveFocus();
                    }
                }
                else
                {
                    string[] newArgs = new string[args.Length + 1];
                    newArgs[0] = "";
                    for (int i = 1; i < newArgs.Length; i++)
                    {
                        newArgs[i] = args[i - 1];
                    }
                    SendCommandTo(focusedModule, "Execute", newArgs);
                }
            }
        }
        catch (System.Exception e)
        {
            PrintError("Error on CLIU Core: " + e.ToString());
        }
    }

    /// <summary>
    /// Call Help() from a specific module or, if the module is not found or specified, the help message of Core.
    /// </summary>
    /// <param name="args"></param>
    public void ShowHelp(string[] args)
    {
        if (args.Length > 1)
        {
            SendCommandTo(args[1].ToLower(), "help", args);
        }
        else
        {
            Print("help: show this message\nmodules: List the code of all the modules running\nhelp moduleCode: show the help message of the module specified\nhide: Hide the CLIU window\nexit: Exit the entire application/game\nclear: Clear all the text on the output\nreset: Returns the CLIU window to its initial position\nfocus moduleCode: specify a module so only its commands can be executed (even Core commands won’t run). Enter focus again to return to the normal CLIU behaviour.\nalpha value: sets the transparency of the window");
            Print("Be sure to check the documentation PDF if you need more help.");
        }
    }    

    /// <summary>
    /// Print a message on the CLIU Window.
    /// </summary>
    /// <param name="message"></param>
    public static void Print(string message)
    {
        GameObject.Find("CLIU-InputField").GetComponent<CommandLineInputField>().PrintOutputOnView(message);
    }

    /// <summary>
    /// Print a message on the CLIU Window using a specified color.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="colorTag">The color tag for the Rich Text tag. Example: #00800ff</param>
    public static void Print(string message, string colorTag)
    {
        GameObject.Find("CLIU-InputField").GetComponent<CommandLineInputField>().PrintColoredOutputOnView(message, colorTag);
    }

    /// <summary>
    /// Print a message on the CLIU Window using a red color.
    /// </summary>
    /// <param name="message"></param>
    public static void PrintError(string message)
    {
        Print(message, "#ff0000ff");
    }

    /// <summary>
    /// Print a message on the CLIU Window using a yellow color.
    /// </summary>
    /// <param name="message"></param>
    public static void PrintWarning(string message)
    {
        Print(message, "#ffff00ff");
    }

    /// <summary>
    /// Print a message on the CLIU Window using a green color.
    /// </summary>
    /// <param name="message"></param>
    public static void PrintSuccess(string message)
    {
        Print(message, "#008000ff");
    }

    /// <summary>
    /// Get the strings of an array and build them into a single string separating each element with a space
    /// </summary>
    /// <param name="args"></param>
    /// <param name="initialIndex"></param>
    /// <returns></returns>
    public static string StringWithSpaces(string[] args, int initialIndex)
    {
        StringBuilder entireString = new StringBuilder();

        for (int i = initialIndex; i < args.Length; i++)
        {
            entireString.Append(args[i]);
            if (i + 1 < args.Length)
            {
                entireString.Append(" ");
            }
        }

        return entireString.ToString();
    }

    /// <summary>
    /// Replace the pipe symbols of a string with spaces.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static string StringWithPipes(string message)
    {
        string newKey = message;
        newKey = newKey.Replace('|', ' ');
        return newKey;
    }

    /// <summary>
    /// Reset all settings to the their default values
    /// </summary>
    public void ResetSettings()
    {
        openWindowHotkey = "]";
        closeWindowHotkey = "escape";
        startHidden = true;
        defaultOpacity = 1;
        hideIfNotRunning = false;
        hideOpenWindowButton = false;
        draggableWindow = true;
        resetWindowHotkey = "";
        destroyOnSceneLoad = false;
        noGUI = false;
        noGUIOutsideUnity = false;
        showFocusedModule = true;
        focusByDefault = "";
        preventUnfocus = false;
    }

    private void ShowModulesLoaded()
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < moduleNames.Count; i++)
        {
            builder.Append(moduleNames[i]);

            if (i < moduleNames.Count - 1)
            {
                builder.Append(", ");
            }
            else
            {
                builder.Append(".");
            }
        }

        Print("Modules loaded: " + builder.ToString());
    }

    private void SendCommandToAllModules(string command, string[] args)
    {
        string[] modifiedArgs = new string[args.Length + 1];

        modifiedArgs[0] = "";
        for (int i = 1; i < modifiedArgs.Length; i++)
        {
            modifiedArgs[i] = args[i - 1];
        }

        for (int i = 0; i < commandLineModules.Length; i++)
        {
            commandLineModules[i].Execute(modifiedArgs);
        }
    }

    private void SendCommandTo(string moduleName, string command, string[] args)
    {
        for (int i = 0; i < commandLineModules.Length; i++)
        {
            if (moduleNames[i] == moduleName)
            {
                commandLineModules[i].Execute(args);
                i = commandLineModules.Length;
            }
        }
    }

    private void InitiateCore()
    {
        try
        {
            buttonOpenWindow = GameObject.Find("CLIU-OpenCLIUButton");
            window = GameObject.Find("CLIU-Window").gameObject;
            windowManager = FindObjectOfType<CommandLineWindowManager>();
            inputField = FindObjectOfType<CommandLineInputField>();
            placeholderText = GameObject.Find("CLIU-InputPlaceholderText").GetComponent<Text>();
            initialPlaceholderText = placeholderText.text;
            canvasGroup = GetComponent<CanvasGroup>();
        }
        catch (System.Exception e)
        {
            Debug.LogError("CLIU could not Initiate!\n" + e.ToString());
        }        
    }

    private void InitiateModules()
    {
        try
        {
            Object[] modules = Resources.LoadAll("CLIU Modules");
            commandLineModules = new CommandLineModule[modules.Length];
            moduleSettings = new List<CommandLineModuleSettings>();
            modulesParent = transform.Find("CLIU-Modules").gameObject;

            for (int i = 0; i < modules.Length; i++)
            {
                GameObject moduleObj = (GameObject)Instantiate(modules[i], modulesParent.transform);
                commandLineModules[i] = moduleObj.GetComponent<CommandLineModule>();
            }

            for (int i = 0; i < commandLineModules.Length; i++)
            {
                moduleSettings.Add(commandLineModules[i].GetComponent<CommandLineModuleSettings>());
                moduleNames.Add(moduleSettings[i].moduleInternalCode.ToLower());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("CLIU could not Initiate its modules properly!\n" + e.ToString());
        }
    }

    private void SettingBasedProcedures()
    {
        focusedModule = focusByDefault;
        if (focusedModule != "")
        {
            SetFocus(focusedModule);
        }

        if (noGUI || (!Application.isEditor && noGUIOutsideUnity))
        {
            GetComponent<Canvas>().enabled = false;
            openWindowHotkey = "";
            startHidden = true;
            hideOpenWindowButton = true;
        }

        if (destroyOnSceneLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        canvasGroup.alpha = defaultOpacity;

        if (startHidden)
        {
            window.SetActive(false);
            if (hideOpenWindowButton)
            {
                buttonOpenWindow.SetActive(false);
            }
        }
        else
        {
            buttonOpenWindow.SetActive(false);
        }
    }

    private void ResetWindow()
    {
        window.transform.position = initPos;
    }

    private void SetFocus(string module)
    {
        if (moduleNames.Contains(module))
        {
            focusedModule = module.ToLower();
            if (showFocusedModule)
            {
                placeholderText.text = "Focused Module: " + module.ToLower();
            }
        }
        else
        {
            PrintError("Could not focus because the module was not found!");
        }
    }

    private void RemoveFocus()
    {
        focusedModule = "";
        placeholderText.text = initialPlaceholderText;
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        UnityEditor.EditorApplication.update += HideIfNotRunning;
    }

    private void OnDisable()
    {
        UnityEditor.EditorApplication.update -= HideIfNotRunning;
    }

    private void HideIfNotRunning()
    {
        if (!Application.isPlaying)
        {
            if (hideIfNotRunning)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                canvasGroup.alpha = 0;
            }
            else
            {
                canvasGroup = GetComponent<CanvasGroup>();
                canvasGroup.alpha = defaultOpacity;
            }
        }
    }
#endif
}

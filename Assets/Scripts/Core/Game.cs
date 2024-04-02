#if UNITY_EDITOR
#pragma warning disable CS0162 // Unreachable code detected
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public enum CommandFlag
{
    DEBUG,
    DEVELOPER,
    GAME,
    CHEAT,
    NO_HELP
}

public class ConsoleCommandBase
{
    private readonly string _id;
    private readonly string _description;
    private readonly string _format;
    private readonly CommandFlag[] _flags;
    public static Dictionary<string, ConsoleCommandBase> Commands;

    public ConsoleCommandBase(string id, string description, string format, params CommandFlag[] flags)
    {
        _id = id;
        _description = description;
        _format = format;
        if (flags.Length != 0) _flags = flags;
        Commands ??= new Dictionary<string, ConsoleCommandBase>();
        Commands[id] = this;
    }

    public string Id => _id;
    public string Description => _description;
    public string Format => _format;
    public CommandFlag[] Flags => _flags;
}

public class ConsoleCommand : ConsoleCommandBase
{
    private readonly Action _action;

    public ConsoleCommand(string id, string description, string format, Action action, params CommandFlag[] flags) : base(id, description, format, flags)
    {
        _action = action;
    }

    public void Invoke()
    {
        _action.Invoke();
    }
}

public class ConsoleCommand<T> : ConsoleCommandBase
{
    private readonly Action<T> _action;

    public ConsoleCommand(string id, string description, string format, Action<T> action, params CommandFlag[] flags) : base(id, description, format, flags)
    {
        _action = action;
    }

    public void Invoke(T value)
    {
        _action.Invoke(value);
    }
}

public enum AppEvent
{
    Quit,
    Restart,
    Crash
}

public class Game : MonoBehaviour
{
    public static Game local;
    [Header("FOR TESTING")]
    [Tooltip("Developer Mode is special bool, which unlocks various tools and cheats\nFor example, it will force enable cheats, and it allows you to unlock any achievements even if you have cheats enabled (Editor Only)")]
    public bool devMode;

    [Header("Basic")]
    [Tooltip("When enabled, the player's controls will be disabled\nDON'T MODIFY HERE, instead use MakeUncontrollable() function")]
    public bool dontControl;
    [Tooltip("When enabled, it makes the ability to open the dev console")]
    public bool allowConsole;
    [Tooltip("Cheat Mode - When enabled, it allows to execute commands that's flagged with \"CHEAT\"")]
    public bool cheats;
    [Tooltip("When enabled, it allows to exclude running speed limit")]
    public bool enableBunnyHop = true;
    [Tooltip("When enabled, it allows to jump even when the jump button is still holding")]
    public bool autoBunnyHop;
    [Tooltip("Used for pausing normally")]
    public GameObject pauseMenu;
    [Tooltip("Used for button")]
    public GameObject btnTooltip;

    [Header("Console")]
    [Tooltip("The console can be used for developers/modders")]
    public GameObject console;
    [Tooltip("The console input can be used for executing commands")]
    public TMP_InputField consoleInput;
    [Tooltip("The console log can be used for viewing what happened recently or earlier")]
    public TextMeshProUGUI consoleLog;
    [Tooltip("Used for when Log() occured, will scroll to bottom")]
    public ScrollRect consoleLogScroll;

    [Header("INFO - DO NOT EDIT")]
    [Tooltip("In-game timescale (not bulit-in), which occured only when either Pause() or SetGameSpeed() has triggered")]
    public float timeScale = 1f;
    [Tooltip("Wherever or not the game has paused")]
    public bool paused;
    [Tooltip("Used to prevent pause menu opening even in main menu")]
    public bool playing;

    public static void ConsoleCommand(string input)
    {
        string[] args = input.Split(" ");
        string mainInput = args[0];

        // check against available commands
        ConsoleCommandBase command;
        if (ConsoleCommandBase.Commands.TryGetValue(mainInput.ToLower(), out command))
        {
            // try to invoke command if it exists
            if (command is ConsoleCommand dc)
                dc.Invoke();
            else
            {
                if (args.Length < 2)
                {
                    Log("Missing parameter!");
                    return;
                }
                if (command is ConsoleCommand<int> ccInt)
                {
                    int i;
                    if (int.TryParse(args[1], out i))
                        ccInt.Invoke(i);
                }
                if (command is ConsoleCommand<float> ccFloat)
                {
                    float f;
                    if (float.TryParse(args[1], NumberStyles.Number, CultureInfo.InvariantCulture, out f))
                        ccFloat.Invoke(f);
                }
                if (command is ConsoleCommand<bool> ccBool)
                {
                    bool b;
                    if (bool.TryParse(args[1], out b))
                        ccBool.Invoke(b);
                }
                if (command is ConsoleCommand<string> ccStr)
                {
                    ccStr.Invoke(args[1]);
                }
            }
        } else
        {
            Log("Unknown command!");
        }
    }

    public static void Log(object msg, bool useUnity = false, bool verbose = false)
    {
        if (useUnity) UnityEngine.Debug.Log(msg);
        else
        {
            if (verbose && !local.devMode) return;
            if (string.IsNullOrEmpty(local.consoleLog.text)) local.consoleLog.text += msg;
            else local.consoleLog.text += "\n" + msg;
            local.consoleLogScroll.verticalNormalizedPosition = 0;
            Canvas.ForceUpdateCanvases();
        }
    }

    public void DebugLog(string cond, string stack, LogType type)
    {
        string typeStr = "";
        if (type == LogType.Exception) typeStr = "<color=#990000>[EXCEPTION]</color>";
        else if (type == LogType.Error) typeStr = "<color=red>[ERROR]</color>";
        else if (type == LogType.Warning) typeStr = "<color=orange>[WARNING]</color>";
        if (UnityEngine.Debug.isDebugBuild) Log(typeStr + cond + stack);
        else Log(typeStr + cond);
    }

    private void Awake()
    {
        if (local != null) { Destroy(gameObject); return; }
        local = this;
        DontDestroyOnLoad(local);
        Application.logMessageReceived += DebugLog;
        if (UnityEngine.Debug.isDebugBuild)
        {
#if UNITY_EDITOR
            devMode = true;
#endif
            allowConsole = true;
        }
        if (devMode) cheats = true;
        CmdArgs();
        RegisterCommands();
    }

#if UNITY_EDITOR
    private void OnDisable()
    {
        Application.logMessageReceived -= DebugLog;
    }
#endif

    private void RegisterCommands()
    {
        new ConsoleCommand("iamthelaw", "Enable/disable cheats", "iamthelaw", () => { if (devMode) Log("I am already the law."); EnableCheats(!cheats); if (cheats) Log("I am the law! Ha, ha, ha!"); else Log("Now i'm just an regular person"); });
        new ConsoleCommand<string>("man", "Print an infomation based on <string>", "man <string>", (x) => { ConsoleCommandBase command; if (ConsoleCommandBase.Commands.TryGetValue(x.ToLower(), out command)) GetCmdHelp(command); });
        new ConsoleCommand("clear", "Clear the console log", "clear", () => { consoleLog.text = ""; });
        new ConsoleCommand("quit", "Exit the application", "quit", () => { SendApplicationEvent(AppEvent.Quit); });
        new ConsoleCommand<float>("speed", "Set the game timescale to <scale>", "speed <scale>", (x) => { SetGameSpeed(x); }, CommandFlag.CHEAT);
        new ConsoleCommand<bool>("bunnyhop", "When enabled, it allows to exclude running speed limit", "bunnyhop <true/false>", (x) => { enableBunnyHop = x; if (enableBunnyHop) Log("Quake 3 mode activated!"); else Log("Call Of Duty mode activated."); });
        new ConsoleCommand<bool>("autobunnyhop", "When enabled, it allows to jump even when the jump button is still holding", "autobunnyhop", (x) => { autoBunnyHop = x; if (autoBunnyHop) Log("Bouncy, bouncy!"); else Log("bruh"); });
    }

    private void GetCmdHelp(ConsoleCommandBase command)
    {
        if (!ConsoleCommandBase.Commands.ContainsKey(command.Id))
        {
            Log(command.Id + " has no information.");
            return;
        }
        string flagsStr = "";
        if (command.Flags != null)
        {
            if (command.Flags.Contains(CommandFlag.NO_HELP))
            {
                Log(command.Id + " has no help flag.");
                return;
            }
            foreach (CommandFlag flag in command.Flags)
            {
                if (string.IsNullOrEmpty(flagsStr)) flagsStr += flag;
                else flagsStr += ' ' + flag;
            }
            Log(string.Format("{0} - {1} [{2}]", command.Format, command.Description, flagsStr));
        }
        else
        {
            Log(string.Format("{0} - {1}", command.Format, command.Description));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote) || (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.C))
        {
            if (!console.activeSelf && allowConsole) OpenConsole();
        }

        if (Input.GetKeyDown(KeyCode.Return) && console.activeSelf)
        {
            if (string.IsNullOrEmpty(consoleInput.text)) return;
            ConsoleCommand(consoleInput.text);
            consoleInput.text = "";
            consoleInput.ActivateInputField();
        }

        if (Input.GetButtonDown("Pause"))
        {
            if (console.activeSelf) { if (!pauseMenu.activeSelf) { MakeUncontrollable(false); if (playing) ShowCursor(false); } console.SetActive(false); consoleInput.DeactivateInputField(); return; }
            if (pauseMenu.activeSelf) { Resume(); return; }
            if (!playing) return;
            pauseMenu.SetActive(true);
            Pause(true);
            ShowCursor(true);
        }
    }

    public void OpenConsole()
    {
        if (!pauseMenu.activeSelf) MakeUncontrollable(true);
        ShowCursor(true);
        console.SetActive(true);
        consoleInput.ActivateInputField();
    }

    public void EnableCheats(bool enable)
    {
        if (devMode)
        {
            Log("Cheats has already been enabled because you using Developer Mode.");
            return;
        }
        cheats = enable;
    }

    private void CmdArgs()
    {
        string[] args = Environment.CommandLine.Split(" ");
        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-dev":
                    if (!devMode) devMode = true;
                    break;
                case "-console":
                    if (!allowConsole) allowConsole = true;
                    break;
                case "-iamthelaw":
                    EnableCheats(true);
                    break;
            }
        }
    }

    public static void ShowCursor(bool value)
    {
        Cursor.visible = value;
        Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void Resume()
    {
        if (!paused) return;
        Pause(false);
        ShowCursor(false);
        pauseMenu.SetActive(false);
    }

    public void Pause(bool value)
    {
        paused = value;
        if (paused) Time.timeScale = 0;
        else Time.timeScale = timeScale;
    }

    public void SetGameSpeed(float value)
    {
        timeScale = value;
        if (!paused) Time.timeScale = timeScale;
    }

    public void MakeUncontrollable(bool value)
    {
        dontControl = value;
        if (dontControl)
        {
            InputManager.local.move = Vector2.zero;
            InputManager.local.look = Vector2.zero;
            InputManager.local.jump = false;
            InputManager.local.sprint = false;
            InputManager.local.walk = false;
        }
    }

    public void SendApplicationEvent(AppEvent appEvent)
    {
        if (appEvent == AppEvent.Quit)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            return;
#endif
            Application.Quit();
        }
        else if (appEvent == AppEvent.Restart)
        {
#if UNITY_EDITOR
            Log("Can't restart while in editor");
            return;
#endif
            Process.Start(Application.dataPath.Replace("_Data", ".exe"));
            Application.Quit();
        }
    }

    public void Menu()
    {
        if (btnTooltip.activeSelf) btnTooltip.SetActive(false);
        playing = false;
        pauseMenu.SetActive(false);
        Pause(false);
        SceneManager.LoadScene(0);
    }

    public void RestartOrQuit()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) SendApplicationEvent(AppEvent.Restart);
        else SendApplicationEvent(AppEvent.Quit);
    }
}

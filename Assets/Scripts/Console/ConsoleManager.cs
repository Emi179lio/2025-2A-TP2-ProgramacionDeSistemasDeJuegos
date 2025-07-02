using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ConsoleManager : MonoBehaviour, ILogHandler
{
    public static ConsoleManager _Instance;

    [Header("UI References")]
    public TextMeshProUGUI logText;
    public GameObject consoleUI;

    private ILogHandler unityLogHandler;
    private Dictionary<string, string> aliases = new();
    private Dictionary<string, System.Action<string>> commands = new();

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            unityLogHandler = Debug.unityLogger.logHandler;
            Debug.unityLogger.logHandler = this;

            if (logText == null)
                unityLogHandler.LogFormat(LogType.Warning, this, $"{name} (ConsoleManager): The 'logText' field is not assigned.");
            if (consoleUI == null)
                unityLogHandler.LogFormat(LogType.Warning, this, $"{name} (ConsoleManager): The 'consoleUI' field is not assigned.");

            RegisterCommands();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && consoleUI != null)
        {
            consoleUI.SetActive(!consoleUI.activeSelf);
        }
    }

    public void RegisterAlias(string alias, string original)
    {
        aliases[alias] = original;
    }

    private void RegisterCommands()
    {
        commands["help"] = arg =>
        {
            if (commands.ContainsKey(arg))
                SendLog($"Command '{arg}' executes: {commands[arg].Method.Name}");
            else
                SendLog("Command not found.");
        };
        commands["aliasses"] = arg =>
        {
            SendLog("Registered aliases:");
            foreach (var a in aliases)
                SendLog($"{a.Key} => {a.Value}");
        };
        commands["playanimation"] = animationName =>
        {
            var characters = FindObjectsOfType<Character>();
            foreach (var c in characters)
            {
                var anim = c.GetComponentInChildren<Animator>();
                if (anim)
                    anim.Play(animationName);
            }
        };
    }

    public void ExecuteCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            SendLog("Empty command.");
            return;
        }
        var split = input.Split(' ');
        string cmd = split[0].ToLower();
        string arg = split.Length > 1 ? split[1] : "";

        if (aliases.ContainsKey(cmd))
            cmd = aliases[cmd];

        if (commands.TryGetValue(cmd, out var action))
        {
            action.Invoke(arg);
        }
        else
        {
            SendLog($"Unknown command: {cmd}");
        }
    }

    public void SendLog(string msg)
    {
        if (logText != null)
            logText.text += msg + "\n";
    }

    public void LogFormat(LogType logType, Object context, string format, params object[] args)
    {
        string log = string.Format(format, args);
        if (logText != null)
            logText.text += log + "\n";
        unityLogHandler.LogFormat(logType, context, format, args);
    }

    public void LogException(System.Exception exception, Object context)
    {
        if (logText != null)
            logText.text += exception.ToString() + "\n";
        unityLogHandler.LogException(exception, context);
    }
}

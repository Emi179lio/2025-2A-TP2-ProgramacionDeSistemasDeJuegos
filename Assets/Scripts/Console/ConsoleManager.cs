using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class ConsoleManager : MonoBehaviour, ILogHandler
{
    public static ConsoleManager _Instance;

    [Header("UI References")]
    public TextMeshProUGUI logText;
    public GameObject consoleUI;
    public TMP_InputField commandInputField;

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
            if (commandInputField == null)
                unityLogHandler.LogFormat(LogType.Warning, this, $"{name} (ConsoleManager): The 'commandInputField' field is not assigned.");

            RegisterCommands();

            if (commandInputField != null)
                commandInputField.onSubmit.AddListener(OnCommandSubmit);
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

            if (consoleUI.activeSelf && commandInputField != null)
            {
                commandInputField.ActivateInputField(); 
            }
        }
    }

    private void OnCommandSubmit(string input)
    {
        ExecuteCommand(input);
        if (commandInputField != null)
        {
            commandInputField.text = string.Empty;
            commandInputField.ActivateInputField();
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
            if (string.IsNullOrEmpty(arg))
            {
                SendLog("Available commands:");
                foreach (var cmd in commands.Keys)
                    SendLog($"- {cmd}");
            }
            else
            {
                if (commands.ContainsKey(arg))
                    SendLog($"Command '{arg}' is registered. Method: {commands[arg].Method.Name}");
                else
                    SendLog($"Command '{arg}' not found.");
            }
        };

        commands["aliasses"] = arg =>
        {
            SendLog("Registered aliases:");
            foreach (var a in aliases)
                SendLog($"{a.Key} => {a.Value}");
        };

        commands["playanimation"] = animationName =>
        {
            if (string.IsNullOrEmpty(animationName))
            {
                SendLog("Usage: playanimation <AnimationName>");
                return;
            }

            var characters = FindObjectsOfType<Character>();
            int played = 0;
            foreach (var c in characters)
            {
                var anim = c.GetComponentInChildren<Animator>();
                if (anim)
                {
                    anim.Play(animationName);
                    played++;
                }
            }

            if (played > 0)
                SendLog($"Played animation '{animationName}' on {played} character(s).");
            else
                SendLog($"No Animator found on any Character.");
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

    public void SubmitCommandFromButton()
    {
        OnCommandSubmit(commandInputField.text);
    }
}

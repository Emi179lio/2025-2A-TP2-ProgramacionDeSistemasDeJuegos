using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    private Dictionary<string, string> commandDescriptions = new();

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
        if (GameManager.OnPressF1() && consoleUI != null)
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
        RegisterAlias("pa", "playanimation");
        RegisterAlias("anima", "playanimation");
        RegisterAlias("h", "help");

        commandDescriptions["help"] = "help [command] — Displays all commands or details of a specific one.";
        commandDescriptions["aliasses"] = "aliasses <command> — Lists the aliases registered for a specific command.";
        commandDescriptions["playanimation"] = "playanimation <AnimationName> — Plays the animation on all characters.";

        commands["help"] = arg =>
        {
            if (string.IsNullOrEmpty(arg))
            {
                SendLog("Available commands:");
                foreach (var cmd in commands.Keys)
                {
                    if (commandDescriptions.TryGetValue(cmd, out string description))
                        SendLog($"- {description}");
                    else
                        SendLog($"- {cmd}");
                }
            }
            else
            {
                if (commands.ContainsKey(arg))
                {
                    if (commandDescriptions.TryGetValue(arg, out string description))
                        SendLog(description);
                    else
                        SendLog($"Command '{arg}' is registered but has no description.");
                }
                else
                {
                    SendLog($"Command '{arg}' not found.");
                }
            }
        };

        commands["aliasses"] = commandName =>
        {
            if (string.IsNullOrEmpty(commandName))
            {
                if (aliases.Count == 0)
                {
                    SendLog("No aliases registered.");
                    return;
                }

                SendLog("All registered aliases:");
                foreach (var a in aliases)
                    SendLog($"- {a.Key} -> {a.Value}");
            }
            else
            {
                var matchingAliases = aliases
                    .Where(a => a.Value == commandName)
                    .Select(a => a.Key)
                    .ToList();

                if (matchingAliases.Count > 0)
                {
                    SendLog($"Aliases registered for '{commandName}':");
                    foreach (var alias in matchingAliases)
                        SendLog($"- {alias}");
                }
                else
                {
                    SendLog($"No aliases found for command '{commandName}'.");
                }
            }
        };

        commands["playanimation"] = animationName =>
        {
            if (string.IsNullOrEmpty(animationName))
            {
                SendLog("Usage: playanimation <AnimationName>");
                return;
            }

            var characters = Object.FindObjectsByType<Character>(FindObjectsSortMode.None);
            int played = 0;
            int missing = 0;

            foreach (var c in characters)
            {
                var anim = c.GetComponentInChildren<Animator>();
                if (anim)
                {
                    if (anim.HasState(0, Animator.StringToHash(animationName)))
                    {
                        anim.Play(animationName);
                        played++;
                    }
                    else
                    {
                        missing++;
                    }
                }
            }

            if (played > 0)
            {
                SendLog($"Played animation '{animationName}' on {played} character(s).");
                if (missing > 0)
                    SendLog($"Warning: {missing} character(s) do not have the animation '{animationName}'.");
            }
            else
            {
                SendLog($"No characters had the animation '{animationName}'.");
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

    public void SubmitCommandFromButton()
    {
        OnCommandSubmit(commandInputField.text);
    }
}

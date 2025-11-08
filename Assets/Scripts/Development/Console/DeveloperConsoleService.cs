using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Zenject;

public class DeveloperConsoleService : IDeveloperConsoleOutput
{
    private const int MaxLogEntries = 256;
    private const int MaxHistoryEntries = 64;

    private readonly Dictionary<string, IDeveloperConsoleCommand> _commands = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _aliasLookup = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<DeveloperConsoleLogEntry> _log = new();
    private readonly List<string> _history = new();

    private readonly GameModel _gameModel;
    private readonly GameViewModel _gameViewModel;
    private readonly IGameCommandExecutor _commandExecutor;
    private readonly ITurnService _turnService;
    private readonly ActionResolver _actionResolver;
    private readonly AdventureCommander _adventureCommander;
    private readonly GridCommander _gridCommander;

    public event Action LogUpdated;

    public bool IsVisible { get; private set; }

    public IReadOnlyList<DeveloperConsoleLogEntry> Log => _log;
    public IReadOnlyList<string> History => _history;
    public IEnumerable<IDeveloperConsoleCommand> RegisteredCommands => _commands.Values.Distinct();
    [Inject]
    public DeveloperConsoleService(
        IEnumerable<IDeveloperConsoleCommand> commands,
        GameModel gameModel,
        GameViewModel gameViewModel,
        IGameCommandExecutor commandExecutor,
        ITurnService turnService,
        ActionResolver actionResolver,
        AdventureCommander adventureCommander = null,
        GridCommander gridCommander = null)
    {
        _gameModel = gameModel ?? throw new ArgumentNullException(nameof(gameModel));
        _gameViewModel = gameViewModel ?? throw new ArgumentNullException(nameof(gameViewModel));
        _commandExecutor = commandExecutor ?? throw new ArgumentNullException(nameof(commandExecutor));
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
        _adventureCommander = adventureCommander;
        _gridCommander = gridCommander;

        if (commands != null)
        {
            foreach (var command in commands)
            {
                RegisterCommand(command);
            }
        }

        AppendLine("Консоль разработчика готова. Введите 'help' для отображения доступных команд.");
    }

    public void RegisterCommand(IDeveloperConsoleCommand command)
    {
        if (command == null)
        {
            return;
        }

        if (_commands.TryGetValue(command.Key, out var existing) && existing != command)
        {
            AppendWarning($"Command '{command.Key}' is already registered. Overriding previous instance.");
        }

        _commands[command.Key] = command;

        if (command.Aliases != null)
        {
            foreach (var alias in command.Aliases)
            {
                if (string.IsNullOrWhiteSpace(alias))
                {
                    continue;
                }

                if (_aliasLookup.TryGetValue(alias, out var currentKey) && !string.Equals(currentKey, command.Key, StringComparison.OrdinalIgnoreCase))
                {
                    AppendWarning($"Alias '{alias}' is already mapped to '{currentKey}'. Overriding with '{command.Key}'.");
                }

                _aliasLookup[alias] = command.Key;
            }
        }
    }

    public void ToggleVisibility()
    {
        IsVisible = !IsVisible;
    }

    public void SetVisibility(bool visible)
    {
        IsVisible = visible;
    }

    public void Submit(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        AppendLine($"> {input}");
        PushHistory(input);

        var tokens = Tokenize(input);
        if (tokens.Count == 0)
        {
            return;
        }

        var commandKey = tokens[0];
        var args = tokens.Skip(1).ToList();

        if (string.Equals(commandKey, "help", StringComparison.OrdinalIgnoreCase) || string.Equals(commandKey, "?", StringComparison.OrdinalIgnoreCase))
        {
            HandleHelp(args);
            return;
        }

        if (string.Equals(commandKey, "clear", StringComparison.OrdinalIgnoreCase))
        {
            ClearLog();
            return;
        }

        var resolvedKey = ResolveKey(commandKey);
        if (resolvedKey == null)
        {
            AppendError($"Unknown command '{commandKey}'. Type 'help' to see all commands.");
            return;
        }

        if (!_commands.TryGetValue(resolvedKey, out var command))
        {
            AppendError($"Command '{resolvedKey}' is not registered.");
            return;
        }

        try
        {
            var context = new DeveloperConsoleCommandContext(
                _gameModel,
                _gameViewModel,
                _commandExecutor,
                _turnService,
                _actionResolver,
                _adventureCommander,
                _gridCommander);
            command.Execute(context, args, this);
        }
        catch (Exception ex)
        {
            AppendError($"Command '{resolvedKey}' failed: {ex.Message}");
            Debug.LogException(ex);
        }
    }

    public void ClearLog()
    {
        _log.Clear();
        LogUpdated?.Invoke();
    }

    private void HandleHelp(IReadOnlyList<string> args)
    {
        if (args == null || args.Count == 0)
        {
            AppendLine("Available commands:");
            foreach (var command in RegisteredCommands.OrderBy(c => c.Key))
            {
                AppendLine($" - {command.Key}: {command.Description}");
            }
            AppendLine("Use 'help <command>' for details.");
            AppendLine("Use 'clear' to reset the console output.");
            return;
        }

        var key = args[0];
        var resolvedKey = ResolveKey(key);
        if (resolvedKey == null || !_commands.TryGetValue(resolvedKey, out var commandDetails))
        {
            AppendError($"Command '{key}' not found.");
            return;
        }

        AppendLine($"Command: {commandDetails.Key}");
        AppendLine($"Description: {commandDetails.Description}");
        AppendLine($"Usage: {commandDetails.Usage}");

        if (commandDetails.Aliases != null && commandDetails.Aliases.Count > 0)
        {
            AppendLine($"Aliases: {string.Join(", ", commandDetails.Aliases)}");
        }
    }

    private void PushHistory(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        _history.Add(input);
        if (_history.Count > MaxHistoryEntries)
        {
            _history.RemoveAt(0);
        }
    }

    private string ResolveKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        if (_commands.ContainsKey(key))
        {
            return key;
        }

        if (_aliasLookup.TryGetValue(key, out var resolved))
        {
            return resolved;
        }

        return null;
    }

    private static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        if (string.IsNullOrWhiteSpace(input))
        {
            return tokens;
        }

        var builder = new StringBuilder();
        var inQuotes = false;

        foreach (var ch in input)
        {
            if (ch == '\"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(ch) && !inQuotes)
            {
                if (builder.Length > 0)
                {
                    tokens.Add(builder.ToString());
                    builder.Clear();
                }
                continue;
            }

            builder.Append(ch);
        }

        if (builder.Length > 0)
        {
            tokens.Add(builder.ToString());
        }

        return tokens;
    }

    public void AppendLine(string message)
    {
        AddLogEntry(message, DeveloperConsoleLogType.Info);
        Debug.Log(message);
    }

    public void AppendWarning(string message)
    {
        AddLogEntry(message, DeveloperConsoleLogType.Warning);
        Debug.LogWarning(message);
    }

    public void AppendError(string message)
    {
        AddLogEntry(message, DeveloperConsoleLogType.Error);
        Debug.LogError(message);
    }

    private void AddLogEntry(string message, DeveloperConsoleLogType type)
    {
        _log.Add(new DeveloperConsoleLogEntry(message, type));
        if (_log.Count > MaxLogEntries)
        {
            _log.RemoveAt(0);
        }
        LogUpdated?.Invoke();
    }
}

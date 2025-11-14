using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

/// <summary>
/// Simple in-game developer console that mirrors classic Source engine consoles.
/// Provides scrolling output, input submission, and history navigation.
/// </summary>
public partial class DeveloperConsoleView : MonoBehaviour
{
    [SerializeField] private bool focusInputOnOpen = true;
    [SerializeField] private float windowHeightFraction = 0.5f;

    private DeveloperConsoleService _service;
    private string _currentInput = string.Empty;
    private Vector2 _scrollPosition;
    private bool _scrollToBottom;
    private bool _shouldFocusInput;
    private int _historyIndex = -1;
    private readonly List<IDeveloperConsoleCommand> _suggestions = new();
    private int _suggestionIndex = -1;
    private static readonly IDeveloperConsoleCommand[] BuiltInCommands =
    {
        new BuiltInCommand("help", "Введите help для вывода всех комманд", "help [command]"),
        new BuiltInCommand("clear", "Введите clear, чтобы очистить консоль", "clear")
    };

    [Inject]
    public void Construct([InjectOptional] DeveloperConsoleService service)
    {
        if (service == null)
        {
            Debug.LogWarning("[DeveloperConsoleView] DeveloperConsoleService is not bound in this scene. Console view will be disabled.", this);
            enabled = false;
            return;
        }

        _service = service;
        _service.LogUpdated += OnLogUpdated;
    }

    private void OnDestroy()
    {
        if (_service != null)
        {
            _service.LogUpdated -= OnLogUpdated;
        }
    }

    public void ToggleVisibility()
    {
        if (_service == null)
        {
            return;
        }

        var newState = !_service.IsVisible;
        _service.SetVisibility(newState);
        if (!newState)
        {
            ClearSuggestions();
            _currentInput = string.Empty;
        }
        if (newState && focusInputOnOpen)
        {
            _shouldFocusInput = true;
        }
    }

    private void OnGUI()
    {
        if (_service == null || !_service.IsVisible)
        {
            return;
        }

        const float margin = 8f;
        var rect = new Rect(margin, margin, Screen.width - margin * 2f, Screen.height * Mathf.Clamp01(windowHeightFraction));
        GUI.ModalWindow(GetInstanceID(), rect, DrawConsoleWindow, "Developer Console");
    }

    private void DrawConsoleWindow(int windowId)
    {
        GUILayout.BeginVertical();

        if (_scrollToBottom)
        {
            _scrollPosition.y = float.MaxValue;
            _scrollToBottom = false;
        }

        using (var scrollScope = new GUILayout.ScrollViewScope(_scrollPosition, GUILayout.ExpandHeight(true)))
        {
            _scrollPosition = scrollScope.scrollPosition;
            foreach (var entry in _service.Log)
            {
                DrawLogEntry(entry);
            }
        }

        GUILayout.Space(4f);

        GUI.SetNextControlName("DeveloperConsoleInput");
        var previousInput = _currentInput;
        _currentInput = GUILayout.TextField(_currentInput, GUILayout.ExpandWidth(true));

        if (!string.Equals(previousInput, _currentInput, StringComparison.Ordinal))
        {
            UpdateSuggestions();
        }

        if (_shouldFocusInput)
        {
            GUI.FocusControl("DeveloperConsoleInput");
            _shouldFocusInput = false;
        }

        var inputBeforeHandling = _currentInput;
        HandleKeyboardInput();
        if (!string.Equals(inputBeforeHandling, _currentInput, StringComparison.Ordinal))
        {
            UpdateSuggestions();
        }

        var inputRect = GUILayoutUtility.GetLastRect();
        var suggestionHeight = DrawSuggestionPopup(inputRect);
        if (suggestionHeight > 0f)
        {
            GUILayout.Space(suggestionHeight);
        }

        GUILayout.Space(4f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Submit", GUILayout.Width(80f)))
        {
            SubmitCurrentInput();
        }
        if (GUILayout.Button("Clear", GUILayout.Width(80f)))
        {
            _service.ClearLog();
        }
        if (GUILayout.Button("Close", GUILayout.Width(80f)))
        {
            _service.SetVisibility(false);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void SubmitCurrentInput()
    {
        if (string.IsNullOrWhiteSpace(_currentInput))
        {
            return;
        }

        _service.Submit(_currentInput);
        _currentInput = string.Empty;
        _historyIndex = -1;
        _shouldFocusInput = true;
        ClearSuggestions();
    }

    // Expose minimal control for InputSystem routing
    public void SubmitFromInputActions() => SubmitCurrentInput();
    public void HistoryUp() => NavigateHistory(-1);
    public void HistoryDown() => NavigateHistory(1);
    public void Close() => _service?.SetVisibility(false);
    public void Open() => _service?.SetVisibility(true);

    private void HandleKeyboardInput()
    {
        var evt = Event.current;
        if (evt == null || evt.type != EventType.KeyDown)
        {
            return;
        }

        switch (evt.keyCode)
        {
            case KeyCode.Tab:
                if (TryAcceptSuggestion())
                {
                    evt.Use();
                }
                break;
            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                SubmitCurrentInput();
                evt.Use();
                break;
            case KeyCode.Escape:
                _service.SetVisibility(false);
                evt.Use();
                break;
            case KeyCode.UpArrow:
                NavigateHistory(-1);
                evt.Use();
                break;
            case KeyCode.DownArrow:
                NavigateHistory(1);
                evt.Use();
                break;
        }
    }

    private void NavigateHistory(int direction)
    {
        var history = _service.History;
        if (history == null || history.Count == 0)
        {
            return;
        }

        if (_historyIndex < 0)
        {
            _historyIndex = history.Count;
        }

        _historyIndex = Mathf.Clamp(_historyIndex + direction, 0, history.Count);

        if (_historyIndex >= history.Count)
        {
            _currentInput = string.Empty;
        }
        else
        {
            _currentInput = history[_historyIndex];
            // ensure caret at end
            GUI.SetNextControlName("DeveloperConsoleInput");
        }
        UpdateSuggestions();
    }

    private static void DrawLogEntry(DeveloperConsoleLogEntry entry)
    {
        var previousColor = GUI.color;
        GUI.color = entry.Type switch
        {
            DeveloperConsoleLogType.Warning => Color.yellow,
            DeveloperConsoleLogType.Error => Color.red,
            _ => Color.white
        };
        GUILayout.Label(entry.ToString());
        GUI.color = previousColor;
    }

    private void OnLogUpdated()
    {
        _scrollToBottom = true;
    }

    private void UpdateSuggestions()
    {
        if (_service == null)
        {
            ClearSuggestions();
            return;
        }

        var trimmed = (_currentInput ?? string.Empty).TrimStart();
        if (string.IsNullOrEmpty(trimmed))
        {
            ClearSuggestions();
            return;
        }

        var delimiterIndex = trimmed.IndexOfAny(new[] { ' ', '\t' });
        var prefix = delimiterIndex >= 0 ? trimmed.Substring(0, delimiterIndex) : trimmed;
        if (string.IsNullOrEmpty(prefix))
        {
            ClearSuggestions();
            return;
        }

        var matches = _service.RegisteredCommands
            .Where(command => MatchesPrefix(command, prefix))
            .OrderBy(command => command.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var builtIn in BuiltInCommands)
        {
            if (MatchesPrefix(builtIn, prefix))
            {
                matches.Add(builtIn);
            }
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _suggestions.Clear();
        foreach (var command in matches)
        {
            if (seen.Add(command.Key))
            {
                _suggestions.Add(command);
            }

            if (_suggestions.Count >= 8)
            {
                break;
            }
        }

        _suggestionIndex = _suggestions.Count > 0 ? 0 : -1;
    }

    private float DrawSuggestionPopup(Rect inputRect)
    {
        if (_suggestions.Count == 0)
        {
            return 0f;
        }

        var visibleCount = Mathf.Min(6, _suggestions.Count);
        var lineHeight = (GUI.skin.label?.lineHeight ?? 16f) + 4f;
        var height = visibleCount * lineHeight + 4f;

        var rect = new Rect(inputRect.x, inputRect.yMax + 2f, inputRect.width, height);
        GUI.BeginGroup(rect, GUIContent.none, GUI.skin.box);
        for (int i = 0; i < visibleCount; i++)
        {
            var item = _suggestions[i];
            var rowRect = new Rect(4f, 2f + i * lineHeight, rect.width - 8f, lineHeight);
            if (i == _suggestionIndex)
            {
                var highlightRect = new Rect(2f, rowRect.y - 1f, rect.width - 4f, lineHeight + 2f);
                var previousColor = GUI.color;
                GUI.color = new Color(0.2f, 0.5f, 0.9f, 0.25f);
                GUI.DrawTexture(highlightRect, Texture2D.whiteTexture);
                GUI.color = previousColor;
            }

            var description = string.IsNullOrEmpty(item.Description) ? string.Empty : $" вЂ” {item.Description}";
            GUI.Label(rowRect, $"{item.Key}{description}");
        }
        GUI.EndGroup();
        return rect.height + 2f;
    }

    private bool TryAcceptSuggestion()
    {
        if (_suggestions.Count == 0 || _suggestionIndex < 0 || _suggestionIndex >= _suggestions.Count)
        {
            return false;
        }

        var suggestion = _suggestions[_suggestionIndex];
        var trimmed = (_currentInput ?? string.Empty).TrimStart();
        var delimiterIndex = trimmed.IndexOfAny(new[] { ' ', '\t' });
        var remainder = delimiterIndex >= 0 ? trimmed.Substring(delimiterIndex) : string.Empty;

        _currentInput = suggestion.Key;
        if (string.IsNullOrWhiteSpace(remainder))
        {
            _currentInput += " ";
        }
        else
        {
            _currentInput += remainder;
        }

        _shouldFocusInput = true;
        UpdateSuggestions();
        return true;
    }

    private void ClearSuggestions()
    {
        _suggestions.Clear();
        _suggestionIndex = -1;
    }

    private static bool MatchesPrefix(IDeveloperConsoleCommand command, string prefix)
    {
        if (command == null || string.IsNullOrEmpty(prefix))
        {
            return false;
        }

        if (command.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (command.Aliases == null)
        {
            return false;
        }

        return command.Aliases.Any(alias => !string.IsNullOrEmpty(alias) && alias.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}



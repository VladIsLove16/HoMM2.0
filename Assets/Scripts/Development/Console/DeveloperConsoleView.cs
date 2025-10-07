using System;
using UnityEngine;
using Zenject;

/// <summary>
/// Simple in-game developer console that mirrors classic Source engine consoles.
/// Provides scrolling output, input submission, and history navigation.
/// </summary>
public class DeveloperConsoleView : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
    [SerializeField] private bool focusInputOnOpen = true;
    [SerializeField] private float windowHeightFraction = 0.5f;

    private DeveloperConsoleService _service;
    private string _currentInput = string.Empty;
    private Vector2 _scrollPosition;
    private bool _scrollToBottom;
    private bool _shouldFocusInput;
    private int _historyIndex = -1;

    [Inject]
    public void Construct(DeveloperConsoleService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _service.LogUpdated += OnLogUpdated;
    }

    private void OnDestroy()
    {
        if (_service != null)
        {
            _service.LogUpdated -= OnLogUpdated;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleVisibility();
        }
    }

    private void ToggleVisibility()
    {
        if (_service == null)
        {
            return;
        }

        var newState = !_service.IsVisible;
        _service.SetVisibility(newState);
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
        _currentInput = GUILayout.TextField(_currentInput, GUILayout.ExpandWidth(true));

        if (_shouldFocusInput)
        {
            GUI.FocusControl("DeveloperConsoleInput");
            _shouldFocusInput = false;
        }

        HandleKeyboardInput();

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
    }

    private void HandleKeyboardInput()
    {
        var evt = Event.current;
        if (evt == null || evt.type != EventType.KeyDown)
        {
            return;
        }

        switch (evt.keyCode)
        {
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
}

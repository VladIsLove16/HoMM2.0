using UnityEngine;
using Zenject;
using SharedView;

public class AttackActionPanel : CanvasGroupVisibilityPanelBase, IAttackActionPanel
{
    [SerializeField] private TMPro.TextMeshProUGUI damageText;
    private GameViewModel _gameViewModel;
    [SerializeField] bool enableOnStart = false;
    [Inject]
    public void Construct([InjectOptional] GameViewModel gameViewModel)
    {
        _gameViewModel = gameViewModel;
        if (_gameViewModel == null)
        {
            Debug.LogError("[AttackActionPanel] Missing GameViewModel binding. Panel disabled.", this);
            enabled = false;
            HidePanelImmediate();
            return;
        }

        _gameViewModel.DamageContextPreviewChanged += OnActionPreviewChanged;
        ToggleStartingVivsibilty();
    }
    private void ToggleStartingVivsibilty()
    {
        if (enableOnStart)
            ShowPanelImmediate();
        else
            HidePanelImmediate();
    }

    private void Show()
    {
        ShowPanel();
    }
    private void Show(DamageContextPreview info)
    {
        ShowPanel();
        damageText.text = $"Damage: {info.Damage.DamageAmount} \nDied: {info.Damage.DieAmount}";
        // Можно добавить отображение статуса, дебаффов и т.п.
    }

    private void Hide()
    {
        HidePanel();
    }

    private void OnActionPreviewChanged(DamageContextPreview preview)
    {
        if (preview?.Damage == null)
        {
            Hide();
            return;
        }

        Show(preview);
    }

    
   

    protected override void OnDestroy()
    {
        if (_gameViewModel != null)
        {
            _gameViewModel.DamageContextPreviewChanged -= OnActionPreviewChanged;
        }

        base.OnDestroy();
    }
}

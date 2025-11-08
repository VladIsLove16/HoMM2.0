using UnityEngine;
using Zenject;

public class AttackActionPanel : MonoBehaviour, IAttackActionPanel
{
    [SerializeField] private TMPro.TextMeshProUGUI damageText;
    [SerializeField] private GameObject panelRoot;
    private GameViewModel _gameViewModel;

    [Inject]
    public void Construct([InjectOptional] GameViewModel gameViewModel)
    {
        _gameViewModel = gameViewModel;
        if (_gameViewModel == null)
        {
            Debug.LogError("[AttackActionPanel] Missing GameViewModel binding. Panel disabled.", this);
            enabled = false;
            if (panelRoot != null) panelRoot.SetActive(false);
            return;
        }

        _gameViewModel.DamageContextPreviewChanged += OnActionPreviewChanged;
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

    private void Show(DamageContextPreview info)
    {
        panelRoot.SetActive(true);
        damageText.text = $"Damage: {info.Damage.DamageAmount} \nDied: {info.Damage.DieAmount}";
        // Можно добавить отображение статуса, дебаффов и т.п.
    }

    private void Hide()
    {
        panelRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_gameViewModel != null)
        {
            _gameViewModel.DamageContextPreviewChanged -= OnActionPreviewChanged;
        }
    }
}

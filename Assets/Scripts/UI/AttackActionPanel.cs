using UnityEngine;
using Zenject;

public class AttackActionPanel : MonoBehaviour, IAttackActionPanel
{
    [SerializeField] private TMPro.TextMeshProUGUI damageText;
    [SerializeField] private GameObject panelRoot;
    private GameViewModel _gameViewModel;

    [Inject]
    public void Construct(GameViewModel gameViewModel)
    {
        _gameViewModel = gameViewModel;
        _gameViewModel.ActionPreviewChanged += OnActionPreviewChanged;
    }

    private void OnActionPreviewChanged(ActionPreview preview)
    {
        if (preview .Damage == null)
            return;
        Show(preview);
    }

    private void Show(ActionPreview info)
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
            _gameViewModel.ActionPreviewChanged -= OnActionPreviewChanged;
        }
    }
}

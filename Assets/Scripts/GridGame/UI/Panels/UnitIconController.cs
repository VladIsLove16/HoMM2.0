using System.Collections;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitIconController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visuals")]
    [SerializeField] private Image unitIcon;
    [SerializeField] private Image unitTeamBorder;
    [SerializeField] private Color blueTeamColor = Color.blue;
    [SerializeField] private Color redTeamColor = Color.red;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private Animator portraitAnimator;
    [SerializeField] private string damageTriggerName = "Damage";

    [Header("Damage Popup")]
    [SerializeField] private TextMeshProUGUI damagePopupText;
    [SerializeField] private float damagePopupDuration = 1.25f;

    private UnitPortraitViewModel _viewModel;
    private readonly CompositeDisposable _bindings = new();
    private Coroutine _damageRoutine;

    private void Awake()
    {
        ClearDamagePopup();
        ClearVisuals();
    }

    public void Bind(UnitPortraitViewModel viewModel)
    {
        if (_viewModel == viewModel)
        {
            return;
        }

        Unbind();
        _viewModel = viewModel;
        if (_viewModel == null)
        {
            ClearVisuals();
            return;
        }

        ApplySprite(_viewModel.Icon);
        UpdateTeam(_viewModel.Team);
        UpdateTurnText(_viewModel.TurnOrder);

        _bindings.Add(_viewModel.TeamObservable.Subscribe(UpdateTeam));
        _bindings.Add(_viewModel.TurnOrderObservable.Subscribe(UpdateTurnText));
        _bindings.Add(_viewModel.DamageTaken.Subscribe(OnDamageTaken));
    }

    public void Unbind()
    {
        _viewModel?.ReleaseFocus();
        _bindings.Clear();
        _viewModel = null;
        ClearDamagePopup();
        ClearVisuals();
    }

    private void ApplySprite(Sprite sprite)
    {
        if (unitIcon != null)
        {
            unitIcon.sprite = sprite;
            unitIcon.enabled = sprite != null;
        }
    }

    private void UpdateTeam(Team team)
    {
        if (unitTeamBorder != null)
        {
            unitTeamBorder.color = team == Team.Blue ? blueTeamColor : redTeamColor;
        }
    }

    private void UpdateTurnText(int turnIndex)
    {
        if (turnText != null)
        {
            turnText.text = turnIndex.ToString();
        }
    }

    private void OnDamageTaken(UnitPortraitDamageEvent evt)
    {
        if (damagePopupText != null)
        {
            damagePopupText.gameObject.SetActive(true);
            damagePopupText.text = $"-{evt.Amount}";
            if (_damageRoutine != null)
            {
                StopCoroutine(_damageRoutine);
            }
            _damageRoutine = StartCoroutine(HideDamagePopup());
        }

        if (portraitAnimator != null && !string.IsNullOrEmpty(damageTriggerName))
        {
            portraitAnimator.SetTrigger(damageTriggerName);
        }
    }

    private IEnumerator HideDamagePopup()
    {
        yield return new WaitForSeconds(damagePopupDuration);
        ClearDamagePopup();
        _damageRoutine = null;
    }

    private void ClearDamagePopup()
    {
        if (_damageRoutine != null)
        {
            StopCoroutine(_damageRoutine);
            _damageRoutine = null;
        }

        if (damagePopupText != null)
        {
            damagePopupText.gameObject.SetActive(false);
        }
    }

    private void ClearVisuals()
    {
        ApplySprite(null);
        if (turnText != null)
        {
            turnText.text = string.Empty;
        }
        ClearDamagePopup();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _viewModel?.RequestFocus();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _viewModel?.ReleaseFocus();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _viewModel?.SelectUnit();
    }

    private void OnDestroy()
    {
        Unbind();
    }
}

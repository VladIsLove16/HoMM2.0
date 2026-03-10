using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

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
    [SerializeField] private string hoverOnTriggerName = "HoverOn";
    [SerializeField] private string hoverOffTriggerName = "HoverOff";

    [Header("Damage Popup")]
    [SerializeField] private TextMeshProUGUI damagePopupText;
    [SerializeField] private float damagePopupFadeIn = 0.1f;
    [SerializeField] private float damagePopupHold = 0.6f;
    [SerializeField] private float damagePopupFadeOut = 0.25f;
    [SerializeField] private float damagePopupMoveUp = 18f;

    private UnitPortraitViewModel _viewModel;
    private readonly CompositeDisposable _bindings = new();
    private Sequence _damageSequence;
    private Vector2 _damagePopupBasePos;
    private int _damageTriggerId;
    private int _hoverOnTriggerId;
    private int _hoverOffTriggerId;

    private void Awake()
    {
        _damageTriggerId = string.IsNullOrEmpty(damageTriggerName) ? 0 : Animator.StringToHash(damageTriggerName);
        _hoverOnTriggerId = string.IsNullOrEmpty(hoverOnTriggerName) ? 0 : Animator.StringToHash(hoverOnTriggerName);
        _hoverOffTriggerId = string.IsNullOrEmpty(hoverOffTriggerName) ? 0 : Animator.StringToHash(hoverOffTriggerName);

        if (damagePopupText != null)
        {
            var rect = damagePopupText.rectTransform;
            _damagePopupBasePos = rect.anchoredPosition;
            SetDamagePopupVisible(false, true);
        }
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
        _bindings.Add(_viewModel.IsHoveredObservable.Subscribe(OnHoveredChanged));
    }

    public void Unbind()
    {
        //_viewModel?.ReleaseFocus();
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
            damagePopupText.text = $"-{evt.Amount}";
            PlayDamagePopupAnimation();
        }

        if (portraitAnimator != null && _damageTriggerId != 0)
        {
            portraitAnimator.SetTrigger(_damageTriggerId);
        }
    }

    private void OnHoveredChanged(bool isHovered)
    {
        if (portraitAnimator == null)
            return;

        if (isHovered && _hoverOnTriggerId != 0)
        {
            portraitAnimator.SetTrigger(_hoverOnTriggerId);
        }
        else if (!isHovered && _hoverOffTriggerId != 0)
        {
            portraitAnimator.SetTrigger(_hoverOffTriggerId);
        }
    }

    private void ClearDamagePopup()
    {
        _damageSequence?.Kill();
        _damageSequence = null;
        SetDamagePopupVisible(false, true);
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

    private void PlayDamagePopupAnimation()
    {
        if (damagePopupText == null)
            return;

        _damageSequence?.Kill();

        var rect = damagePopupText.rectTransform;
        rect.anchoredPosition = _damagePopupBasePos;
        SetDamagePopupVisible(true, true, 0f);

        _damageSequence = DOTween.Sequence()
            .Append(damagePopupText.DOFade(1f, damagePopupFadeIn))
            .Join(rect.DOAnchorPos(_damagePopupBasePos + Vector2.up * damagePopupMoveUp, damagePopupFadeIn))
            .AppendInterval(damagePopupHold)
            .Append(damagePopupText.DOFade(0f, damagePopupFadeOut))
            .OnComplete(() => SetDamagePopupVisible(false, true));
    }

    private void SetDamagePopupVisible(bool visible, bool resetPosition, float? alphaOverride = null)
    {
        if (damagePopupText == null)
            return;

        if (resetPosition)
        {
            damagePopupText.rectTransform.anchoredPosition = _damagePopupBasePos;
        }

        var color = damagePopupText.color;
        color.a = alphaOverride ?? (visible ? 1f : 0f);
        damagePopupText.color = color;
        damagePopupText.gameObject.SetActive(visible);
    }
}

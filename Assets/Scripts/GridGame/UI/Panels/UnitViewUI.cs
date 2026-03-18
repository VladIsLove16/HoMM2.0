using System;
using TMPro;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class UnitViewUI : MonoBehaviour, IDisposable
{
    [SerializeField] private UnitHealthBar healthBar;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI healthAmountText;
    [SerializeField] private TextMeshProUGUI damagePopupText;
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private float damagePopupFadeIn = 0.08f;
    [SerializeField] private float damagePopupHold = 0.5f;
    [SerializeField] private float damagePopupFadeOut = 0.2f;
    [SerializeField] private float damagePopupMoveUp = 28f;

    private UnitViewModel _unitViewModel;
    private CompositeDisposable _disposables = new();
    private Camera _camera;
    private Sequence _damagePopupSequence;
    private Vector3 _damagePopupBaseLocalPosition;

    public virtual void Init(UnitViewModel vm)
    {
        _unitViewModel = vm;

        // Подписываемся на события ViewModel (MVVM)
        _unitViewModel.OnHealthChanged
            .Subscribe(_ => UpdateHealth())
            .AddTo(_disposables);

        _unitViewModel.OnAmountChanged
            .Subscribe(_ => UpdateAmount())
            .AddTo(_disposables);

        _unitViewModel.OnDamageTaken
            .Subscribe(ShowDamagePopup)
            .AddTo(_disposables);

        _unitViewModel.OnDeath
            .Subscribe(_ => OnDeath())
            .AddTo(_disposables);

        _unitViewModel.OnTurnStarted
            .Subscribe(_ => OnTurnStart())
            .AddTo(_disposables);

        healthBar.Init();
        CacheDamagePopupDefaults();

        UpdateHealth();
        UpdateAmount();
    }

    private void UpdateHealth()
    {
        // Use ViewModel properties instead of direct model access
        var health = _unitViewModel.Health;
        var maxHealth = _unitViewModel.MaxHealth;

        healthAmountText.text = $"{health}/{maxHealth}";

        float ratio = maxHealth > 0 ? (float)health / maxHealth : 0f;
        healthBar.SetRatio(ratio);
    }

    private void UpdateAmount()
    {
        amountText.text = _unitViewModel.Amount.ToString();
    }

    private void OnDeath()
    {
        amountText.color = Color.black;
        gameObject.SetActive(false);
    }

    private void OnTurnStart()
    {
    }

    public void Dispose()
    {
        _damagePopupSequence?.Kill();
        _disposables.Dispose();
    }

    private void LateUpdate()
    {
        if (!faceCamera)
            return;

        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (_camera != null)
        {
            transform.rotation = _camera.transform.rotation;
        }
    }

    private void CacheDamagePopupDefaults()
    {
        if (damagePopupText == null)
            return;

        _damagePopupBaseLocalPosition = damagePopupText.transform.localPosition;
        SetDamagePopupVisible(false, true, 0f);
    }

    private void ShowDamagePopup(DamageContext context)
    {
        if (damagePopupText == null || context == null)
            return;

        _damagePopupSequence?.Kill();

        damagePopupText.text = $"-{context.DamageAmount}";
        damagePopupText.transform.localPosition = _damagePopupBaseLocalPosition;
        SetDamagePopupVisible(true, true, 0f);

        _damagePopupSequence = DOTween.Sequence()
            .Append(DOTween.To(() => damagePopupText.alpha, value => damagePopupText.alpha = value, 1f, damagePopupFadeIn))
            .Join(damagePopupText.transform.DOLocalMoveY(_damagePopupBaseLocalPosition.y + damagePopupMoveUp, damagePopupFadeIn))
            .AppendInterval(damagePopupHold)
            .Append(DOTween.To(() => damagePopupText.alpha, value => damagePopupText.alpha = value, 0f, damagePopupFadeOut))
            .OnComplete(() => SetDamagePopupVisible(false, true, 0f));
    }

    private void SetDamagePopupVisible(bool visible, bool resetPosition, float alpha)
    {
        if (damagePopupText == null)
            return;

        if (resetPosition)
        {
            damagePopupText.transform.localPosition = _damagePopupBaseLocalPosition;
        }

        damagePopupText.alpha = alpha;
        damagePopupText.gameObject.SetActive(visible);
    }
}

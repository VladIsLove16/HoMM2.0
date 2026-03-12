using System;
using TMPro;
using UnityEngine;
using UniRx;

public class UnitViewUI : MonoBehaviour, IDisposable
{
    [SerializeField] private UnitHealthBar healthBar;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI healthAmountText;
    [SerializeField] private bool faceCamera = true;

    private UnitViewModel _unitViewModel;
    private CompositeDisposable _disposables = new();
    private Camera _camera;

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

        _unitViewModel.OnDeath
            .Subscribe(_ => OnDeath())
            .AddTo(_disposables);

        _unitViewModel.OnTurnStarted
            .Subscribe(_ => OnTurnStart())
            .AddTo(_disposables);

        healthBar.Init();

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
    }

    private void OnTurnStart()
    {
    }

    public void Dispose()
    {
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
}

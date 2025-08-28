using System;
using TMPro;
using UnityEngine;
using UniRx;

public class UnitViewUI : MonoBehaviour, IDisposable
{
    [SerializeField] private UnitHealthBar healthBar;
    [SerializeField] private TextMeshProUGUI amountText;

    private UnitViewModel _unitViewModel;
    private CompositeDisposable _disposables = new();

    public void Init(UnitViewModel vm)
    {
        _unitViewModel = vm;

        _unitViewModel.OnHealthChanged
            .Subscribe(_ => UpdateHealth())
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

        transform.rotation = new(Camera.main.transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
    }

    private void UpdateHealth()
    {
        // ��������� �������� �������� � ��������
        var health = _unitViewModel.Model.ModifiedStats.Health;
        var maxHealth = _unitViewModel.Model.ModifiedStats.MaxHealth;

        float ratio = maxHealth > 0 ? (float)health / maxHealth : 0f;
        healthBar.SetRatio(ratio);
    }

    private void UpdateAmount()
    {
        amountText.text = _unitViewModel.Model.Amount.ToString();
    }

    private void OnDeath()
    {
        amountText.color = Color.black;
        // ����� �������� ������ ������������, ��� ������ ��������
    }

    private void OnTurnStart()
    {
        // ����� �������� ���������� �����, �������� UI ���������� � �.�.
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}

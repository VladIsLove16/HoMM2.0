using Adventure.Domain.Inventory;
using Adventure.Presentation.Mushroom;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MushroomBookView : MonoBehaviour
{
    [Header("Blocked")]
    [SerializeField] private List<MushroomBookEntryView> entrySlots = new List<MushroomBookEntryView>();
    [SerializeField] private TextMeshProUGUI pageNumberText;
    [SerializeField] private UnitDefinitionSOCollection testMushrooms;
    private MushroomBookViewModel _viewModel;
    private CompositeDisposable _subscriptions = new CompositeDisposable();
    private readonly ReactiveCollection<UnitDefinitionSO> _reactiveEntries = new();
    public int PageCapacity => entrySlots?.Count ?? 0;
    public void Construct(MushroomBookViewModel viewModel)
    {
        if (_viewModel == viewModel)
            return;

        DisposeSubscriptions();
        _viewModel = viewModel;

        if (_viewModel == null)
        {
            RenderEntries(null);
            UpdatePageNumber(0, 0);
            return;
        }

        _subscriptions = new CompositeDisposable();

        _viewModel.PresentationMode
            .Subscribe(OnPresentationModeChanged)
            .AddTo(_subscriptions);

        _viewModel.CurrentPageEntries
            .ObserveCountChanged()
            .Subscribe(_ => RenderEntries(_viewModel.CurrentPageEntries))
            .AddTo(_subscriptions);

        _viewModel.CurrentPage
            .Subscribe(_ => RefreshPageNumber())
            .AddTo(_subscriptions);

        _viewModel.TotalPages
            .Subscribe(_ => RefreshPageNumber())
            .AddTo(_subscriptions);
        _viewModel.IsOpen
            .Subscribe(OnBookStateChanged)
            .AddTo(_subscriptions);

        OnPresentationModeChanged(_viewModel.PresentationMode.Value);
        RenderEntries(_viewModel.CurrentPageEntries);
        RefreshPageNumber();
    }

    [Button("Render Test Page")]
    private void RenderTestPage()
    {
        // Очистим коллекцию и добавим тестовые данные
        _reactiveEntries.Clear();
        foreach (var m in testMushrooms.GetAll())
        {
            _reactiveEntries.Add(m);
        }

        // Отрисуем "фиктивную" страницу
        RenderEntries(_reactiveEntries);
        UpdatePageNumber(0, 1);

        Debug.Log($"[MushroomBookView] Rendered {testMushrooms.GetAll()} test entries.");
    }
    [Button("ToglePresMode")]
    private void ToglePresMode()
    {
       foreach( var slot in entrySlots)
        {
            slot?.SetPresentationMode(slot.Mode == PresentationMode.Normal ? PresentationMode.Humanized : PresentationMode.Normal);
        }
    }

    //public void NextPage() => _viewModel?.NextPage();
    //public void PrevPage() => _viewModel?.PrevPage();
    //public void GoToPage(int pageIndex) => _viewModel?.GoToPage(pageIndex);

    private void SetModeHumanized(bool humanized)
    {
        _viewModel?.SetPresentationMode(humanized ? PresentationMode.Humanized : PresentationMode.Normal);
    }

    private void Open()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    private void OnPresentationModeChanged(PresentationMode mode)
    {
        foreach (var slot in entrySlots)
        {
            slot?.SetPresentationMode(mode);
        }
    }
    private void RenderEntries(IReadOnlyReactiveCollection<UnitDefinitionSO> entries)
    {
        var count = entries?.Count ?? 0;
        for (var i = 0; i < entrySlots.Count; i++)
        {
            var slot = entrySlots[i];
            if (slot == null)
                continue;

            if (i < count)
            {
                slot.Bind(entries[i]);
                slot.SetPresentationMode(_viewModel == null ? PresentationMode.Normal : _viewModel.PresentationMode.Value);
            }
            else
            {
                slot.Bind(null);
            }
        }
    }
    private void OnBookStateChanged(bool isOpen)
    {
        gameObject.SetActive(isOpen);
    }
    private void RefreshPageNumber()
    {
        if (_viewModel == null)
        {
            UpdatePageNumber(0, 0);
            return;
        }

        UpdatePageNumber(_viewModel.CurrentPage.Value, _viewModel.TotalPages.Value);
    }

    private void UpdatePageNumber(int currentPage, int totalPages)
    {
        if (pageNumberText == null)
            return;

        pageNumberText.text = totalPages > 0 ? (currentPage + 1).ToString() : "0";
    }

    private void DisposeSubscriptions()
    {
        if (_subscriptions != null)
        {
            _subscriptions.Dispose();
            _subscriptions = new CompositeDisposable();
        }
    }

    private void OnDestroy()
    {
        DisposeSubscriptions();
    }
}

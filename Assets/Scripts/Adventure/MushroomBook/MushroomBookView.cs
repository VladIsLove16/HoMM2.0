using Adventure.Domain.Inventory;
using Adventure.Presentation.Mushroom;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MushroomBookView : MonoBehaviour
{
    [Header("Blocked")]
    [SerializeField] private List<MushroomBookEntryView> pageSlots = new List<MushroomBookEntryView>();
    [SerializeField] private TextMeshProUGUI pageNumberText;
    [SerializeField] private UnitDefinitionSOCollection testMushrooms;
    private MushroomBookViewModel _viewModel;
    private CompositeDisposable _subscriptions = new CompositeDisposable();
    private readonly ReactiveCollection<MushroomBookEntryViewModel> _reactiveEntries = new();
    public int PageCapacity => pageSlots?.Count ?? 0;
    public IReadOnlyList<MushroomBookEntryView> Slots => pageSlots ?? (IReadOnlyList<MushroomBookEntryView>)Array.Empty<MushroomBookEntryView>();
    public string CurrentPageLabel => pageNumberText != null ? pageNumberText.text : string.Empty;
    [Inject]
    public void Construct(MushroomBookViewModel viewModel)
    {
        DisposeSubscriptions();
        _viewModel = viewModel;

        if (_viewModel == null)
        {
            RenderEntries(null);
            UpdatePageNumber(0, 0);
            return;
        }

        _subscriptions = new CompositeDisposable();
        RenderEntries(_viewModel.CurrentPageEntries);
        viewModel.SetPageCapacity(pageSlots.Count);
        _viewModel.PresentationMode
            .Skip(1)
            .Subscribe(OnPresentationModeChanged)
            .AddTo(_subscriptions);

        _viewModel.CurrentPageEntries
            .ObserveCountChanged()
            .Skip(1)
            .Subscribe(_ => RenderEntries(_viewModel.CurrentPageEntries))
            .AddTo(_subscriptions);

        _viewModel.CurrentPage
            .Skip(1)
            .Subscribe(_ => RefreshPageNumber())
            .AddTo(_subscriptions);

        _viewModel.IsOpen
            .Skip(1)
            .Subscribe(OnBookStateChanged)
            .AddTo(_subscriptions);

        RefreshPageNumber();
    }

    [Button("Render Test Page")]
    private void RenderTestPage()
    {
        _reactiveEntries.Clear();
        foreach (var mushroom in testMushrooms.GetAll())
        {
            _reactiveEntries.Add(new MushroomBookEntryViewModel(
                mushroom.UnitType,
                mushroom.DisplayName,
                mushroom.Description,
                mushroom.Icon,
                mushroom.HoveredIcon,
                mushroom.HumanizedIcon,
                mushroom.HumanizedHoveredIcon,
                Array.Empty<MushroomStatViewData>()));
        }

        RenderEntries(_reactiveEntries);
        UpdatePageNumber(0, 1);

        Debug.Log($"[MushroomBookView] Rendered {testMushrooms.GetAll()} test entries.");
    }
    [Button("ToglePresMode")]
    private void ToglePresMode()
    {
       foreach( var slot in pageSlots)
        {
            slot?.SetPresentationMode(slot.Mode == PresentationMode.Normal ? PresentationMode.Humanized : PresentationMode.Normal);
        }
    }

    private void SetModeHumanized(bool humanized)
    {
        _viewModel?.SetPresentationMode(humanized ? PresentationMode.Humanized : PresentationMode.Normal);
    }
    private void OnPresentationModeChanged(PresentationMode mode)
    {
        foreach (var slot in pageSlots)
        {
            slot?.SetPresentationMode(mode);
        }
    }
    private void RenderEntries(IReadOnlyReactiveCollection<MushroomBookEntryViewModel> entries)
    {
        if (pageSlots == null || pageSlots.Count == 0)
            return;

        var count = entries?.Count ?? 0;
        var mode = _viewModel?.PresentationMode.Value ?? PresentationMode.Normal;

        for (var i = 0; i < pageSlots.Count; i++)
        {
            var slot = pageSlots[i];
            if (slot == null)
                continue;

            if (i < count && entries != null)
            {
                slot.Bind(entries[i]);
                slot.SetPresentationMode(mode);
            }
            else
            {
                slot.Bind(null);
            }
        }
    }

    private void ClearSlots()
    {
        if (pageSlots == null)
            return;

        foreach (var slot in pageSlots)
        {
            slot?.Bind(null);
        }
    }

    private void OnBookStateChanged(bool isOpen)
    {
        Debug.Log("new book state" +  isOpen);
        gameObject.SetActive(isOpen);
    }
    private void RefreshPageNumber()
    {
        if (_viewModel == null)
        {
            UpdatePageNumber(0, 0);
            return;
        }

        UpdatePageNumber(_viewModel.CurrentPage.Value, _viewModel.TotalPages);
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

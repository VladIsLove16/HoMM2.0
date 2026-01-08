using Adventure.Domain.Inventory;
using Adventure.Presentation.Mushroom;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MushroomBookView : MonoBehaviour
{
    [Header("Blocked")]
    [SerializeField] private List<MushroomBookEntryView> pageSlots = new List<MushroomBookEntryView>();
    [SerializeField] private TextMeshProUGUI firstListNumberText;
    [SerializeField] private TextMeshProUGUI secondListNumberText;
    [SerializeField] private AdventureMushroomAssetMap testDefinitions;
    private MushroomBookViewModel _viewModel;
    private CompositeDisposable _subscriptions = new CompositeDisposable();
    private readonly ReactiveCollection<MushroomBookEntryViewModel> _reactiveEntries = new();
    public int PageCapacity => pageSlots?.Count ?? 0;
    public IReadOnlyList<MushroomBookEntryView> Slots => pageSlots ?? (IReadOnlyList<MushroomBookEntryView>)Array.Empty<MushroomBookEntryView>();
    public string FirstListNumberText => firstListNumberText != null ? firstListNumberText.text : string.Empty;
    public string SecondListNumberText => secondListNumberText != null ? secondListNumberText.text : string.Empty;
    [Inject]
    public void Construct(MushroomBookViewModel viewModel)
    {
        DisposeSubscriptions();
        _viewModel = viewModel;

        if (_viewModel == null)
        {
            RenderEntries(null);
            UpdateListNumbers(0, 0);
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
        ToglePresentationMode();
    }

    [Button("Render Test Page")]
    private void RenderTestPage()
    {
        _reactiveEntries.Clear();
        if (testDefinitions != null)
        {
            foreach (var type in testDefinitions.Types)
            {
                if (!testDefinitions.TryGetDefinition(type, out var definition))
                    continue;

                var shared = definition.SharedData;
                _reactiveEntries.Add(new MushroomBookEntryViewModel(
                    type,
                    string.IsNullOrWhiteSpace(definition.SharedData.DisplayName) ? type.ToString() : definition.SharedData.DisplayName,
                    definition.SharedData.Description,
                    shared?.Icon,
                    shared?.HoveredIcon,
                    shared?.HumanizedIcon,
                    shared?.HumanizedHoveredIcon,
                    Array.Empty<MushroomStatViewData>(),
                    null));
            }
        }

        RenderEntries(_reactiveEntries);
        UpdateListNumbers(0, 1);

        Debug.Log($"[MushroomBookView] Rendered test entries.");
    }
    [Button("ToglePresentationMode")]
    private void ToglePresentationMode()
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
                throw new IndexOutOfRangeException();

            slot.SetSlotIndex(i);
            slot.SwapRequested = HandleSwapRequested;

            if (i < count && entries != null)
            {
                slot.Bind(entries[i]);
                slot.SetPresentationMode(mode);
                slot.Show();
            }
            else
            {
                slot.Bind(null);
                slot.Hide();
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
        Debug.Log("new book open state" +  isOpen);
        gameObject.SetActive(isOpen);
        RenderEntries(_viewModel.CurrentPageEntries);
    }
    private void RefreshPageNumber()
    {
        if (_viewModel == null)
        {
            UpdateListNumbers(0, 0);
            return;
        }

        UpdateListNumbers(_viewModel.CurrentPage.Value, _viewModel.TotalPages);
    }

    private void UpdateListNumbers(int currentPage, int totalPages)
    {
        if (firstListNumberText == null)
            return;
        if (secondListNumberText == null)
            return;

        firstListNumberText.text = totalPages > 0 ? (currentPage*2).ToString() : "0";
        secondListNumberText.text = totalPages > 0 ? (currentPage*2 + 1).ToString() : "1";
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

    private void HandleSwapRequested(int fromIndex, int toIndex)
    {
        if (_viewModel == null)
            return;

        _viewModel.SwapCurrentPageEntries(fromIndex, toIndex);
        RenderEntries(_viewModel.CurrentPageEntries);
    }
}

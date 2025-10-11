using Adventure.Domain.Inventory;
using Adventure.Presentation.Mushroom;
using Adventure.Settings.ViewModel;
using Assets.Scripts.Adventure.Infrastructure.Input;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Adventure.Presentation.Mushroom
{
    public class MushroomBookViewModel : IDisposable, IActiveMenu
    {
        private const int _entriesPerPage = 4;
        private readonly MushroomInventoryModel _mushroomInventoryModel;
        private readonly UnitDefinitionSOCollection _catalog;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
        private readonly List<UnitDefinitionSO> _allEntries = new List<UnitDefinitionSO>();

        private readonly ReactiveCollection<UnitDefinitionSO> _currentPageEntries =
            new ReactiveCollection<UnitDefinitionSO>();
        private readonly ReactiveProperty<int> _currentPageIndex = new ReactiveProperty<int>(0);
        private readonly ReactiveProperty<int> _totalPages = new ReactiveProperty<int>(0);
        private readonly ReactiveProperty<PresentationMode> _presentationMode =
            new ReactiveProperty<PresentationMode>(global::PresentationMode.Normal);
        private readonly ReactiveProperty<bool> _isOpen = new(false);

        public MushroomBookViewModel(
            MushroomInventoryModel mushroomInventoryModel,
            UnitDefinitionSOCollection catalog)
        {
            _mushroomInventoryModel = mushroomInventoryModel;
            _catalog = catalog;

            RebuildEntries();
        }
        public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;
        public IReadOnlyReactiveCollection<UnitDefinitionSO> CurrentPageEntries => _currentPageEntries;
        public IReadOnlyReactiveProperty<int> CurrentPage => _currentPageIndex;
        public IReadOnlyReactiveProperty<int> TotalPages => _totalPages;
        public IReadOnlyReactiveProperty<PresentationMode> PresentationMode => _presentationMode;

        public void NextPage() => GoToPage(_currentPageIndex.Value + 1);
        public void PrevPage() => GoToPage(_currentPageIndex.Value - 1);

        public void Toggle()
        {
            if (_isOpen.Value)
                Close();
            else
                Open();
        }

        public void Open()
        {
            if (_isOpen.Value)
                return;

            _isOpen.SetValueAndForceNotify(true);
        }

        public void Close()
        {
            if (!_isOpen.Value)
                return;

            _isOpen.SetValueAndForceNotify(false);
        }
        public void GoToPage(int pageIndex)
        {
            var clamped = Mathf.Clamp(pageIndex, 0, Math.Max(0, _totalPages.Value - 1));
            if (clamped == _currentPageIndex.Value && _currentPageEntries.Count > 0)
                return;

            _currentPageIndex.Value = clamped;
            UpdateCurrentPage();
        }

        public void SetPresentationMode(PresentationMode mode)
        {
            if (_presentationMode.Value == mode)
                return;

            _presentationMode.Value = mode;
        }

        private void RebuildEntries()
        {
            _allEntries.Clear();
            foreach (var entry in _mushroomInventoryModel.Items)
            {
                if (!_catalog.TryGet(entry.Key, out var visuals))
                    continue;

                _allEntries.Add(visuals);
            }

            UpdatePagination();
        }

        private void UpdatePagination()
        {
            var total = Mathf.CeilToInt(_allEntries.Count / (float)_entriesPerPage);
            _totalPages.Value = Math.Max(0, total);

            if (_totalPages.Value == 0)
            {
                _currentPageIndex.Value = 0;
                _currentPageEntries.Clear();
                return;
            }

            _currentPageIndex.Value = Mathf.Clamp(_currentPageIndex.Value, 0, _totalPages.Value - 1);
            UpdateCurrentPage();
        }

        private void UpdateCurrentPage()
        {
            _currentPageEntries.Clear();

            if (_totalPages.Value == 0)
                return;

            var start = _currentPageIndex.Value * _entriesPerPage;
            var end = Mathf.Min(start + _entriesPerPage, _allEntries.Count);

            for (int i = start; i < end; i++)
            {
                _currentPageEntries.Add(_allEntries[i]);
            }
        }


        public void Dispose()
        {
            _subscriptions.Dispose();
            _currentPageEntries?.Dispose();
            _currentPageIndex?.Dispose();
            _totalPages?.Dispose();
            _presentationMode?.Dispose();
        }
    }
}

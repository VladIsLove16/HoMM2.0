using System.Collections.Generic;
using Adventure.Application.Inventory;
using Adventure.Domain.Inventory;
using UniRx;
using UnityEngine;

namespace Adventure.Presentation.Mushroom
{
    public sealed class MushroomBookInventoryPresenter : MonoBehaviour
    {
        [SerializeField] private MushroomBookController bookController;
        [SerializeField] private MushroomCatalogSO catalog;

        private MushroomInventoryService _inventoryService;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public void Construct(MushroomInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _inventoryService.Entries.ObserveCountChanged().Subscribe(_ => Refresh()).AddTo(_subscriptions);
            _inventoryService.Entries.ObserveAdd().Subscribe(_ => Refresh()).AddTo(_subscriptions);
            _inventoryService.Entries.ObserveReplace().Subscribe(_ => Refresh()).AddTo(_subscriptions);
            _inventoryService.Entries.ObserveRemove().Subscribe(_ => Refresh()).AddTo(_subscriptions);
            Refresh();
        }

        private void OnDestroy()
        {
            _subscriptions.Dispose();
        }

        private void Refresh()
        {
            if (_inventoryService == null || bookController == null || catalog == null)
                return;

            var viewData = new List<MushroomBookEntryViewData>();
            foreach (var entry in _inventoryService.Entries)
            {
                if (catalog.TryGetVisuals(entry.Item.Id, out var visuals))
                {
                    var enrichedCharacteristics = new List<string>(visuals.Characteristics ?? new List<string>())
                    {
                        $"Count: {entry.Amount}"
                    };
                    var enriched = new MushroomBookEntryViewData(
                        visuals.Name,
                        visuals.Description,
                        visuals.Icon,
                        visuals.HoveredIcon,
                        visuals.HumanizedIcon,
                        visuals.HumanizedHoveredIcon,
                        enrichedCharacteristics);
                    viewData.Add(enriched);
                }
            }

            bookController.SetRuntimeEntries(viewData);
        }
    }
}

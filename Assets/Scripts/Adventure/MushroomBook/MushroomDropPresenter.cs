using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Inventory;
using Adventure.Presentation.Mushroom;
using UnityEngine;
using Zenject;

namespace Adventure.MushroomBook
{
    /// <summary>
    /// View-layer presenter that listens for drop requests from the
    /// MushroomBookViewModel and spawns the visual collectible via IMushroomDropService.
    /// </summary>
    public sealed class MushroomDropPresenter : MonoBehaviour
    {
        private MushroomBookViewModel _viewModel;
        private IMushroomDropService _dropService;

        [Inject]
        public void Construct(MushroomBookViewModel viewModel, IMushroomDropService dropService)
        {
            _viewModel = viewModel;
            _dropService = dropService;

            if (_viewModel != null)
            {
                _viewModel.MushroomDropped += OnMushroomDropped;
            }
        }

        private void OnDestroy()
        {
            if (_viewModel != null)
            {
                _viewModel.MushroomDropped -= OnMushroomDropped;
            }
        }

        private void OnMushroomDropped(UnitType type)
        {
            _dropService?.TrySpawn(type);
        }
    }
}

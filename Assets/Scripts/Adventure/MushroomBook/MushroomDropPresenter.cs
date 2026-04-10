using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Inventory;
using Adventure.Infrastructure.Players;
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
        private AdventureMushroomAssetMap _assetMap;
        private ILocalAdventurePlayerProvider _localPlayerProvider;
       [SerializeField] private LayerMask _layerMask;

        private readonly Vector3 _spawnOffset = new Vector3(0f, 0.05f, 0f);
        private const float GroundRayDistance = 5f;
        [Inject]
            
        public void Construct(AdventureMushroomAssetMap assetMap, MushroomBookViewModel viewModel, ILocalAdventurePlayerProvider localPlayerProvider)
        {
            _viewModel = viewModel;
            _assetMap= assetMap;
            _localPlayerProvider = localPlayerProvider;

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
            TrySpawn(type);
        }

        public bool TrySpawn(UnitType type)
        {
            if (_assetMap == null || !_assetMap.TryGetAsset(type, out var prefab) || prefab == null)
                return false;

            var spawnPosition = ResolveSpawnPosition(prefab.transform.position);
            var spawnRotation = prefab.transform.rotation;

            var instance = Object.Instantiate(prefab, spawnPosition, spawnRotation);
            //_container?.InjectGameObject(instance.gameObject);
            instance.Configure(type, true);
            instance.Construct(_viewModel);

            var growth = instance.GetComponent<MushroomGrowthAnimator>();
            if (growth == null)
            {
                growth = instance.gameObject.AddComponent<MushroomGrowthAnimator>();
            }
            growth.Play();

            return true;
        }

        private Vector3 ResolveSpawnPosition(Vector3 fallback)
        {
            var player = _localPlayerProvider?.MovementController;
            if (player == null)
            {
                Debug.LogError("PlayerMovementController is not available for mushroom drop.", this);
                return fallback + _spawnOffset;

            }

            var origin = player.transform.position + _spawnOffset;
            var rayOrigin = origin + Vector3.up * 1f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, GroundRayDistance, _layerMask.value, QueryTriggerInteraction.Ignore))
            {
                origin = hit.point + _spawnOffset;
            }
            return origin;
        }
    }
}

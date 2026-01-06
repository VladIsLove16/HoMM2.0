using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Movement;
using UnityEngine;
using Zenject;

namespace Adventure.Infrastructure.Inventory
{
    public sealed class MushroomDropService : IMushroomDropService
    {
        private readonly AdventureMushroomAssetMap _assetMap;
        private readonly DiContainer _container;
        private readonly PlayerMovementController _player;

        private readonly Vector3 _spawnOffset = new Vector3(0f, 0.05f, 0f);
        private const float GroundRayDistance = 5f;

        public MushroomDropService(
            AdventureMushroomAssetMap assetMap,
            DiContainer container,
            PlayerMovementController playerController)
        {
            _assetMap = assetMap;
            _container = container;
            _player = playerController;
        }

        public bool TrySpawn(UnitType type)
        {
            if (_assetMap == null || !_assetMap.TryGetAsset(type, out var prefab) || prefab == null)
                return false;

            var spawnPosition = ResolveSpawnPosition(prefab.transform.position);
            var spawnRotation = prefab.transform.rotation;

            var instance = Object.Instantiate(prefab, spawnPosition, spawnRotation);
            _container?.InjectGameObject(instance.gameObject);
            instance.Configure(type, true);

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
            if (_player == null)
                return fallback + _spawnOffset;

            var origin = _player.transform.position + _spawnOffset;
            var rayOrigin = origin + Vector3.up * 1f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, GroundRayDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                origin = hit.point + _spawnOffset;
            }
            return origin;
        }
    }
}

using Adventure.Infrastructure.Interaction;
using UnityEngine;

namespace Adventure.Infrastructure.Inventory
{
    public sealed class MushroomCollectible : MonoBehaviour, IMushroomCollectible
    {
        [SerializeField] private string mushroomId;
        [SerializeField] private bool destroyOnCollect = true;

        public string MushroomId => mushroomId;
        public bool CanCollect { get; private set; } = true;

        public void Collect()
        {
            if (!CanCollect)
                return;

            CanCollect = false;
            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(mushroomId))
            {
                mushroomId = name;
            }
        }
    }
}

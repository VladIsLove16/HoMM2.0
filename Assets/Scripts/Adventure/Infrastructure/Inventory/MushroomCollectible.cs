using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.State;
using Adventure.Presentation.Mushroom;
using UnityEngine;
using Zenject;
namespace Adventure.Infrastructure.Inventory
{
    /// <summary>
    /// Runtime behaviour for mushroom pickups encountered in the world.
    /// </summary>
    public sealed class MushroomCollectible : MonoBehaviour, IMushroomCollectible
    {
        [SerializeField] private UnitType UnitType = UnitType.Archer;
        [SerializeField] private bool destroyOnCollect = true;
        public UnitType Type => UnitType;
        public bool CanCollect { get; private set; } = true;
        private MushroomBookViewModel _mushroomCollectionViewModel;

        private void Awake()
        {
            if (AdventureStateCache.IsMushroomCollected(transform.position))
            {
                CanCollect = false;
                if (destroyOnCollect && gameObject != null)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        [Inject]
        public void Construct(MushroomBookViewModel mushroomCollectionvm)
        {
            _mushroomCollectionViewModel = mushroomCollectionvm;
        }
        public void Interact(PlayerInteractionContext context)
        {
            _mushroomCollectionViewModel.Collect(Type);
            Collect(context);
        }

        public void Collect(PlayerInteractionContext context)
        {
            if (!CanCollect)
                return;

            CanCollect = false;
            AdventureStateCache.RegisterCollectedMushroom(transform.position);

            if (destroyOnCollect && gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        public string GetPrompt()
        {
            return "Collect " + UnitType.ToString();
        }
    }
}

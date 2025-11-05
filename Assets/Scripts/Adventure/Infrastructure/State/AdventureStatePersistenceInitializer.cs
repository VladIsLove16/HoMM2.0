using System;
using Adventure.Infrastructure.Persistence;
using Zenject;

namespace Adventure.Infrastructure.State
{
    public sealed class AdventureStatePersistenceInitializer : IInitializable
    {
        private readonly IDataRepository<GameStateSaveData> _repository;

        public AdventureStatePersistenceInitializer(IDataRepository<GameStateSaveData> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public void Initialize()
        {
            AdventureStateCache.ConfigurePersistence(_repository);
        }
    }
}

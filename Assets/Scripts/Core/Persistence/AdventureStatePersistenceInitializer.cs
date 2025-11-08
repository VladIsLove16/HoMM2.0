using System;
using Adventure.Infrastructure.Persistence;
using Zenject;

namespace Adventure.Infrastructure.State
{
    /// <summary>
    /// Configures battle state persistence so both adventure and grid layers share the same repository.
    /// </summary>
    public sealed class AdventureStatePersistenceInitializer : IInitializable
    {
        private readonly IDataRepository<GameStateSaveData> _repository;

        public AdventureStatePersistenceInitializer(IDataRepository<GameStateSaveData> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public void Initialize()
        {
            BattleStateCache.ConfigurePersistence(_repository);
        }
    }
}

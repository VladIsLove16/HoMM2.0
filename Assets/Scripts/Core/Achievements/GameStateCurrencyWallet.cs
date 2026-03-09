using Adventure.Infrastructure.Persistence;
using UnityEngine;

namespace Game.Achievements
{
    public sealed class GameStateCurrencyWallet : ICurrencyWallet
    {
        private readonly IDataRepository<GameStateSaveData> _repository;
        private int _balance;

        public GameStateCurrencyWallet(IDataRepository<GameStateSaveData> repository)
        {
            _repository = repository;
            Load();
        }

        public int Balance => _balance;

        public void Add(int amount)
        {
            if (amount == 0)
                return;

            _balance = Mathf.Max(0, _balance + amount);
            Save();
        }

        private void Load()
        {
            if (_repository == null)
            {
                _balance = 0;
                return;
            }

            var data = _repository.Load() ?? new GameStateSaveData();
            _balance = data.Currency;
        }

        private void Save()
        {
            if (_repository == null)
                return;

            var data = _repository.Load() ?? new GameStateSaveData();
            data.Currency = _balance;
            _repository.Save(data);
        }
    }
}

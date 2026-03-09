using UnityEngine;

namespace Game.Achievements
{
    public sealed class PlayerPrefsCurrencyWallet : ICurrencyWallet
    {
        private const string Key = "player_currency_balance";

        public int Balance => PlayerPrefs.GetInt(Key, 0);

        public void Add(int amount)
        {
            if (amount == 0)
                return;
            var next = Mathf.Max(0, Balance + amount);
            PlayerPrefs.SetInt(Key, next);
            PlayerPrefs.Save();
        }
    }
}

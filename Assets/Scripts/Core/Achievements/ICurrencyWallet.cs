namespace Game.Achievements
{
    public interface ICurrencyWallet
    {
        int Balance { get; }
        void Add(int amount);
    }
}

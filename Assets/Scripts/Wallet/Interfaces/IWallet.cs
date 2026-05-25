namespace GameLogic
{
    public interface IWallet
    {
        int Coins { get; }

        void AddCoins(int amount);
        bool TrySpendCoins(int amount);
    }
}
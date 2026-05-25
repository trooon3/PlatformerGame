using System;
using System.Collections.Generic;

namespace GameLogic
{
    public interface IWalletManager
    {
        event Action<WalletManager.CoinType, int> OnCoinsChanged;

        void AddCoins(WalletManager.CoinType type, int amount);
        bool TrySpendCoins(WalletManager.CoinType type, int amount);
        int GetCoins(WalletManager.CoinType type);

        Dictionary<WalletManager.CoinType, int> GetAllCoins();

        void SaveWallet();
        void LoadWallet();
        void LoadFromSaveData(CoinData saveData);
    }
}
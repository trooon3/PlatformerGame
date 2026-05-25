using UnityEngine;

namespace GameLogic
{
    public interface ICoin
    {
        WalletManager.CoinType CoinType { get; }

        int CoinValue { get; }
        bool IsCollectable { get; }

        void Collect();
        void EnableCollection();
    }
}
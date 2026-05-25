using System;
using UnityEngine;

namespace GameLogic
{
    public sealed class Wallet : MonoBehaviour, IWallet
    {
        public static Wallet Instance { get; private set; }

        private int _coins;
        public event Action<int> OnCoinsChanged;

        public int Coins => _coins;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddCoins(int amount)
        {
            _coins += amount;

            OnCoinsChanged?.Invoke(_coins);
        }

        public bool TrySpendCoins(int amount)
        {
            if (_coins < amount)
            {
                return false;
            }

            _coins -= amount;

            OnCoinsChanged?.Invoke(_coins);

            return true;
        }

        public int GetCoins() => _coins;
    }
}
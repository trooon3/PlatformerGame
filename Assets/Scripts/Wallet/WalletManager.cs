using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public sealed class WalletManager : MonoBehaviour, IWalletManager
    {
        public static WalletManager Instance { get; private set; }

        private const string CoinsPrefix = "Coins_";

        private Dictionary<CoinType, int> _coins = new Dictionary<CoinType, int>();
        public event Action<CoinType, int> OnCoinsChanged;

        public enum CoinType { Bronze, Silver, Gold }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                DontDestroyOnLoad(gameObject);
                InitializeWallet();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeWallet()
        {
            _coins[CoinType.Bronze] = 20;
            _coins[CoinType.Silver] = 20;
            _coins[CoinType.Gold] = 20;
        }

        public void AddCoins(CoinType type, int amount)
        {
            if (!_coins.ContainsKey(type))
            {
                _coins[type] = 0;
            }

            _coins[type] += amount;

            OnCoinsChanged?.Invoke(type, _coins[type]);
        }

        public bool TrySpendCoins(CoinType type, int amount)
        {
            if (!_coins.ContainsKey(type) || _coins[type] < amount)
            {
                return false;
            }

            _coins[type] -= amount;
            OnCoinsChanged?.Invoke(type, _coins[type]);

            return true;
        }

        public int GetCoins(CoinType type)
        {
            return _coins.ContainsKey(type) ? _coins[type] : 0;
        }

        public Dictionary<CoinType, int> GetAllCoins()
        {
            return new Dictionary<CoinType, int>(_coins);
        }

        public void SaveWallet()
        {
            foreach (var kvp in _coins)
            {
                PlayerPrefs.SetInt($"{CoinsPrefix}{kvp.Key}", kvp.Value);
            }

            PlayerPrefs.Save();
        }

        public void LoadWallet()
        {
            foreach (CoinType type in Enum.GetValues(typeof(CoinType)))
            {
                _coins[type] = PlayerPrefs.GetInt($"{CoinsPrefix}{type}", 0);
                OnCoinsChanged?.Invoke(type, _coins[type]);
            }
        }

        public void LoadFromSaveData(CoinData saveData)
        {
            if (!saveData.isInitialized)
            {
                return;
            }

            _coins[CoinType.Bronze] = saveData.bronze;
            _coins[CoinType.Silver] = saveData.silver;
            _coins[CoinType.Gold] = saveData.gold;

            OnCoinsChanged?.Invoke(CoinType.Bronze, saveData.bronze);
            OnCoinsChanged?.Invoke(CoinType.Silver, saveData.silver);
            OnCoinsChanged?.Invoke(CoinType.Gold, saveData.gold);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public sealed class WalletManager : MonoBehaviour, IWalletManager
    {
        private const string CoinsPrefix = "Coins_";
        private const int DefaultCoinAmount = 0;
        private const int MinimumCoinAmount = 0;

        public enum CoinType
        {
            Bronze = 0,
            Silver = 1,
            Gold = 2
        }

        public static WalletManager Instance { get; private set; }

        private readonly Dictionary<CoinType, int> _coins = new Dictionary<CoinType, int>();

        public event Action<CoinType, int> OnCoinsChanged;

        private void Awake()
        {
            InitializeSingleton();
        }

        public void AddCoins(CoinType type, int amount)
        {
            if (amount <= MinimumCoinAmount)
            {
                return;
            }

            EnsureCoinTypeExists(type);

            _coins[type] += amount;

            NotifyCoinChanged(type);
            SaveWallet();
        }

        public bool TrySpendCoins(CoinType type, int amount)
        {
            if (amount <= MinimumCoinAmount)
            {
                return false;
            }

            EnsureCoinTypeExists(type);

            if (_coins[type] < amount)
            {
                return false;
            }

            _coins[type] -= amount;

            NotifyCoinChanged(type);
            SaveWallet();

            return true;
        }

        public int GetCoins(CoinType type)
        {
            EnsureCoinTypeExists(type);

            return _coins[type];
        }

        public Dictionary<CoinType, int> GetAllCoins()
        {
            return new Dictionary<CoinType, int>(_coins);
        }

        public void SaveWallet()
        {
            foreach (KeyValuePair<CoinType, int> pair in _coins)
            {
                PlayerPrefs.SetInt($"{CoinsPrefix}{pair.Key}", pair.Value);
            }

            PlayerPrefs.Save();
        }

        public void LoadWallet()
        {
            foreach (CoinType type in Enum.GetValues(typeof(CoinType)))
            {
                _coins[type] = PlayerPrefs.GetInt($"{CoinsPrefix}{type}", DefaultCoinAmount);

                NotifyCoinChanged(type);
            }
        }

        public void LoadFromSaveData(CoinData saveData)
        {
            if (saveData.isInitialized == false)
            {
                return;
            }

            SetCoins(CoinType.Bronze, saveData.bronze);
            SetCoins(CoinType.Silver, saveData.silver);
            SetCoins(CoinType.Gold, saveData.gold);

            SaveWallet();
        }

        private void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;

                DontDestroyOnLoad(gameObject);
                InitializeWallet();

                return;
            }

            Destroy(gameObject);
        }

        private void InitializeWallet()
        {
            foreach (CoinType type in Enum.GetValues(typeof(CoinType)))
            {
                _coins[type] = DefaultCoinAmount;
            }

            LoadWallet();
        }

        private void SetCoins(CoinType type, int amount)
        {
            _coins[type] = Mathf.Max(MinimumCoinAmount, amount);

            NotifyCoinChanged(type);
        }

        private void EnsureCoinTypeExists(CoinType type)
        {
            if (_coins.ContainsKey(type))
            {
                return;
            }

            _coins[type] = MinimumCoinAmount;
        }

        private void NotifyCoinChanged(CoinType type)
        {
            OnCoinsChanged?.Invoke(type, _coins[type]);
        }
    }
}
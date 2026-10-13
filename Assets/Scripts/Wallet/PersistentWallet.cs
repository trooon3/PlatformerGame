using System;
using System.Collections;
using GameLogic;
using UnityEngine;

public sealed class PersistentWallet : MonoBehaviour, IPersistentWallet
{
    private const string BronzeCoinCommand = "1";
    private const string SilverCoinCommand = "2";
    private const string GoldCoinCommand = "3";

    private const string BronzeCoinName = "bronze";
    private const string SilverCoinName = "silver";
    private const string GoldCoinName = "gold";

    private const string SaveKey = "PlayerCoins";
    private const float SyncDelay = 0.5f;
    private const int MinimumCoinAmount = 0;

    [SerializeField] private CoinData _currentCoins;

    public static PersistentWallet Instance { get; private set; }

    public CoinData CurrentCoins => _currentCoins;

    public event Action<CoinData> OnCoinsUpdated;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        StartCoroutine(SyncWithSaveData());

        if (WalletManager.Instance != null)
        {
            WalletManager.Instance.OnCoinsChanged += HandleCoinsChanged;
        }
    }

    public void AddCoins(string coinType, int amount)
    {
        if (amount <= MinimumCoinAmount)
        {
            return;
        }

        CoinData newCoins = _currentCoins;

        switch (coinType.ToLower())
        {
            case BronzeCoinCommand:
            case BronzeCoinName:
                newCoins.bronze += amount;

                break;

            case SilverCoinCommand:
            case SilverCoinName:
                newCoins.silver += amount;

                break;

            case GoldCoinCommand:
            case GoldCoinName:
                newCoins.gold += amount;

                break;

            default:
                return;
        }

        newCoins.isInitialized = true;

        SetCoins(newCoins);
    }

    public void LoadCoinsFromSave(CoinData savedCoins)
    {
        CoinData validCoins = new CoinData(
            savedCoins.bronze,
            savedCoins.silver,
            savedCoins.gold);

        SetCoins(validCoins);
    }

    public void SaveCoins()
    {
        string json = JsonUtility.ToJson(_currentCoins);

        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadCoins()
    {
        if (PlayerPrefs.HasKey(SaveKey) == false)
        {
            _currentCoins = new CoinData();

            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);

        _currentCoins = JsonUtility.FromJson<CoinData>(json);

        if (_currentCoins.isInitialized == false)
        {
            _currentCoins = new CoinData(
                _currentCoins.bronze,
                _currentCoins.silver,
                _currentCoins.gold);
        }
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            LoadCoins();

            return;
        }

        Destroy(gameObject);
    }

    private IEnumerator SyncWithSaveData()
    {
        yield return new WaitForSeconds(SyncDelay);

        if (SaveSystem.Instance == null || SaveSystem.Instance.HasSave() == false)
        {
            yield break;
        }

        GameSaveData saveData = SaveSystem.Instance.CurrentSave;

        if (saveData == null || saveData.coins.isInitialized == false)
        {
            yield break;
        }

        LoadCoinsFromSave(saveData.coins);
    }

    private void SetCoins(CoinData coins)
    {
        _currentCoins = coins;
        SaveCoins();
        OnCoinsUpdated?.Invoke(_currentCoins);
    }

    private void HandleCoinsChanged(WalletManager.CoinType type, int newAmount)
    {
        CoinData newCoins = _currentCoins;

        switch (type)
        {
            case WalletManager.CoinType.Bronze:
                newCoins.bronze = newAmount;
                break;
            case WalletManager.CoinType.Silver:
                newCoins.silver = newAmount;
                break;
            case WalletManager.CoinType.Gold:
                newCoins.gold = newAmount;
                break;
        }

        newCoins.isInitialized = true;

        _currentCoins = newCoins;

        SaveCoins();
        OnCoinsUpdated?.Invoke(_currentCoins);
    }

    private void OnDestroy()
    {
        if (WalletManager.Instance != null)
        {
            WalletManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
        }
    }
}
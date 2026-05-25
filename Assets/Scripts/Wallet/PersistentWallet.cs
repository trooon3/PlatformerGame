using System;
using System.Collections;
using UnityEngine;

public sealed class PersistentWallet : MonoBehaviour, IPersistentWallet
{
    private const string CommandAddBronzeCoin = "1";
    private const string CommandAddSilverCoin = "2";
    private const string CommandAddGoldCoin = "3";

    public static PersistentWallet Instance { get; private set; }

    private const string SaveKey = "PlayerCoins";
    private const float SyncDelay = 0.5f;

    [SerializeField] private CoinData _currentCoins;

    public CoinData CurrentCoins => _currentCoins;

    public event Action<CoinData> OnCoinsUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            LoadCoins();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(SyncWithSaveData());
    }

    private IEnumerator SyncWithSaveData()
    {
        yield return new WaitForSeconds(SyncDelay);

        if (SaveSystem.Instance == null || !SaveSystem.Instance.HasSave())
        {
            yield break;
        }

        var saveData = SaveSystem.Instance.CurrentSave;

        if (saveData == null)
        {
            yield break;
        }

        if (saveData.coins.bronze > 0 || saveData.coins.silver > 0 || saveData.coins.gold > 0)
        {
            LoadCoinsFromSave(saveData.coins);
        }
    }

    public void AddCoins(string coinType, int amount)
    {
        CoinData newCoins = _currentCoins;

        switch (coinType.ToLower())
        {
            case CommandAddBronzeCoin:
                newCoins.bronze += amount;

                break;

            case CommandAddSilverCoin:
                newCoins.silver += amount;

                break;

            case CommandAddGoldCoin:
                newCoins.gold += amount;

                break;
        }

        _currentCoins = newCoins;

        SaveCoins();

        OnCoinsUpdated?.Invoke(_currentCoins);
    }

    public void LoadCoinsFromSave(CoinData savedCoins)
    {
        _currentCoins = new CoinData(savedCoins.bronze, savedCoins.silver, savedCoins.gold);

        SaveCoins();

        OnCoinsUpdated?.Invoke(_currentCoins);
    }

    public void LoadFromSaveData(CoinData saveData)
    {
        LoadCoinsFromSave(saveData);
    }

    public void SaveCoins()
    {
        string json = JsonUtility.ToJson(_currentCoins);

        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadCoins()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            _currentCoins = JsonUtility.FromJson<CoinData>(json);
        }
        else
        {
            _currentCoins = new CoinData();
        }
    }
}
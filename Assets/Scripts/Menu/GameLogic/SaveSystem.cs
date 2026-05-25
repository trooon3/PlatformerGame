using DoorControl;
using GameLogic;
using Player.Abilities;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using YG;

public sealed class SaveSystem : MonoBehaviour
{
    private const string SaveDirectory = "/Saves/";
    private const string SaveFileName = "game_save.json";

    public static SaveSystem Instance { get; private set; }
    public GameSaveData CurrentSave { get; private set; }

    public event Action<GameSaveData> OnGameLoaded;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame(string checkpointId, Vector3 playerPosition)
    {
        InitializeSaveDataIfNeeded();

        CurrentSave.saveId = Guid.NewGuid().ToString();
        CurrentSave.saveTime = DateTime.Now;
        CurrentSave.checkpointId = checkpointId;
        CurrentSave.playerPosition = playerPosition;
        CurrentSave.sceneName = SceneManager.GetActiveScene().name;

        UpdatePlayerData();
        UpdateKeyCollectionData();
        UpdateEnemyData();
        UpdateCoinData();

        SaveToFile();
        YG2.InterstitialAdvShow();
    }

    private void InitializeSaveDataIfNeeded()
    {
        int zeroValue = 0;

        int initialQuantityBronzeCoins = 0;
        int initialQuantitySilverCoins = 0;
        int initialQuantityGoldCoins = 0;

        if (CurrentSave == null)
        {
            CurrentSave = new GameSaveData();
        }

        if (CurrentSave.coins.bronze == zeroValue && CurrentSave.coins.silver == zeroValue && CurrentSave.coins.gold == zeroValue)
        {
            CurrentSave.coins = new CoinData(initialQuantityBronzeCoins, initialQuantitySilverCoins, initialQuantityGoldCoins);
        }
    }

    private void UpdatePlayerData()
    {
        int defaultArmor = 0;

        Hero player = FindObjectOfType<Hero>();

        if (player != null)
        {
            CurrentSave.playerHealth = player.Lives;

            ArmorManager armorManager = player.GetComponent<ArmorManager>();

            CurrentSave.playerArmor = armorManager?.CurrentArmor ?? defaultArmor;
        }
    }

    private void UpdateKeyCollectionData()
    {
        KeyCollection keyCollection = FindObjectOfType<KeyCollection>();

        if (keyCollection != null)
        {
            CurrentSave.collectedKeys = keyCollection.GetCollectedKeys();
        }
    }

    private void UpdateEnemyData()
    {
        if (EnemyManager.Instance != null)
        {
            HashSet<string> killedEnemies = EnemyManager.Instance.GetKilledEnemies();

            CurrentSave.killedEnemies = new List<string>(killedEnemies);
        }
        else
        {
            CurrentSave.killedEnemies = new List<string>();
        }
    }

    private void UpdateCoinData()
    {
        if (PersistentWallet.Instance != null)
        {
            CurrentSave.coins.bronze = PersistentWallet.Instance.CurrentCoins.bronze;
            CurrentSave.coins.silver = PersistentWallet.Instance.CurrentCoins.silver;
            CurrentSave.coins.gold = PersistentWallet.Instance.CurrentCoins.gold;
        }
        else if (WalletManager.Instance != null)
        {
            CurrentSave.coins.bronze = WalletManager.Instance.GetCoins(WalletManager.CoinType.Bronze);
            CurrentSave.coins.silver = WalletManager.Instance.GetCoins(WalletManager.CoinType.Silver);
            CurrentSave.coins.gold = WalletManager.Instance.GetCoins(WalletManager.CoinType.Gold);
        }
    }

    public void MarkEnemyKilled(string enemyId)
    {
        InitializeSaveDataIfNeeded();
        InitializeKilledEnemiesListIfNeeded();

        if (!CurrentSave.killedEnemies.Contains(enemyId))
        {
            CurrentSave.killedEnemies.Add(enemyId);
        }
    }

    public void MarkHealthPickupCollected(string pickupId)
    {
        InitializeSaveDataIfNeeded();
        InitializeHealthPickupsListIfNeeded();

        if (!CurrentSave.collectedHealthPickups.Contains(pickupId))
        {
            CurrentSave.collectedHealthPickups.Add(pickupId);
        }
    }

    public void MarkItemPurchased(string itemId)
    {
        InitializeSaveDataIfNeeded();

        if (CurrentSave.purchasedItemIds == null)
        {
            CurrentSave.purchasedItemIds = new List<string>();
        }

        if (!CurrentSave.purchasedItemIds.Contains(itemId))
        {
            CurrentSave.purchasedItemIds.Add(itemId);

            SaveToFile();
        }
    }

    public void UpdateAbilityData(AbilityManager abilityManager)
    {
        InitializeSaveDataIfNeeded();

        if (CurrentSave.abilityData == null)
        {
            CurrentSave.abilityData = new AbilitySaveData();
        }

        CurrentSave.abilityData.hasDash = abilityManager.HasDash;
        CurrentSave.abilityData.hasDoubleJump = abilityManager.HasDoubleJump;
        CurrentSave.abilityData.hasMap = abilityManager.HasMap;
        CurrentSave.abilityData.hasAnatomy = abilityManager.HasAnatomy;
        CurrentSave.abilityData.hasArmor = abilityManager.HasArmor;
        CurrentSave.abilityData.hasSwampDamageBonus = abilityManager.HasSwampDamageBonus;
        CurrentSave.abilityData.hasSkeletonDamageBonus = abilityManager.HasSkeletonDamageBonus;
        CurrentSave.abilityData.hasDemonDamageBonus = abilityManager.HasDemonDamageBonus;
        CurrentSave.abilityData.hasSpiderDamageBonus = abilityManager.HasSpiderDamageBonus;
        CurrentSave.abilityData.hasZombieDamageBonus = abilityManager.HasZombieDamageBonus;
        CurrentSave.abilityData.hasBossDamageBonus = abilityManager.HasBossDamageBonus;
        CurrentSave.abilityData.isLastChanceActive = abilityManager.IsLastChanceActive;
        CurrentSave.abilityData.hasPassiveHealthRegeneration = abilityManager.HasPassiveHealthRegeneration;
        CurrentSave.abilityData.hasRobocopRegeneration = abilityManager.HasRobocopRegeneration;
        CurrentSave.abilityData.hasVampireAbility = abilityManager.HasVampireAbility;
        CurrentSave.abilityData.hasOnePunchManAbility = abilityManager.HasOnePunchManAbility;

        SaveToFile();
    }

    private void InitializeKilledEnemiesListIfNeeded()
    {
        if (CurrentSave.killedEnemies == null)
        {
            CurrentSave.killedEnemies = new List<string>();
        }
    }

    private void InitializeHealthPickupsListIfNeeded()
    {
        if (CurrentSave.collectedHealthPickups == null)
        {
            CurrentSave.collectedHealthPickups = new List<string>();
        }
    }

    private void SaveToFile()
    {
        if (CurrentSave == null)
        {
            return;
        }

        string saveDirectoryPath = Application.persistentDataPath + SaveDirectory;

        CreateDirectoryIfNotExists(saveDirectoryPath);

        string jsonData = JsonUtility.ToJson(CurrentSave, true);

        File.WriteAllText(saveDirectoryPath + SaveFileName, jsonData);
    }

    private void CreateDirectoryIfNotExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    public void LoadGameData()
    {
        string filePath = Application.persistentDataPath + SaveDirectory + SaveFileName;

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);

            CurrentSave = JsonUtility.FromJson<GameSaveData>(jsonData);

            FixNullValues();
        }
        else
        {
            CurrentSave = null;
        }

        OnGameLoaded?.Invoke(CurrentSave);
    }

    private void FixNullValues()
    {
         int minimumCoinValue = 0;
         int minimumArmorValue = 0;

        if (CurrentSave == null)
        {
            return;
        }

        if (CurrentSave.coins.bronze < minimumCoinValue)
        {
            CurrentSave.coins.bronze = minimumCoinValue;
        }

        if (CurrentSave.coins.silver < minimumCoinValue)
        {
            CurrentSave.coins.silver = minimumCoinValue;
        }
        if (CurrentSave.coins.gold < minimumCoinValue)
        {
            CurrentSave.coins.gold = minimumCoinValue;
        }

        CurrentSave.collectedKeys ??= new List<KeyColor>();

        CurrentSave.killedEnemies ??= new List<string>();
        CurrentSave.collectedHealthPickups ??= new List<string>();
        CurrentSave.purchasedItemIds ??= new List<string>();
        CurrentSave.abilityData ??= new AbilitySaveData();

        if (CurrentSave.playerArmor < minimumArmorValue)
        {
            CurrentSave.playerArmor = minimumArmorValue;
        }
    }

    public void DeleteSaveData()
    {
        string filePath = Application.persistentDataPath + SaveDirectory + SaveFileName;

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        CurrentSave = null;
    }

    public bool HasSave()
    {
        string filePath = Application.persistentDataPath + SaveDirectory + SaveFileName;

        return File.Exists(filePath);
    }
}
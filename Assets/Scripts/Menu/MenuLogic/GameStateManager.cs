using DoorControl;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameStateManager : MonoBehaviour
{
    private const string ChestOpenedPrefix = "Chest_";
    private const string ChestOpenedSuffix = "_Opened";
    private const string KeySpawnedSuffix = "_KeySpawned";
    private const string KeyPrefix = "Key_";
    private const string GameSavedKey = "GameSaved";
    private const int TrueValue = 1;
    private const int FalseValue = 0;
    private const int MaxChestCount = 100;

    public static GameStateManager Instance { get; private set; }

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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static bool IsChestOpened(string chestId)
    {
        string key = GetChestOpenedKey(chestId);

        return PlayerPrefs.GetInt(key, FalseValue) == TrueValue;
    }

    public static void SetChestOpened(string chestId, bool isOpened)
    {
        string key = GetChestOpenedKey(chestId);

        PlayerPrefs.SetInt(key, isOpened ? TrueValue : FalseValue);
        PlayerPrefs.Save();
    }

    public static bool IsKeySpawned(string chestId)
    {
        string key = GetKeySpawnedKey(chestId);

        return PlayerPrefs.GetInt(key, FalseValue) == TrueValue;
    }

    public static void SetKeySpawned(string chestId, bool isSpawned)
    {
        string key = GetKeySpawnedKey(chestId);

        PlayerPrefs.SetInt(key, isSpawned ? TrueValue : FalseValue);
        PlayerPrefs.Save();
    }

    public static void ResetGameState()
    {
        ClearAllKeyData();
        ClearAllChestData();

        PlayerPrefs.Save();
    }

    public static void MarkGameSaved()
    {
        PlayerPrefs.SetInt(GameSavedKey, TrueValue);
        PlayerPrefs.Save();
    }

    public static bool HasKey(KeyColor color)
    {
        string key = GetKeyColorKey(color);

        return PlayerPrefs.GetInt(key, FalseValue) == TrueValue;
    }

    public static void AddKey(KeyColor color)
    {
        string key = GetKeyColorKey(color);

        PlayerPrefs.SetInt(key, TrueValue);
        PlayerPrefs.Save();
    }

    public static void RemoveKey(KeyColor color)
    {
        string key = GetKeyColorKey(color);

        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }

    public static void DebugKeys()
    {
        foreach (KeyColor color in System.Enum.GetValues(typeof(KeyColor)))
        {
            bool hasKey = HasKey(color);
        }
    }

    private static string GetChestOpenedKey(string chestId)
    {
        return $"{ChestOpenedPrefix}{chestId}{ChestOpenedSuffix}";
    }

    private static string GetKeySpawnedKey(string chestId)
    {
        return $"{ChestOpenedPrefix}{chestId}{KeySpawnedSuffix}";
    }

    private static string GetKeyColorKey(KeyColor color)
    {
        return $"{KeyPrefix}{color}";
    }

    private static void ClearAllKeyData()
    {
        foreach (KeyColor color in System.Enum.GetValues(typeof(KeyColor)))
        {
            PlayerPrefs.DeleteKey(GetKeyColorKey(color));
        }
    }

    private static void ClearAllChestData()
    {
        for (int i = 0; i < MaxChestCount; i++)
        {
            PlayerPrefs.DeleteKey(GetChestOpenedKey(i.ToString()));
            PlayerPrefs.DeleteKey(GetKeySpawnedKey(i.ToString()));
        }

        PlayerPrefs.DeleteKey($"{ChestOpenedPrefix}{ChestOpenedSuffix}");
        PlayerPrefs.DeleteKey($"{ChestOpenedPrefix}{KeySpawnedSuffix}");
    }
}
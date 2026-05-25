using DoorControl;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public string saveId;

    public DateTime saveTime;
    public Vector3 playerPosition;

    public int playerHealth;
    public int playerArmor;

    public List<KeyColor> collectedKeys = new List<KeyColor>();
    public List<string> killedEnemies = new List<string>();
    public List<string> collectedHealthPickups = new List<string>();

    public string checkpointId;
    public string sceneName;

    public CoinData coins;

    public List<string> purchasedItemIds = new List<string>();
    public AbilitySaveData abilityData = new AbilitySaveData();

    public GameSaveData()
    {
        coins = new CoinData(0, 0, 0);

        collectedKeys = new List<KeyColor>();

        killedEnemies = new List<string>();
        collectedHealthPickups = new List<string>();

        playerArmor = 0;

        purchasedItemIds = new List<string>();
        abilityData = new AbilitySaveData();
    }
}
using DoorControl;
using Player.Abilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class LevelLoader : MonoBehaviour
{
    private const string CommandUnlockMap = "1";
    private const string CommandUnlockDash = "2";
    private const string CommandUnlockAnatomy = "3";
    private const string CommandUnlockArmor = "4";
    private const string CommandUnlockSwampDamageBonus = "5";
    private const string CommandUnlockSkeletonDamageBonus = "8";
    private const string CommandUnlockDemonDamageBonus = "9";
    private const string CommandUnlockSpiderDamageBonus = "10";
    private const string CommandUnlockZombieDamageBonus = "11";
    private const string CommandUnlockPassiveHealthRegeneration = "12";
    private const string CommandUnlockRobocopRegeneration = "13";
    private const string CommandUnlockVampireAbility = "14";
    private const string CommandUnlockOnePunchManAbility = "15";
    private const string CommandUnlockBossDamageBonus = "16";

    private const float LoadDelay = 0.1f;
    private const float WaitAfterApply = 0.2f;

    private void Start()
    {
        StartCoroutine(LoadSavedGameData());
    }

    private IEnumerator LoadSavedGameData()
    {
        yield return new WaitForSeconds(LoadDelay);

        if (!CanLoadGame())
        {
            yield break;
        }

        GameSaveData saveData = SaveSystem.Instance.CurrentSave;

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName != saveData.sceneName)
        {
            yield break;
        }

        yield return StartCoroutine(ApplySavedGameData(saveData));
    }

    private bool CanLoadGame()
    {
        return SaveSystem.Instance != null &&
               SaveSystem.Instance.HasSave() &&
               SaveSystem.Instance.CurrentSave != null;
    }

    private IEnumerator ApplySavedGameData(GameSaveData saveData)
    {
        Hero player = FindObjectOfType<Hero>();

        if (player == null)
        {
            yield break;
        }

        Vector3 spawnPosition = CalculateSpawnPosition(saveData);

        ApplyPlayerData(player, spawnPosition, saveData);
        ApplyAbilityData(player, saveData);

        yield return new WaitForSeconds(0.1f);

        RestoreShopPurchases(saveData);

        yield return new WaitForSeconds(WaitAfterApply);

        ApplyGameWorldData(saveData);
        ApplyCoinData(saveData);

        SaveSystem.Instance?.UpdateAbilityData(player.AbilityManager);
    }

    private void RestoreShopPurchases(GameSaveData saveData)
    {
        if (saveData?.purchasedItemIds == null)
        {
            return;
        }

        var shopManager = FindObjectOfType<ShopManager>();

        if (shopManager != null)
        {
            var shopItemsField = typeof(ShopManager).GetField("_shopItems",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (shopItemsField != null)
            {
                var shopItems = shopItemsField.GetValue(shopManager) as List<ShopItemData>;

                if (shopItems != null)
                {
                    foreach (var itemId in saveData.purchasedItemIds)
                    {
                        var item = shopItems.Find(i => i.ItemId == itemId);

                        if (item != null && itemId != "6" && itemId != "7")
                        {
                            item.IsSold = true;
                        }
                    }
                }
            }
        }

        var hero = FindObjectOfType<Hero>();

        if (hero != null && hero.AbilityManager != null)
        {
            foreach (var itemId in saveData.purchasedItemIds)
            {
                ApplyAbilityEffect(itemId, hero.AbilityManager, hero);
            }
        }
    }

    private void ApplyAbilityEffect(string itemId, AbilityManager abilityManager, Hero hero)
    {
        if (abilityManager == null)
        {
            return;
        }

        switch (itemId)
        {
            case CommandUnlockMap: 
                if (!abilityManager.HasMap)
                {
                    abilityManager.UnlockMap(); 
                }

                break;

            case CommandUnlockDash:
                if (!abilityManager.HasDash)
                {
                    abilityManager.UnlockDash();
                }

                break;

            case CommandUnlockAnatomy: 
                if (!abilityManager.HasAnatomy)
                {
                    abilityManager.UnlockAnatomy(); 
                }

                break;

            case CommandUnlockArmor:
                if (!abilityManager.HasArmor)
                {
                    abilityManager.UnlockArmor();
                }

                var armorManager = hero.GetComponent<ArmorManager>();

                armorManager?.UnlockArmorAbility();

                break;

            case CommandUnlockSwampDamageBonus: 
                if (!abilityManager.HasSwampDamageBonus)
                {
                    abilityManager.UnlockSwampDamageBonus();
                }

                break;

            case CommandUnlockSkeletonDamageBonus: 
                if (!abilityManager.HasSkeletonDamageBonus)
                {
                    abilityManager.UnlockSkeletonDamageBonus(); 
                }

                break;

            case CommandUnlockDemonDamageBonus: 
                if (!abilityManager.HasDemonDamageBonus)
                {
                    abilityManager.UnlockDemonDamageBonus(); 
                }

                break;

            case CommandUnlockSpiderDamageBonus:
                if (!abilityManager.HasSpiderDamageBonus)
                {
                    abilityManager.UnlockSpiderDamageBonus();
                }

                break;

            case CommandUnlockZombieDamageBonus: 
                if (!abilityManager.HasZombieDamageBonus)
                {
                    abilityManager.UnlockZombieDamageBonus(); 
                }

                break;

            case CommandUnlockPassiveHealthRegeneration: 
                if (!abilityManager.HasPassiveHealthRegeneration)
                {
                    abilityManager.UnlockPassiveHealthRegeneration();
                }

                break;

            case CommandUnlockRobocopRegeneration: 
                if (!abilityManager.HasRobocopRegeneration)
                {
                    abilityManager.UnlockRobocopRegeneration();
                }

                break;

            case CommandUnlockVampireAbility:
                if (!abilityManager.HasVampireAbility)
                {
                    abilityManager.UnlockVampireAbility();
                }

                break;

            case CommandUnlockOnePunchManAbility: 
                if (!abilityManager.HasOnePunchManAbility)
                {
                    abilityManager.UnlockOnePunchManAbility(); 
                }

                break;

            case CommandUnlockBossDamageBonus: 
                if (!abilityManager.HasBossDamageBonus)
                {
                    abilityManager.UnlockBossDamageBonus();
                }

                break;
        }
    }

    private void ApplyAbilityData(Hero player, GameSaveData saveData)
    {
        if (player == null || saveData?.abilityData == null)
        {
            return;
        }

        if (saveData.abilityData.hasDash)
        {
            player.AbilityManager?.UnlockDash();
        }

        if (saveData.abilityData.hasAnatomy)
        {
            player.AbilityManager?.UnlockAnatomy();
        }

        if (saveData.abilityData.hasMap)
        {
            player.AbilityManager?.UnlockMap();
        }

        if (saveData.abilityData.hasArmor)
        {
            player.AbilityManager?.UnlockArmor();

            var armorManager = player.GetComponent<ArmorManager>();
            armorManager?.LoadArmorFromSave(saveData.playerArmor);
        }

        if (saveData.abilityData.hasSwampDamageBonus)
        {
            player.AbilityManager?.UnlockSwampDamageBonus();
        }

        if (saveData.abilityData.hasSkeletonDamageBonus)
        {
            player.AbilityManager?.UnlockSkeletonDamageBonus();
        }

        if (saveData.abilityData.hasDemonDamageBonus)
        {
            player.AbilityManager?.UnlockDemonDamageBonus();
        }

        if (saveData.abilityData.hasSpiderDamageBonus)
        {
            player.AbilityManager?.UnlockSpiderDamageBonus();
        }

        if (saveData.abilityData.hasZombieDamageBonus)
        {
            player.AbilityManager?.UnlockZombieDamageBonus();
        }

        if (saveData.abilityData.hasBossDamageBonus)
        {
            player.AbilityManager?.UnlockBossDamageBonus();
        }

        if (saveData.abilityData.hasPassiveHealthRegeneration)
        {
            player.AbilityManager?.UnlockPassiveHealthRegeneration();
        }

        if (saveData.abilityData.hasRobocopRegeneration)
        {
            player.AbilityManager?.UnlockRobocopRegeneration();
        }

        if (saveData.abilityData.hasVampireAbility)
        {
            player.AbilityManager?.UnlockVampireAbility();
        }

        if (saveData.abilityData.hasOnePunchManAbility)
        {
            player.AbilityManager?.UnlockOnePunchManAbility();
        }

        if (saveData.abilityData.isLastChanceActive)
        {
            player.AbilityManager?.PurchaseLastChance();
        }
    }

    private void ApplyCoinData(GameSaveData saveData)
    {
        if (saveData == null || !saveData.coins.isInitialized)
        {
            return;
        }

        if (PersistentWallet.Instance != null)
        {
            PersistentWallet.Instance.LoadCoinsFromSave(saveData.coins);
        }
    }

    private Vector3 CalculateSpawnPosition(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return Vector3.zero;
        }

        if (!string.IsNullOrEmpty(saveData.checkpointId))
        {
            Vector3 checkpointPosition = FindCheckpointPosition(saveData.checkpointId);

            if (checkpointPosition != Vector3.zero)
            {
                return checkpointPosition;
            }
        }

        return saveData.playerPosition;
    }

    private void ApplyPlayerData(Hero player, Vector3 spawnPosition, GameSaveData saveData)
    {
        int deathTreshold = 0;

        if (saveData == null)
        {
            return;
        }

        if (spawnPosition != Vector3.zero)
        {
            player.transform.position = spawnPosition;
        }

        if (saveData.playerHealth > deathTreshold)
        {
            player.SetHealth(saveData.playerHealth);
        }

        ApplyArmorData(player, saveData);
    }

    private void ApplyArmorData(Hero player, GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        ArmorManager armorManager = player.GetComponent<ArmorManager>();

        if (armorManager != null)
        {
            armorManager.LoadArmorFromSave(saveData.playerArmor);
        }
    }

    private void ApplyGameWorldData(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        KeyCollection keyCollection = FindObjectOfType<KeyCollection>();

        if (keyCollection != null && saveData.collectedKeys != null)
        {
            keyCollection.LoadCollectedKeys(saveData.collectedKeys);
        }

        EnemyManager.Instance?.SyncWithSaveData();
    }

    private Vector3 FindCheckpointPosition(string checkpointId)
    {
        if (string.IsNullOrEmpty(checkpointId))
        {
            return Vector3.zero;
        }

        Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>();

        foreach (Checkpoint checkpoint in allCheckpoints)
        {
            if (checkpoint.GetCheckpointId() == checkpointId)
            {
                return checkpoint.GetSpawnPosition();
            }
        }

        return Vector3.zero;
    }
}
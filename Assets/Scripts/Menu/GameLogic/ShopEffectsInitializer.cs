using GameLogic;
using Player.Abilities;
using UnityEngine;

public sealed class ShopEffectsInitializer : MonoBehaviour
{
    [SerializeField] private ShopManager _shopManager;

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

    private void Start()
    {
        InitializeShopEffects();
    }

    public void InitializeShopEffects()
    {
        if (_shopManager == null)
        {
            _shopManager = FindObjectOfType<ShopManager>();

            if (_shopManager == null)
            {
                return;
            }
        }

        var hero = FindObjectOfType<Hero>();

        if (hero == null || hero.AbilityManager == null)
        {
            return;
        }

        var shopItemsField = typeof(ShopManager).GetField("_shopItems",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (shopItemsField == null)
        {
            return;
        }

        var shopItems = shopItemsField.GetValue(_shopManager) as System.Collections.Generic.List<ShopItemData>;

        if (shopItems == null)
        {
            return;
        }

        foreach (var item in shopItems)
        {
            if (item == null)
            {
                continue;
            }

            if (item.IsSold && item.ItemId != "6" && item.ItemId != "7")
            {
                ApplyItemEffect(item.ItemId, hero.AbilityManager, hero);
            }
        }
    }

    private void ApplyItemEffect(string itemId, AbilityManager abilityManager, Hero hero)
    {
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

                    var armorManager = hero.GetComponent<ArmorManager>();

                    armorManager?.UnlockArmorAbility();
                }

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
}
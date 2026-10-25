using GameLogic;
using Player.Abilities;
using UnityEngine;

[System.Serializable]
public sealed class ShopItemData : IShopItem
{
    [SerializeField] private string _itemId;
    [SerializeField] private string _itemName;
    [SerializeField] private string _description;
    [SerializeField] private int _price;
    [SerializeField] private WalletManager.CoinType _currencyType;
    [SerializeField] private bool _isSold;

    public string ItemId => _itemId;
    public string DisplayName => _itemName;
    public string Description => _description;
    public int Price => _price;
    public WalletManager.CoinType CurrencyType => _currencyType;

    public bool IsSold
    {
        get
        {
            if (_itemId == ShopItemIds.RestoreArmor)
            {
                return false;
            }

            AbilityManager abilityManager = FindAbilityManager();

            if (abilityManager != null)
            {
                return IsAbilityUnlocked(abilityManager);
            }

            if (SaveSystem.Instance != null && SaveSystem.Instance.CurrentSave?.purchasedItemIds != null)
            {
                return SaveSystem.Instance.CurrentSave.purchasedItemIds.Contains(_itemId);
            }

            return _isSold;
        }
        set
        {
            if (_itemId == ShopItemIds.RestoreArmor || _itemId == ShopItemIds.ActivateLastChance)
            {
                return;
            }

            _isSold = value;

            if (value)
            {
                SaveSystem.Instance?.MarkItemPurchased(_itemId);
            }
        }
    }

    public bool CanBePurchased()
    {
        if (WalletManager.Instance == null)
        {
            return false;
        }

        if (WalletManager.Instance.GetCoins(_currencyType) < _price)
        {
            return false;
        }

        if (_itemId == ShopItemIds.RestoreArmor)
        {
            ArmorManager armorManager = FindArmorManager();

            return armorManager != null &&
                   armorManager.IsArmorUnlocked() &&
                   armorManager.CurrentArmor < armorManager.MaxArmor;
        }

        if (_itemId == ShopItemIds.ActivateLastChance)
        {
            AbilityManager abilityManager = FindAbilityManager();

            return abilityManager != null && abilityManager.IsLastChanceActive == false;
        }

        return IsSold == false;
    }

    public void Purchase()
    {
        if (_itemId == ShopItemIds.RestoreArmor)
        {
            IncrementArmorPlatesUsage();

            return;
        }

        IsSold = true;

        SavePurchaseState();
    }

    private bool IsAbilityUnlocked(AbilityManager abilityManager)
    {
        return _itemId switch
        {
            ShopItemIds.UnlockMap => abilityManager.HasMap,
            ShopItemIds.UnlockDash => abilityManager.HasDash,
            ShopItemIds.UnlockAnatomy => abilityManager.HasAnatomy,
            ShopItemIds.UnlockArmor => abilityManager.HasArmor,
            ShopItemIds.UnlockSwampDamageBonus => abilityManager.HasSwampDamageBonus,
            ShopItemIds.ActivateLastChance => abilityManager.IsLastChanceActive,
            ShopItemIds.UnlockSkeletonDamageBonus => abilityManager.HasSkeletonDamageBonus,
            ShopItemIds.UnlockDemonDamageBonus => abilityManager.HasDemonDamageBonus,
            ShopItemIds.UnlockSpiderDamageBonus => abilityManager.HasSpiderDamageBonus,
            ShopItemIds.UnlockPassiveHealthRegeneration => abilityManager.HasPassiveHealthRegeneration,
            ShopItemIds.UnlockRobocopRegeneration => abilityManager.HasRobocopRegeneration,
            ShopItemIds.UnlockVampireAbility => abilityManager.HasVampireAbility,
            ShopItemIds.UnlockOnePunchManAbility => abilityManager.HasOnePunchManAbility,
            ShopItemIds.UnlockBossDamageBonus => abilityManager.HasBossDamageBonus,
            _ => _isSold
        };
    }

    private AbilityManager FindAbilityManager()
    {
        Hero hero = Object.FindFirstObjectByType<Hero>();

        return hero != null ? hero.AbilityManager : null;
    }

    private ArmorManager FindArmorManager()
    {
        Hero hero = Object.FindFirstObjectByType<Hero>();

        if (hero != null)
        {
            ArmorManager heroArmorManager = hero.GetComponent<ArmorManager>();

            if (heroArmorManager != null)
            {
                return heroArmorManager;
            }
        }

        return Object.FindFirstObjectByType<ArmorManager>();
    }

    private void SavePurchaseState()
    {
        const int purchasedValue = 1;

        PlayerPrefs.SetInt($"Item_{_itemId}_Purchased", purchasedValue);
        PlayerPrefs.Save();
    }

    private void IncrementArmorPlatesUsage()
    {
        const string armorUsesKey = "ArmorPlatesUsed";

        int uses = PlayerPrefs.GetInt(armorUsesKey, 0);

        PlayerPrefs.SetInt(armorUsesKey, uses + 1);
        PlayerPrefs.Save();
    }
}
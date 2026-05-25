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

    private const string CommandUnlockMap = "1";
    private const string CommandUnlockDash = "2";
    private const string CommandUnlockAnatomy = "3";
    private const string CommandUnlockArmor = "4";
    private const string CommandUnlockSwampDamageBonus = "5";
    private const string CommandActiveLastChance = "6";
    private const string CommandUnlockedArmor = "7";
    private const string CommandUnlockSkeletonDamageBonus = "8";
    private const string CommandUnlockDemonDamageBonus = "9";
    private const string CommandUnlockSpiderDamageBonus = "10";
    private const string CommandUnlockZombieDamageBonus = "11";
    private const string CommandUnlockPassiveHealthRegeneration = "12";
    private const string CommandUnlockRobocopRegeneration = "13";
    private const string CommandUnlockVampireAbility = "14";
    private const string CommandUnlockOnePunchManAbility = "15";
    private const string CommandUnlockBossDamageBonus = "16";

    public string ItemId => _itemId;
    public string DisplayName => _itemName;
    public string Description => _description;
    public int Price => _price;
    public WalletManager.CoinType CurrencyType => _currencyType;

    public bool IsSold
    {
        get
        {
            if (_itemId == CommandUnlockedArmor)
            {
                return false;
            }

            if (_itemId == CommandActiveLastChance)
            {
                return GetLastChanceState();
            }

            var abilityManager = FindAbilityManager();

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
            if (_itemId != CommandUnlockedArmor && _itemId != CommandActiveLastChance)
            {
                _isSold = value;

                if (value && SaveSystem.Instance != null)
                {
                    SaveSystem.Instance.MarkItemPurchased(_itemId);
                }
            }
        }
    }

    private bool IsAbilityUnlocked(AbilityManager abilityManager)
    {
        return _itemId switch
        {
            CommandUnlockMap => abilityManager.HasMap,
            CommandUnlockDash => abilityManager.HasDash,
            CommandUnlockAnatomy => abilityManager.HasAnatomy,
            CommandUnlockArmor => abilityManager.HasArmor,
            CommandUnlockSwampDamageBonus => abilityManager.HasSwampDamageBonus,
            CommandActiveLastChance => abilityManager.IsLastChanceActive,
            CommandUnlockSkeletonDamageBonus => abilityManager.HasSkeletonDamageBonus,
            CommandUnlockDemonDamageBonus => abilityManager.HasDemonDamageBonus,
            CommandUnlockSpiderDamageBonus => abilityManager.HasSpiderDamageBonus,
            CommandUnlockZombieDamageBonus => abilityManager.HasZombieDamageBonus,
            CommandUnlockPassiveHealthRegeneration => abilityManager.HasPassiveHealthRegeneration,
            CommandUnlockRobocopRegeneration => abilityManager.HasRobocopRegeneration,
            CommandUnlockVampireAbility => abilityManager.HasVampireAbility,
            CommandUnlockOnePunchManAbility => abilityManager.HasOnePunchManAbility,
            CommandUnlockBossDamageBonus => abilityManager.HasBossDamageBonus
        };
    }

    private bool GetLastChanceState()
    {
        string lastChanceKey = "LastChanceActive";
        int activeValue = 1;
        int inactiveValue = 0;

        if (_itemId != CommandActiveLastChance)
        {
            return _isSold;
        }

        var abilityManager = FindAbilityManager();

        if (abilityManager != null)
        {
            return abilityManager.IsLastChanceActive;
        }

        if (SaveSystem.Instance != null && SaveSystem.Instance.CurrentSave?.abilityData != null)
        {
            return SaveSystem.Instance.CurrentSave.abilityData.isLastChanceActive;
        }

        return PlayerPrefs.GetInt(lastChanceKey, inactiveValue) == activeValue;
    }

    private AbilityManager FindAbilityManager()
    {
        var hero = GameObject.FindObjectOfType<Hero>();

        if (hero != null)
        {
            return hero.AbilityManager;
        }

        return null;
    }

    public bool CanBePurchased()
    {
        if (_itemId == CommandUnlockedArmor)
        {
            var armorManager = FindArmorManager();

            if (armorManager == null)
            {
                return false;
            }

            bool canBuyArmorPlates = armorManager.IsArmorUnlocked() && armorManager.CurrentArmor < armorManager.MaxArmor;

            bool hasEnoughMoney = WalletManager.Instance != null && WalletManager.Instance.GetCoins(CurrencyType) >= Price;

            return canBuyArmorPlates && hasEnoughMoney;
        }

        if (_itemId == CommandActiveLastChance)
        {
            var abilityManager = FindAbilityManager();

            if (abilityManager == null)
            {
                return false;
            }

            bool canBuyLastChance = !abilityManager.IsLastChanceActive;

            bool hasEnoughCoins = WalletManager.Instance != null && WalletManager.Instance.GetCoins(CurrencyType) >= Price;

            return canBuyLastChance && hasEnoughCoins;
        }

        if (IsSold)
        {
            return false;
        }

        if (!WalletManager.Instance)
        {
            return false;
        }

        bool canAfford = WalletManager.Instance.GetCoins(CurrencyType) >= Price;

        return canAfford;
    }

    private ArmorManager FindArmorManager()
    {
        var hero = GameObject.FindObjectOfType<Hero>();

        if (hero != null)
        {
            var armorManager = hero.GetComponent<ArmorManager>();

            if (armorManager != null)
            {
                return armorManager;
            }
        }

        return GameObject.FindObjectOfType<ArmorManager>();
    }

    public void Purchase()
    {
        string armorUsesKey = "ArmorPlatesUsed";
        int incrementByOne = 1;
        int initialArmorIndex = 0;

        if (_itemId != CommandUnlockedArmor)
        {
            IsSold = true;

            SavePurchaseState();
        }
        else
        {
            int uses = PlayerPrefs.GetInt(armorUsesKey, initialArmorIndex);

            PlayerPrefs.SetInt(armorUsesKey, uses + incrementByOne);
            PlayerPrefs.Save();
        }
    }

    private void SavePurchaseState()
    {
        int purchasedValue = 1;

        if (_itemId != CommandUnlockedArmor)
        {
            PlayerPrefs.SetInt($"Item_{ItemId}_Purchased", purchasedValue);
            PlayerPrefs.Save();
        }
    }
}
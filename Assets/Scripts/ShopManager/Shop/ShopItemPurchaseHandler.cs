using GameLogic;
using Player.Abilities;
using System.Collections.Generic;
using UnityEngine;

public sealed class ShopItemPurchaseHandler
{
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

    private readonly Hero _hero;
    private readonly AbilityManager _abilityManager;
    private ArmorManager _armorManager;

    public ShopItemPurchaseHandler(Hero hero = null, ArmorManager armorManager = null)
    {
        _hero = hero;

        if (_hero != null)
        {
            _abilityManager = _hero.AbilityManager;
            _armorManager = armorManager ?? _hero.GetComponent<ArmorManager>();
        }
        else
        {
            _armorManager = armorManager;
        }
    }

    public bool TryPurchaseItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return false;
        }

        if (!ValidateItemPurchase(itemId))
        {
            return false;
        }

        if (!ProcessPayment(itemId))
        {
            return false;
        }

        ApplyItemEffect(itemId);
        SavePurchase(itemId);

        SaveGameAfterPurchase(itemId);

        return true;
    }

    private bool ValidateItemPurchase(string itemId)
    {
        if (_abilityManager == null)
        {
            return false;
        }

        switch (itemId)
        {
            case CommandUnlockMap:

                return !_abilityManager.HasMap;

            case CommandUnlockDash:

                return !_abilityManager.HasDash;

            case CommandUnlockAnatomy:

                return !_abilityManager.HasAnatomy;

            case CommandUnlockArmor:

                return !_abilityManager.HasArmor;

            case CommandUnlockSwampDamageBonus:

                return !_abilityManager.HasSwampDamageBonus;

            case CommandActiveLastChance:

                return !_abilityManager.IsLastChanceActive;

            case CommandUnlockedArmor:
                if (_armorManager == null)
                {
                    _armorManager = GetArmorManager();
                }

                if (_armorManager == null)
                {
                    return false;
                }

                bool isUnlocked = _armorManager.IsArmorUnlocked();
                bool isNotFull = _armorManager.CurrentArmor < _armorManager.MaxArmor;

                return isUnlocked && isNotFull;

            case CommandUnlockSkeletonDamageBonus:

                return !_abilityManager.HasSkeletonDamageBonus;

            case CommandUnlockDemonDamageBonus:

                return !_abilityManager.HasDemonDamageBonus;

            case CommandUnlockSpiderDamageBonus:

                return !_abilityManager.HasSpiderDamageBonus;

            case CommandUnlockZombieDamageBonus:

                return !_abilityManager.HasZombieDamageBonus;

            case CommandUnlockPassiveHealthRegeneration:

                return !_abilityManager.HasPassiveHealthRegeneration;

            case CommandUnlockRobocopRegeneration:

                return _abilityManager.HasArmor && !_abilityManager.HasRobocopRegeneration;

            case CommandUnlockVampireAbility:

                return !_abilityManager.HasVampireAbility;

            case CommandUnlockOnePunchManAbility:

                return !_abilityManager.HasOnePunchManAbility;

            case CommandUnlockBossDamageBonus:

                return !_abilityManager.HasBossDamageBonus;

            default:

                return false;
        }
    }

    private ArmorManager GetArmorManager()
    {
        if (_hero != null)
        {
            var armorManager = _hero.GetComponent<ArmorManager>();

            if (armorManager != null)
            {
                return armorManager;
            }
        }

        return null;
    }

    private bool ProcessPayment(string itemId)
    {
        if (WalletManager.Instance == null)
        {
            return false;
        }

        var shopItem = FindShopItem(itemId);

        if (shopItem == null)
        {
            return false;
        }

        int price = shopItem.Price;

        WalletManager.CoinType currency = shopItem.CurrencyType;

        int currentCoins = WalletManager.Instance.GetCoins(currency);

        if (currentCoins < price)
        {
            return false;
        }

        bool spentSuccessfully = WalletManager.Instance.TrySpendCoins(currency, price);

        if (spentSuccessfully)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private ShopItemData FindShopItem(string itemId)
    {
        var shopManager = GameObject.FindObjectOfType<ShopManager>();

        if (shopManager == null || shopManager.ShopItems == null)
        {
            return null;
        }

        var shopItemsField = typeof(ShopManager).GetField("_shopItems",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (shopItemsField != null)
        {
            List<ShopItemData> shopItems = shopItemsField.GetValue(shopManager) as List<ShopItemData>;

            if (shopItems != null)
            {
                return shopItems.Find(item => item.ItemId == itemId);
            }
        }

        return null;
    }

    private void ApplyItemEffect(string itemId)
    {
        if (_abilityManager == null)
        {
            return;
        }

        switch (itemId)
        {
            case CommandUnlockMap:
                _abilityManager.UnlockMap();

                break;

            case CommandUnlockDash:
                _abilityManager.UnlockDash();

                break;

            case CommandUnlockAnatomy:
                _abilityManager.UnlockAnatomy();

                break;

            case CommandUnlockArmor:
                _abilityManager.UnlockArmor();

                if (_armorManager != null)
                {
                    _armorManager.FillArmor();
                }

                break;

            case CommandUnlockSwampDamageBonus:
                _abilityManager.UnlockSwampDamageBonus();

                break;

            case CommandActiveLastChance:
                _abilityManager.PurchaseLastChance();

                break;

            case CommandUnlockedArmor:
                RestoreArmor();

                break;

            case CommandUnlockSkeletonDamageBonus:
                _abilityManager.UnlockSkeletonDamageBonus();

                break;

            case CommandUnlockDemonDamageBonus:
                _abilityManager.UnlockDemonDamageBonus();

                break;

            case CommandUnlockSpiderDamageBonus:
                _abilityManager.UnlockSpiderDamageBonus();

                break;

            case CommandUnlockZombieDamageBonus:
                _abilityManager.UnlockZombieDamageBonus();

                break;

            case CommandUnlockPassiveHealthRegeneration:
                _abilityManager.UnlockPassiveHealthRegeneration();

                break;

            case CommandUnlockRobocopRegeneration:
                _abilityManager.UnlockRobocopRegeneration();

                break;

            case CommandUnlockVampireAbility:
                _abilityManager.UnlockVampireAbility();

                break;

            case CommandUnlockOnePunchManAbility:
                _abilityManager.UnlockOnePunchManAbility();

                break;

            case CommandUnlockBossDamageBonus:
                _abilityManager.UnlockBossDamageBonus();

                break;
        }
    }

    private void RestoreArmor()
    {
        if (_armorManager == null)
        {
            _armorManager = GetArmorManager();
        }

        if (_armorManager == null)
        {
            return;
        }

        if (!_armorManager.IsArmorUnlocked())
        {
            return;
        }

        int oldArmor = _armorManager.CurrentArmor;

        _armorManager.FillArmor();

        int newArmor = _armorManager.CurrentArmor;


        ShowArmorRestoredEffect();
    }

    private void ShowArmorRestoredEffect()
    {
        float verticalOffset = 2f;
        string effectName = "ArmorRestoredEffect";
        string effectText = "броня восстановлена";
        int fontSize = 20;
        float destroyDelay = 2f;

        if (_hero == null)
        {
            return;
        }

        GameObject effect = new GameObject(effectName);

        effect.transform.position = _hero.transform.position + Vector3.up * verticalOffset;

        TextMesh textMesh = effect.AddComponent<TextMesh>();

        textMesh.text = effectText;

        textMesh.color = Color.cyan;
        textMesh.fontSize = fontSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.fontStyle = FontStyle.Bold;

        Object.Destroy(effect, destroyDelay);
    }

    private void SavePurchase(string itemId)
    {
        if (itemId == CommandActiveLastChance)
        {
            PlayerPrefs.SetInt("LastChance_Active", 1);
            PlayerPrefs.Save();

            SaveSystem.Instance?.UpdateAbilityData(_abilityManager);
        }
        else if (itemId != CommandUnlockedArmor)
        {
            SaveSystem.Instance?.MarkItemPurchased(itemId);
        }
        else
        {
            int uses = PlayerPrefs.GetInt("ArmorPlates_Used", 0);

            PlayerPrefs.SetInt("ArmorPlates_Used", uses + 1);

            PlayerPrefs.Save();
        }
    }

    private void SaveGameAfterPurchase(string itemId)
    {
        if (itemId == CommandUnlockArmor || itemId == CommandActiveLastChance || itemId == CommandUnlockRobocopRegeneration ||
            itemId == CommandUnlockVampireAbility || itemId == CommandUnlockOnePunchManAbility)
        {
            var saveSystem = SaveSystem.Instance;

            if (saveSystem != null && _hero != null)
            {
                saveSystem.UpdateAbilityData(_abilityManager);

                if (_hero != null)
                {
                    saveSystem.SaveGame("", _hero.transform.position);
                }
            }
        }
    }

    public string GetItemDescription(string itemId)
    {
        return itemId switch
        {
            CommandUnlockMap => "Карта - показывает всю карту игры",
            CommandUnlockDash => "Рывок - быстрое перемещение",
            CommandUnlockAnatomy => "Анатомия - позволяет подбирать аптечки",
            CommandUnlockArmor => "",
            CommandUnlockSwampDamageBonus => "И на болоте хорошо - +100% урона по болотным врагам",
            CommandActiveLastChance => "Последний шанс - выживание при смертельном ударе ( единоразово )",
            CommandUnlockedArmor => "Пластины брони - восстановление брони до максимума",
            CommandUnlockSkeletonDamageBonus => "Несвежее мясо - +100% урона по скелетам",
            CommandUnlockDemonDamageBonus => "Девять кругов - +100% урона по демонам",
            CommandUnlockSpiderDamageBonus => "Арахнофобия - +100% урона по паукам",
            CommandUnlockZombieDamageBonus => "По имени Шон - +100% урона по зомби",
            CommandUnlockPassiveHealthRegeneration => "Спасение утопающего - пассивное восстановление здоровья",
            CommandUnlockRobocopRegeneration => "Робокоп - пассивное восстановление брони",
            CommandUnlockVampireAbility => "Дракула - получение здоровья за убийство врагов",
            CommandUnlockOnePunchManAbility => "Ван Панч Мэн - шанс мгновенного убийства врага",
            CommandUnlockBossDamageBonus => "ГодСлэер - +100% урона по боссам",
        };
    }

    public string GetItemName(string itemId)
    {
        return itemId switch
        {
            CommandUnlockMap => "Карта",
            CommandUnlockDash => "Рывок",
            CommandUnlockAnatomy => "Анатомия",
            CommandUnlockArmor => "Броня",
            CommandUnlockSwampDamageBonus => "И на болоте хорошо",
            CommandActiveLastChance => "Последний шанс",
            CommandUnlockedArmor => "Пластины брони",
            CommandUnlockSkeletonDamageBonus => "Несвежее мясо",
            CommandUnlockDemonDamageBonus => "Девять кругов",
            CommandUnlockSpiderDamageBonus => "Арахнофобия",
            CommandUnlockZombieDamageBonus => "По имени Шон",
            CommandUnlockPassiveHealthRegeneration => "Спасение утопающего",
            CommandUnlockRobocopRegeneration => "Робокоп",
            CommandUnlockVampireAbility => "Дракула",
            CommandUnlockOnePunchManAbility => "Ван Панч Мэн",
            CommandUnlockBossDamageBonus => "ГодСлэер",
        };
    }
}
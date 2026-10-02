using GameLogic;
using Player.Abilities;
using UnityEngine;

public sealed class ShopItemPurchaseHandler
{
    private const float EffectVerticalOffset = 2f;
    private const float EffectDestroyDelay = 2f;
    private const int EffectFontSize = 20;

    private readonly Hero _hero;
    private readonly AbilityManager _abilityManager;

    private ArmorManager _armorManager;
    private ShopManager _shopManager;

    public ShopItemPurchaseHandler(Hero hero = null, ArmorManager armorManager = null, ShopManager shopManager = null)
    {
        _hero = hero;
        _abilityManager = _hero != null ? _hero.AbilityManager : null;
        _armorManager = armorManager;
        _shopManager = shopManager;

        if (_armorManager == null && _hero != null)
        {
            _armorManager = _hero.GetComponent<ArmorManager>();
        }
    }

    public bool TryPurchaseItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return false;
        }

        ShopItemData item = FindShopItem(itemId);

        if (item == null || item.CanBePurchased() == false)
        {
            return false;
        }

        if (ProcessPayment(item) == false)
        {
            return false;
        }

        ApplyItemEffect(itemId);
        SavePurchase(itemId);

        return true;
    }

    public string GetItemDescription(string itemId)
    {
        switch (LocalizationManager.CurrentLanguage)
        {
            case LocalizationManager.Language.English:
                return itemId switch
                {
                    ShopItemIds.UnlockMap => "prison map",
                    ShopItemIds.UnlockDash => "quick dash",
                    ShopItemIds.UnlockAnatomy => "allows picking up medkits",
                    ShopItemIds.UnlockArmor => "grants armor",
                    ShopItemIds.UnlockSwampDamageBonus => "+100% damage swamp monsters",
                    ShopItemIds.ActivateLastChance => "survive a fatal blow",
                    ShopItemIds.RestoreArmor => "restores armor",
                    ShopItemIds.UnlockSkeletonDamageBonus => "+100% damage to skeletons",
                    ShopItemIds.UnlockDemonDamageBonus => "+100% damage to demons",
                    ShopItemIds.UnlockSpiderDamageBonus => "+100% damage to spiders",
                    ShopItemIds.UnlockZombieDamageBonus => "+100% damage to zombies",
                    ShopItemIds.UnlockPassiveHealthRegeneration => "passive health regeneration",
                    ShopItemIds.UnlockRobocopRegeneration => "passive armor regeneration",
                    ShopItemIds.UnlockVampireAbility => "restores health on kill",
                    ShopItemIds.UnlockOnePunchManAbility => "chance of instant kill",
                    ShopItemIds.UnlockBossDamageBonus => "+100% damage to bosses",
                    _ => string.Empty
                };

            case LocalizationManager.Language.Turkish:
                return itemId switch
                {
                    ShopItemIds.UnlockMap => "hapishane haritası",
                    ShopItemIds.UnlockDash => "hızlı atılma",
                    ShopItemIds.UnlockAnatomy => "sağlık çantası almayı sağlar",
                    ShopItemIds.UnlockArmor => "zırh verir",
                    ShopItemIds.UnlockSwampDamageBonus => "bataklık canavarlarına +%100 hasar",
                    ShopItemIds.ActivateLastChance => "ölümcül darbeden sağ çık",
                    ShopItemIds.RestoreArmor => "zırhı yeniler",
                    ShopItemIds.UnlockSkeletonDamageBonus => "iskeletlere +%100 hasar",
                    ShopItemIds.UnlockDemonDamageBonus => "iblislere +%100 hasar",
                    ShopItemIds.UnlockSpiderDamageBonus => "örümceklere +%100 hasar",
                    ShopItemIds.UnlockZombieDamageBonus => "zombilere +%100 hasar",
                    ShopItemIds.UnlockPassiveHealthRegeneration => "pasif can yenilenmesi",
                    ShopItemIds.UnlockRobocopRegeneration => "pasif zırh yenilenmesi",
                    ShopItemIds.UnlockVampireAbility => "öldürme başına can yeniler",
                    ShopItemIds.UnlockOnePunchManAbility => "anında öldürme şansı",
                    ShopItemIds.UnlockBossDamageBonus => "boss'lara +%100 hasar",
                    _ => string.Empty
                };

            default: 
                return itemId switch
                {
                    ShopItemIds.UnlockMap => "карта тюрьмы",
                    ShopItemIds.UnlockDash => "быстрый подкат",
                    ShopItemIds.UnlockAnatomy => "позволяет подбирать аптечки",
                    ShopItemIds.UnlockArmor => "получение брони",
                    ShopItemIds.UnlockSwampDamageBonus => "+100% урона по болотным",
                    ShopItemIds.ActivateLastChance => "выживание при смертельном ударе",
                    ShopItemIds.RestoreArmor => "восстановление брони",
                    ShopItemIds.UnlockSkeletonDamageBonus => "+100% урона по скелетам",
                    ShopItemIds.UnlockDemonDamageBonus => "+100% урона по демонам",
                    ShopItemIds.UnlockSpiderDamageBonus => "+100% урона по паукам",
                    ShopItemIds.UnlockZombieDamageBonus => "+100% урона по зомби",
                    ShopItemIds.UnlockPassiveHealthRegeneration => "пассивное восстановление здоровья",
                    ShopItemIds.UnlockRobocopRegeneration => "пассивное восстановление брони",
                    ShopItemIds.UnlockVampireAbility => "получение здоровья за убийство",
                    ShopItemIds.UnlockOnePunchManAbility => "шанс мгновенного убийства",
                    ShopItemIds.UnlockBossDamageBonus => "+100% урона по боссам",
                    _ => string.Empty
                };
        }
    }

    public string GetItemName(string itemId)
    {
        switch (LocalizationManager.CurrentLanguage)
        {
            case LocalizationManager.Language.English:
                return itemId switch
                {
                    ShopItemIds.UnlockMap => "Map",
                    ShopItemIds.UnlockDash => "Dash",
                    ShopItemIds.UnlockAnatomy => "Anatomy",
                    ShopItemIds.UnlockArmor => "Armor",
                    ShopItemIds.UnlockSwampDamageBonus => "Dirty",
                    ShopItemIds.ActivateLastChance => "Last Dance",
                    ShopItemIds.RestoreArmor => "Plates",
                    ShopItemIds.UnlockSkeletonDamageBonus => "Rotten",
                    ShopItemIds.UnlockDemonDamageBonus => "Nine Circles",
                    ShopItemIds.UnlockSpiderDamageBonus => "Arachnohate",
                    ShopItemIds.UnlockZombieDamageBonus => "Walking Dad",
                    ShopItemIds.UnlockPassiveHealthRegeneration => "Time Heals",
                    ShopItemIds.UnlockRobocopRegeneration => "Robocop",
                    ShopItemIds.UnlockVampireAbility => "Dracula",
                    ShopItemIds.UnlockOnePunchManAbility => "One Punch",
                    ShopItemIds.UnlockBossDamageBonus => "Godslayer",
                    _ => string.Empty
                };

            case LocalizationManager.Language.Turkish:
                return itemId switch
                {
                    ShopItemIds.UnlockMap => "Harita",
                    ShopItemIds.UnlockDash => "Atılma",
                    ShopItemIds.UnlockAnatomy => "Anatomi",
                    ShopItemIds.UnlockArmor => "Zırh",
                    ShopItemIds.UnlockSwampDamageBonus => "Kirli",
                    ShopItemIds.ActivateLastChance => "Son Dans",
                    ShopItemIds.RestoreArmor => "Plakalar",
                    ShopItemIds.UnlockSkeletonDamageBonus => "Çürük",
                    ShopItemIds.UnlockDemonDamageBonus => "Dokuz Çember",
                    ShopItemIds.UnlockSpiderDamageBonus => "Araknofobi",
                    ShopItemIds.UnlockZombieDamageBonus => "Yürüyen Baba",
                    ShopItemIds.UnlockPassiveHealthRegeneration => "Zaman İyileştirir",
                    ShopItemIds.UnlockRobocopRegeneration => "Robokop",
                    ShopItemIds.UnlockVampireAbility => "Drakula",
                    ShopItemIds.UnlockOnePunchManAbility => "Tek Yumruk",
                    ShopItemIds.UnlockBossDamageBonus => "Tanrı Katili",
                    _ => string.Empty
                };

            default: 
                return itemId switch
                {
                    ShopItemIds.UnlockMap => "карта",
                    ShopItemIds.UnlockDash => "рывок",
                    ShopItemIds.UnlockAnatomy => "анатомия",
                    ShopItemIds.UnlockArmor => "броня",
                    ShopItemIds.UnlockSwampDamageBonus => "грязный",
                    ShopItemIds.ActivateLastChance => "ласт дэнс",
                    ShopItemIds.RestoreArmor => "пластины",
                    ShopItemIds.UnlockSkeletonDamageBonus => "гнилой",
                    ShopItemIds.UnlockDemonDamageBonus => "девять кругов",
                    ShopItemIds.UnlockSpiderDamageBonus => "арахнофобия",
                    ShopItemIds.UnlockZombieDamageBonus => "ходячий отец",
                    ShopItemIds.UnlockPassiveHealthRegeneration => "время лечит",
                    ShopItemIds.UnlockRobocopRegeneration => "робокоп",
                    ShopItemIds.UnlockVampireAbility => "дракула",
                    ShopItemIds.UnlockOnePunchManAbility => "ван панч мэн",
                    ShopItemIds.UnlockBossDamageBonus => "годСлэер",
                    _ => string.Empty
                };
        }
    }

    private bool ProcessPayment(ShopItemData item)
    {
        return WalletManager.Instance != null &&
               WalletManager.Instance.TrySpendCoins(item.CurrencyType, item.Price);
    }

    private ShopItemData FindShopItem(string itemId)
    {
        if (_shopManager == null)
        {
            _shopManager = Object.FindFirstObjectByType<ShopManager>();
        }

        if (_shopManager == null || _shopManager.ShopItems == null)
        {
            return null;
        }

        return _shopManager.ShopItems.Find(item => item.ItemId == itemId);
    }

    private void ApplyItemEffect(string itemId)
    {
        if (_abilityManager == null)
        {
            return;
        }

        switch (itemId)
        {
            case ShopItemIds.UnlockMap: _abilityManager.UnlockMap(); break;

            case ShopItemIds.UnlockDash: _abilityManager.UnlockDash(); break;

            case ShopItemIds.UnlockAnatomy:
                _abilityManager.UnlockAnatomy();
                RefreshAllPickups();

                if (_shopManager != null) _shopManager.Invoke(nameof(RefreshAllPickups), 0.2f);

                break;

            case ShopItemIds.UnlockArmor:
                _abilityManager.UnlockArmor();
                _armorManager?.FillArmor();

                break;

            case ShopItemIds.UnlockSwampDamageBonus: _abilityManager.UnlockSwampDamageBonus(); break;

            case ShopItemIds.ActivateLastChance: _abilityManager.PurchaseLastChance(); break;

            case ShopItemIds.RestoreArmor: RestoreArmor(); break;

            case ShopItemIds.UnlockSkeletonDamageBonus: _abilityManager.UnlockSkeletonDamageBonus(); break;

            case ShopItemIds.UnlockDemonDamageBonus: _abilityManager.UnlockDemonDamageBonus(); break;

            case ShopItemIds.UnlockSpiderDamageBonus: _abilityManager.UnlockSpiderDamageBonus(); break;

            case ShopItemIds.UnlockZombieDamageBonus: _abilityManager.UnlockZombieDamageBonus(); break;

            case ShopItemIds.UnlockPassiveHealthRegeneration: _abilityManager.UnlockPassiveHealthRegeneration(); break;

            case ShopItemIds.UnlockRobocopRegeneration: _abilityManager.UnlockRobocopRegeneration(); break;

            case ShopItemIds.UnlockVampireAbility: _abilityManager.UnlockVampireAbility(); break;

            case ShopItemIds.UnlockOnePunchManAbility: _abilityManager.UnlockOnePunchManAbility(); break;

            case ShopItemIds.UnlockBossDamageBonus: _abilityManager.UnlockBossDamageBonus(); break;
        }
    }

    private void RestoreArmor()
    {
        if (_armorManager == null && _hero != null)
        {
            _armorManager = _hero.GetComponent<ArmorManager>();
        }

        if (_armorManager == null || _armorManager.IsArmorUnlocked() == false)
        {
            return;
        }

        _armorManager.FillArmor();
        ShowArmorRestoredEffect();
    }

    private void RefreshAllPickups()
    {
        var pickups = Object.FindObjectsByType<HealthPickup>(FindObjectsSortMode.None);

        foreach (var pickup in pickups)
        {
            if (pickup != null)
            {
                pickup.RefreshPickupState();
            }
        }
    }

    private void SavePurchase(string itemId)
    {
        if (itemId == ShopItemIds.RestoreArmor)
        {
            int uses = PlayerPrefs.GetInt("ArmorPlates_Used", 0);

            PlayerPrefs.SetInt("ArmorPlates_Used", uses + 1);
            PlayerPrefs.Save();

            return;
        }

        if (itemId != ShopItemIds.ActivateLastChance)
        {
            SaveSystem.Instance?.MarkItemPurchased(itemId);
        }

        if (_abilityManager != null)
        {
            SaveSystem.Instance?.UpdateAbilityData(_abilityManager);
            ShopSaveManager.Instance?.OnItemPurchased(itemId, _abilityManager);
        }

        SaveGameAfterPurchase();
    }

    private void SaveGameAfterPurchase()
    {
        if (SaveSystem.Instance == null || _hero == null)
        {
            return;
        }

        SaveSystem.Instance.SaveGame(string.Empty, _hero.transform.position);
    }

    private void ShowArmorRestoredEffect()
    {
        if (_hero == null)
        {
            return;
        }

        GameObject effect = new GameObject("ArmorRestoredEffect");
        effect.transform.position = _hero.transform.position + Vector3.up * EffectVerticalOffset;

        TextMesh textMesh = effect.AddComponent<TextMesh>();

        textMesh.text = LocalizationManager.CurrentLanguage switch
        {
            LocalizationManager.Language.English => "armor restored",
            LocalizationManager.Language.Turkish => "zırh yenilendi",
            _ => "броня восстановлена"
        };

        textMesh.color = Color.cyan;
        textMesh.fontSize = EffectFontSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.fontStyle = FontStyle.Bold;

        Object.Destroy(effect, EffectDestroyDelay);
    }
}
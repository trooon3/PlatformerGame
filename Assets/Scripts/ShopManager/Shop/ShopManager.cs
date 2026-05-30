using GameLogic;
using Player.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopManager : MonoBehaviour
{
    [Header("UI Manager")]
    [SerializeField] private ShopUIManager _uiManager;

    [Header("Purchase Handler")]
    [SerializeField] private ShopItemPurchaseHandler _purchaseHandler;

    [Header("Close Button")]
    [SerializeField] private Button _closeButton;

    [Header("Input")]
    [SerializeField] private AggregatedInputProvider _inputProvider;
    private IInputProvider _input;

    [Header("Shop Items")]
    [SerializeField] private List<ShopItemData> _shopItems = new List<ShopItemData>();

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

    private ShopNavigationController _navigationController;
    public List<ShopItemData> ShopItems => _shopItems;

    private bool _isShopOpen = false;

    private void Start()
    {
        Initialize();
        LoadShopPurchases();
        InitializeShopSaveManager();
    }

    private void InitializeShopSaveManager()
    {
        if (ShopSaveManager.Instance == null)
        {
            GameObject managerObject = new GameObject("ShopSaveManager");
            managerObject.AddComponent<ShopSaveManager>();

            DontDestroyOnLoad(managerObject);
        }

        ShopSaveManager.Instance?.LoadAllPurchases();
    }

    private void LoadShopPurchases()
    {
        if (SaveSystem.Instance == null || SaveSystem.Instance.CurrentSave == null)
        {
            return;
        }

        if (SaveSystem.Instance.CurrentSave.purchasedItemIds != null)
        {
            foreach (var itemId in SaveSystem.Instance.CurrentSave.purchasedItemIds)
            {
                var item = _shopItems.Find(i => i.ItemId == itemId);

                if (item != null && itemId != CommandActiveLastChance && itemId != CommandUnlockedArmor)
                {
                    item.IsSold = true;

                    ApplyItemEffectOnLoad(itemId);
                }
            }
        }

        if (_uiManager != null)
        {
            _uiManager.RefreshItemViews();
        }
    }


    private void ApplyItemEffectOnLoad(string itemId)
    {
        var hero = FindObjectOfType<Hero>();

        if (hero == null || hero.AbilityManager == null)
        {
            return;
        }

        var armorManager = FindObjectOfType<ArmorManager>();

        var purchaseHandler = new ShopItemPurchaseHandler(hero, armorManager);

        switch (itemId)
        {
            case CommandUnlockMap: hero.AbilityManager.UnlockMap();
                
                break;

            case CommandUnlockDash: hero.AbilityManager.UnlockDash();
                
                break;

            case CommandUnlockAnatomy: hero.AbilityManager.UnlockAnatomy();
                
                break;

            case CommandUnlockArmor:
                hero.AbilityManager.UnlockArmor();
                armorManager?.UnlockArmorAbility();

                break;

            case CommandUnlockSwampDamageBonus: hero.AbilityManager.UnlockSwampDamageBonus(); 
                
                break;

            case CommandUnlockSkeletonDamageBonus: hero.AbilityManager.UnlockSkeletonDamageBonus();
                
                break;

            case CommandUnlockDemonDamageBonus: hero.AbilityManager.UnlockDemonDamageBonus();
                
                break;

            case CommandUnlockSpiderDamageBonus: hero.AbilityManager.UnlockSpiderDamageBonus();
                
                break;

            case CommandUnlockZombieDamageBonus: hero.AbilityManager.UnlockZombieDamageBonus();
                
                break;

            case CommandUnlockPassiveHealthRegeneration: hero.AbilityManager.UnlockPassiveHealthRegeneration();
                
                break;

            case CommandUnlockRobocopRegeneration: hero.AbilityManager.UnlockRobocopRegeneration();
                
                break;

            case CommandUnlockVampireAbility: hero.AbilityManager.UnlockVampireAbility(); 
                
                break;

            case CommandUnlockOnePunchManAbility: hero.AbilityManager.UnlockOnePunchManAbility();
                
                break;

            case CommandUnlockBossDamageBonus: hero.AbilityManager.UnlockBossDamageBonus();
                
                break;
        }
    }

    private void Initialize()
    {

        if (_closeButton)
        {
            _closeButton.onClick.AddListener(CloseShop);
        }

        var hero = FindObjectOfType<Hero>();
        var armorManager = FindObjectOfType<ArmorManager>();

        _purchaseHandler = new ShopItemPurchaseHandler(hero, armorManager);

        _inputProvider = FindObjectOfType<AggregatedInputProvider>();
        _input = _inputProvider as IInputProvider;

        _navigationController = new ShopNavigationController();

        InitializeNavigation();
    }


    private void InitializeNavigation()
    {
        Dictionary<WalletManager.CoinType, List<IShopItem>> itemsByCurrency = new Dictionary<WalletManager.CoinType, List<IShopItem>>();

        foreach (var item in _shopItems)
        {
            if (!itemsByCurrency.ContainsKey(item.CurrencyType))
            {
                itemsByCurrency[item.CurrencyType] = new List<IShopItem>();
            }

            itemsByCurrency[item.CurrencyType].Add(item);
        }

        _navigationController = new ShopNavigationController();

        foreach (var kvp in itemsByCurrency)
        {
            _navigationController.AddItems(kvp.Key, kvp.Value);
        }
    }

    private void Update()
    {
        if (!_isShopOpen)
        {
            return;
        }

        HandleShopInput();
    }

    private void HandleShopInput()
    {
        Vector2 input = new Vector2(
            UnityEngine.Input.GetAxisRaw("Horizontal"),
            UnityEngine.Input.GetAxisRaw("Vertical")
        );

        if (_navigationController.TryMove(input))
        {
            OnNavigationChanged();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Space) ||
            UnityEngine.Input.GetKeyDown(KeyCode.Return))
        {
            TryPurchaseSelectedItem();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    private void HandleShopInput(Vector2 input)
    {
        if (_navigationController.TryMove(input))
        {
            OnNavigationChanged();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Space) ||
            UnityEngine.Input.GetKeyDown(KeyCode.Return))
        {
            TryPurchaseSelectedItem();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    private void OnNavigationChanged()
    {
        UpdateUI();
        UpdateDescription();
    }

    private void UpdateUI()
    {
        var currency = _navigationController.CurrentCurrency;

        _uiManager.SwitchCurrency(currency);

        _uiManager.UpdateItemSelection(currency, _navigationController.CurrentItemIndex);
    }

    private void UpdateDescription()
    {
        if (_navigationController.CurrentItemIndex >= 0)
        {
            var item = _navigationController.GetCurrentItem();

            if (item != null)
            {
                string status = item.IsSold ? "<color=green>[Куплено]</color>\n" : "";

                _uiManager.UpdateDescription(status + item.Description);
            }
        }
        else
        {
            string description = GetCurrencyDescription(_navigationController.CurrentCurrency);
            _uiManager.UpdateDescription(description);
        }
    }

    private void TryPurchaseSelectedItem()
    {
        if (_navigationController.CurrentItemIndex < 0)
        {
            return;
        }

        var item = _navigationController.GetCurrentItem();

        if (item == null)
        {
            return;
        }

        if (!item.CanBePurchased())
        {
            if (item.IsSold)
            {
                _uiManager.ShowPurchaseMessage("<color=yellow>этот предмет уже куплен</color>");
            }
            else
            {
                _uiManager.ShowPurchaseMessage("<color=red>Недостаточно средств</color>");
            }

            return;
        }

        bool success = _purchaseHandler.TryPurchaseItem(item.ItemId);

        if (success)
        {
            item.Purchase();

            _uiManager.ShowPurchaseMessage("Покупка успешна!");

            _uiManager.UpdateItemSelection(_navigationController.CurrentCurrency,
                _navigationController.CurrentItemIndex);

            string status = item.IsSold ? "<color=green>[Куплено]</color>\n" : "";

            _uiManager.UpdateDescription(status + item.Description);

            SaveGameImmediately();
        }
        else
        {
            _uiManager.ShowPurchaseMessage("<color=red>Не удалось купить предмет!</color>");
        }
    }

    private void SaveGameImmediately()
    {
        var hero = FindObjectOfType<Hero>();

        if (hero != null && SaveSystem.Instance != null)
        {
            if (hero.AbilityManager != null)
            {
                SaveSystem.Instance.UpdateAbilityData(hero.AbilityManager);
            }

            SaveSystem.Instance.SaveGame("", hero.transform.position);
        }
    }


    private string GetCurrencyDescription(WalletManager.CoinType coinType)
    {
        return coinType switch
        {
            WalletManager.CoinType.Bronze => "Бронзовые монеты - базовая валюта\n↓ Выбрать предмет",
            WalletManager.CoinType.Silver => "Серебряные монеты - редкая валюта\n↓ Выбрать предмет",
            WalletManager.CoinType.Gold => "3олотые монеты - ценная валюта\n↓ Выбрать предмет",
        };
    }

    public void OpenShop()
    {
        _isShopOpen = true;

        gameObject.SetActive(true);

        if (_uiManager != null)
        {
            _uiManager.LogCurrentState();
        }
        else
        {
            _uiManager = GetComponentInChildren<ShopUIManager>(true);

            if (_uiManager == null)
            {
                return; 
            }
        }

        if (_navigationController == null)
        {
            InitializeNavigation();
        }

        _navigationController.Reset();

        if (_inputProvider != null)
        {
            _inputProvider.SetShopMode(true);
        }
        else
        {
            _inputProvider = FindObjectOfType<AggregatedInputProvider>();

            if (_inputProvider != null)
            {
                _inputProvider.SetShopMode(true);
            }
        }

        UpdateUI();
        UpdateDescription();
    }

    public void CloseShop()
    {
        _isShopOpen = false;

        gameObject.SetActive(false);

        if (_inputProvider != null)
        {
            _inputProvider.SetShopMode(false);
        }
    }

    public void NavigateUp() => HandleShopInput(Vector2.up);
    public void NavigateDown() => HandleShopInput(Vector2.down);
    public void NavigateLeft() => HandleShopInput(Vector2.left);
    public void NavigateRight() => HandleShopInput(Vector2.right);
    public void ConfirmPurchase() => TryPurchaseSelectedItem();
}
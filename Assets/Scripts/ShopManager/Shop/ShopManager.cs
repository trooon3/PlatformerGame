using GameLogic;
using Player.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopManager : MonoBehaviour
{
    private const string HorizontalAxisName = "Horizontal";
    private const string VerticalAxisName = "Vertical";

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

    [Header("MiniMap")]
    [SerializeField] private MiniMapController _miniMapController;

    private ShopNavigationController _navigationController;
    private bool _isShopOpen;

    public bool IsShopOpen => _isShopOpen;

    public List<ShopItemData> ShopItems => _shopItems;

    private void Start()
    {
        Initialize();
        LoadShopPurchases();
        InitializeShopSaveManager();
    }

    private void Update()
    {
        if (_isShopOpen == false)
        {
            return;
        }

        HandleShopInput();
    }

    private void OnDestroy()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(CloseShop);
        }

        if (_uiManager != null)
        {
            _uiManager.OnCurrencyTabClicked -= HandleCurrencyTabClick;

            if (_uiManager.BuyButton != null)
            {
                _uiManager.BuyButton.onClick.RemoveListener(ConfirmPurchase);
            }
        }
    }

    public void OpenShop()
    {
        _isShopOpen = true;

        gameObject.SetActive(true);

        EnsureInitialized();

        _miniMapController?.CloseMiniMap();

        _navigationController.Reset();

        SetShopInputMode(true);
        UpdateUI();
        UpdateDetailsPanel();
    }

    public void CloseShop()
    {
        _isShopOpen = false;

        SetShopInputMode(false);

        gameObject.SetActive(false);
    }

    public void NavigateUp() => HandleShopInput(Vector2.up);
    public void NavigateDown() => HandleShopInput(Vector2.down);
    public void NavigateLeft() => HandleShopInput(Vector2.left);
    public void NavigateRight() => HandleShopInput(Vector2.right);
    public void ConfirmPurchase() => TryPurchaseSelectedItem();

    private void Initialize()
    {
        if (_miniMapController == null)
        {
            _miniMapController = FindFirstObjectByType<MiniMapController>();
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(CloseShop);
            _closeButton.onClick.AddListener(CloseShop);
        }

        if (_uiManager == null)
        {
            _uiManager = GetComponentInChildren<ShopUIManager>(true);
        }

        if (_uiManager != null && _uiManager.BuyButton != null)
        {
            _uiManager.BuyButton.onClick.RemoveListener(ConfirmPurchase);
            _uiManager.BuyButton.onClick.AddListener(ConfirmPurchase);
        }

        Hero hero = FindFirstObjectByType<Hero>();
        ArmorManager armorManager = FindFirstObjectByType<ArmorManager>();

        _purchaseHandler = new ShopItemPurchaseHandler(hero, armorManager, this);

        if (_uiManager == null)
        {
            _uiManager = GetComponentInChildren<ShopUIManager>(true);
        }

        if (_uiManager != null)
        {
            _uiManager.OnCurrencyTabClicked -= HandleCurrencyTabClick;
            _uiManager.OnCurrencyTabClicked += HandleCurrencyTabClick;

            if (_uiManager.BuyButton != null)
            {
                _uiManager.BuyButton.onClick.RemoveListener(ConfirmPurchase);
                _uiManager.BuyButton.onClick.AddListener(ConfirmPurchase);
            }
        }

        FindInputProvider();
        InitializeNavigation();
        InitializeItemViews();
    }

    private void EnsureInitialized()
    {
        if (_miniMapController == null)
        {
            _miniMapController = FindFirstObjectByType<MiniMapController>();
        }

        if (_navigationController == null)
        {
            InitializeNavigation();
        }

        if (_uiManager == null)
        {
            _uiManager = GetComponentInChildren<ShopUIManager>(true);
        }

        if (_purchaseHandler == null)
        {
            Hero hero = FindFirstObjectByType<Hero>();
            ArmorManager armorManager = FindFirstObjectByType<ArmorManager>();

            _purchaseHandler = new ShopItemPurchaseHandler(hero, armorManager, this);
        }

        FindInputProvider();
    }

    private void FindInputProvider()
    {
        if (_inputProvider == null)
        {
            _inputProvider = FindFirstObjectByType<AggregatedInputProvider>();
        }

        _input = _inputProvider;
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

        if (SaveSystem.Instance.CurrentSave.purchasedItemIds == null)
        {
            return;
        }

        foreach (string itemId in SaveSystem.Instance.CurrentSave.purchasedItemIds)
        {
            ShopItemData item = _shopItems.Find(shopItem => shopItem.ItemId == itemId);

            if (item != null && itemId != ShopItemIds.ActivateLastChance && itemId != ShopItemIds.RestoreArmor)
            {
                item.IsSold = true;
            }
        }

        _uiManager?.RefreshItemViews();
    }

    private void InitializeNavigation()
    {
        Dictionary<WalletManager.CoinType, List<IShopItem>> itemsByCurrency = new Dictionary<WalletManager.CoinType, List<IShopItem>>();

        foreach (ShopItemData item in _shopItems)
        {
            if (itemsByCurrency.ContainsKey(item.CurrencyType) == false)
            {
                itemsByCurrency[item.CurrencyType] = new List<IShopItem>();
            }

            itemsByCurrency[item.CurrencyType].Add(item);
        }

        _navigationController = new ShopNavigationController();

        foreach (KeyValuePair<WalletManager.CoinType, List<IShopItem>> pair in itemsByCurrency)
        {
            _navigationController.AddItems(pair.Key, pair.Value);
        }
    }

    private void InitializeItemViews()
    {
        if (_uiManager == null)
        {
            return;
        }

        Dictionary<WalletManager.CoinType, List<ShopItemData>> itemsByCurrency = new Dictionary<WalletManager.CoinType, List<ShopItemData>>();

        foreach (ShopItemData item in _shopItems)
        {
            if (itemsByCurrency.ContainsKey(item.CurrencyType) == false)
            {
                itemsByCurrency[item.CurrencyType] = new List<ShopItemData>();
            }
            itemsByCurrency[item.CurrencyType].Add(item);
        }

        foreach (KeyValuePair<WalletManager.CoinType, List<ShopItemData>> kvp in itemsByCurrency)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                ShopItemView itemView = _uiManager.GetItemView(kvp.Key, i);

                if (itemView != null)
                {
                    itemView.Initialize(kvp.Value[i]);

                    itemView.OnPointerSelected -= HandlePointerSelection;
                    itemView.OnPointerSelected += HandlePointerSelection;
                }
            }
        }
    }

    private void HandlePointerSelection(IShopItem selectedItem)
    {
        if (_navigationController == null || selectedItem == null)
        {
            return;
        }

        if (_navigationController.TrySetSelectedItem(selectedItem))
        {
            OnNavigationChanged();
        }
    }

    private void HandleCurrencyTabClick(WalletManager.CoinType currency)
    {
        if (_navigationController != null && _navigationController.TrySetCurrency(currency))
        {
            OnNavigationChanged();
        }
    }

    private void HandleShopInput()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw(HorizontalAxisName), Input.GetAxisRaw(VerticalAxisName));
        HandleShopInput(input);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            TryPurchaseSelectedItem();
        }

        if ((_input != null && _input.IsMenuPressed) || Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    private void HandleShopInput(Vector2 input)
    {
        if (_navigationController == null)
        {
            return;
        }

        if (_navigationController.TryMove(input))
        {
            OnNavigationChanged();
        }
    }

    private void OnNavigationChanged()
    {
        UpdateUI();
        UpdateDetailsPanel();
    }

    private void UpdateUI()
    {
        if (_uiManager == null || _navigationController == null)
        {
            return;
        }

        WalletManager.CoinType currency = _navigationController.CurrentCurrency;

        _uiManager.SwitchCurrency(currency);
        _uiManager.UpdateItemSelection(currency, _navigationController.CurrentItemIndex);
    }

    private void UpdateDetailsPanel()
    {
        if (_uiManager == null || _navigationController == null)
        {
            return;
        }

        IShopItem item = _navigationController.GetCurrentItem();

        if (item != null)
        {
            ShopItemView view = _uiManager.GetItemViewById(_navigationController.CurrentCurrency, item.ItemId);
            Sprite icon = view != null ? view.ItemIcon : null;

            string itemName = _purchaseHandler.GetItemName(item.ItemId);
            string itemDesc = _purchaseHandler.GetItemDescription(item.ItemId);

            bool isEnglish = LocalizationManager.CurrentLanguage == LocalizationManager.Language.English;
            string boughtText = isEnglish ? "[Bought]" : "[Куплено]";

            string statusMessage = item.IsSold && item.ItemId != ShopItemIds.RestoreArmor
                ? $"<color=green>{boughtText}</color>\n" + itemDesc
                : itemDesc;

            _uiManager.UpdateRightPanel(item, icon, itemName, statusMessage);
        }
        else
        {
            _uiManager.UpdateRightPanel(null, null, null, GetCurrencyDescription(_navigationController.CurrentCurrency));
        }
    }

    private void TryPurchaseSelectedItem()
    {
        IShopItem item = _navigationController?.GetCurrentItem();
        if (item == null) return;

        if (item.CanBePurchased() == false)
        {
            ShowCannotPurchaseMessage(item);
            return;
        }

        bool success = _purchaseHandler.TryPurchaseItem(item.ItemId);
        bool isEnglish = LocalizationManager.CurrentLanguage == LocalizationManager.Language.English;

        if (success == false)
        {
            _uiManager?.ShowPurchaseMessage(isEnglish ? "<color=red>Failed to purchase</color>" : "<color=red>Не удалось купить предмет</color>");
            return;
        }

        item.Purchase();

        _uiManager?.ShowPurchaseMessage(isEnglish ? "Purchase successful" : "Покупка успешна");
        _uiManager?.RefreshLastChanceItems();

        UpdateUI();
        UpdateDetailsPanel();
        SaveGameImmediately();
    }

    private void ShowCannotPurchaseMessage(IShopItem item)
    {
        bool isEnglish = LocalizationManager.CurrentLanguage == LocalizationManager.Language.English;

        if (item.IsSold && item.ItemId != ShopItemIds.RestoreArmor)
        {
            _uiManager?.ShowPurchaseMessage(isEnglish ? "<color=yellow>Item already bought</color>" : "<color=yellow>Этот предмет уже куплен</color>");
        }
        else
        {
            _uiManager?.ShowPurchaseMessage(isEnglish ? "<color=red>Not enough funds or locked</color>" : "<color=red>Недостаточно средств или покупка недоступна</color>");
        }
    }

    private void SaveGameImmediately()
    {
        Hero hero = FindFirstObjectByType<Hero>();

        if (hero == null || SaveSystem.Instance == null)
        {
            return;
        }

        if (hero.AbilityManager != null)
        {
            SaveSystem.Instance.UpdateAbilityData(hero.AbilityManager);
        }

        SaveSystem.Instance.SaveGame(string.Empty, hero.transform.position);
    }

    private string GetCurrencyDescription(WalletManager.CoinType coinType)
    {
        return string.Empty;
    }

    private void SetShopInputMode(bool isShopOpen)
    {
        if (_inputProvider == null)
        {
            _inputProvider = FindFirstObjectByType<AggregatedInputProvider>();
        }

        _inputProvider?.SetShopMode(isShopOpen);
    }
}
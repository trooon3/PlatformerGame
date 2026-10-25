using GameLogic;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public sealed class ShopUIManager : MonoBehaviour
{
    [Header("Currency Panels")]
    [SerializeField] private GameObject _bronzeMenuPanel;
    [SerializeField] private GameObject _silverMenuPanel;
    [SerializeField] private GameObject _goldMenuPanel;

    [Header("Currency Tabs (Clickable Areas)")]
    [SerializeField] private GameObject _bronzeTabArea;
    [SerializeField] private GameObject _silverTabArea;
    [SerializeField] private GameObject _goldTabArea;

    [Header("Currency Sprites (For Prices)")]
    [SerializeField] private Sprite _bronzeCoinSprite;
    [SerializeField] private Sprite _silverCoinSprite;
    [SerializeField] private Sprite _goldCoinSprite;

    [Header("Currency Indicators")]
    [SerializeField] private Image _bronzeCoinIndicator;
    [SerializeField] private Image _silverCoinIndicator;
    [SerializeField] private Image _goldCoinIndicator;
    [SerializeField] private Color _selectedCoinColor = Color.white;
    [SerializeField] private Color _unselectedCoinColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("Currency Colors (Text)")]
    [SerializeField] private Color _bronzeTextColor = new Color(0.8f, 0.5f, 0.2f);
    [SerializeField] private Color _silverTextColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color _goldTextColor = new Color(1f, 0.84f, 0f);

    [Header("Currency Display")]
    [SerializeField] private TextMeshProUGUI _currentCurrencyText;
    [SerializeField] private Image _currentCurrencyIcon;

    [Header("Right Panel Details")]
    [SerializeField] private TextMeshProUGUI _detailsNameText;
    [SerializeField] private GameObject _detailsTitleDivider;
    [SerializeField] private TextMeshProUGUI _detailsDescriptionText;
    [SerializeField] private Image _detailsItemIcon;

    [Header("Objects to Hide When Empty")]
    [SerializeField] private GameObject _purchaseFrameObject; 
    [SerializeField] private GameObject _priceTextObject;     
    [SerializeField] private GameObject _priceIconObject;     
    [SerializeField] private GameObject _buyButtonObject;     

    [Header("Price Data")]
    [SerializeField] private TextMeshProUGUI _detailsPriceText;
    [SerializeField] private Image _detailsPriceCurrencyIcon;
    [SerializeField] private Button _buyButton;

    public event Action<WalletManager.CoinType> OnCurrencyTabClicked;

    public Button BuyButton => _buyButton;

    private readonly Dictionary<WalletManager.CoinType, List<ShopItemView>> _itemViewsByCurrency =
        new Dictionary<WalletManager.CoinType, List<ShopItemView>>();

    private WalletManager.CoinType _currentCurrency = WalletManager.CoinType.Bronze;
    private bool _isInitialized;

    private void Start()
    {
        Initialize();
        SetupCurrencyClickHandlers();
    }

    private void SetupCurrencyClickHandlers()
    {
        BindCurrencyClick(_bronzeTabArea, WalletManager.CoinType.Bronze);
        BindCurrencyClick(_silverTabArea, WalletManager.CoinType.Silver);
        BindCurrencyClick(_goldTabArea, WalletManager.CoinType.Gold);
    }

    private void BindCurrencyClick(GameObject tabObject, WalletManager.CoinType currency)
    {
        if (tabObject == null)
        {
            return;
        }

        EventTrigger trigger = tabObject.GetComponent<EventTrigger>();

        if (trigger == null)
        {
            trigger = tabObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };

        clickEntry.callback.AddListener((data) =>
        {
            PointerEventData pointerData = data as PointerEventData;

            if (pointerData != null && pointerData.button == PointerEventData.InputButton.Left)
            {
                OnCurrencyTabClicked?.Invoke(currency);
            }
        });

        trigger.triggers.Add(clickEntry);
    }

    public void SwitchCurrency(WalletManager.CoinType currencyType)
    {
        Initialize();

        _currentCurrency = currencyType;

        UpdateCurrencyDisplay();
        ShowCurrencyMenu(currencyType);
    }

    public void ShowCurrencyMenu(WalletManager.CoinType currencyType)
    {
        HideAllMenus();

        GameObject menu = GetPanel(currencyType);

        if (menu != null)
        {
            menu.SetActive(true);
        }
    }

    public void UpdateItemSelection(WalletManager.CoinType currency, int selectedIndex)
    {
        Initialize();

        ResetAllItemSelection();

        if (_itemViewsByCurrency.TryGetValue(currency, out List<ShopItemView> itemViews) == false)
        {
            return;
        }

        if (selectedIndex < 0 || selectedIndex >= itemViews.Count)
        {
            return;
        }

        ShopItemView selectedView = itemViews[selectedIndex];

        if (selectedView == null)
        {
            return;
        }

        bool canPurchase = selectedView.ItemData != null && selectedView.ItemData.CanBePurchased();

        selectedView.SetSelected(true, canPurchase);
    }

    public ShopItemView GetItemView(WalletManager.CoinType currency, int index)
    {
        Initialize();

        if (_itemViewsByCurrency.TryGetValue(currency, out List<ShopItemView> itemViews) == false)
        {
            return null;
        }

        return index >= 0 && index < itemViews.Count ? itemViews[index] : null;
    }

    public ShopItemView GetItemViewById(WalletManager.CoinType currency, string itemId)
    {
        Initialize();

        if (_itemViewsByCurrency.TryGetValue(currency, out List<ShopItemView> itemViews) == false)
        {
            return null;
        }

        foreach (ShopItemView itemView in itemViews)
        {
            if (itemView != null && itemView.ItemData != null && itemView.ItemData.ItemId == itemId)
            {
                return itemView;
            }
        }

        return null;
    }

    public void UpdateRightPanel(IShopItem item, Sprite icon, string customName = null, string customMessage = null)
    {
        if (item == null)
        {
            ClearRightPanel(customMessage);
            return;
        }

        if (_purchaseFrameObject != null) _purchaseFrameObject.SetActive(true);
        if (_priceTextObject != null) _priceTextObject.SetActive(true);
        if (_priceIconObject != null) _priceIconObject.SetActive(true);
        if (_buyButtonObject != null) _buyButtonObject.SetActive(true);

        if (_detailsNameText != null) _detailsNameText.text = !string.IsNullOrEmpty(customName) ? customName : item.DisplayName;
        if (_detailsTitleDivider != null) _detailsTitleDivider.SetActive(true);
        if (_detailsDescriptionText != null) _detailsDescriptionText.text = customMessage ?? item.Description;

        if (_detailsItemIcon != null)
        {
            _detailsItemIcon.sprite = icon;
            _detailsItemIcon.gameObject.SetActive(icon != null);
        }

        if (_detailsPriceText != null)
        {
            bool isEnglish = LocalizationManager.CurrentLanguage == LocalizationManager.Language.English;

            if (item.ItemId == ShopItemIds.ActivateLastChance && item.IsSold)
            {
                _detailsPriceText.text = isEnglish ? "Active" : "Aктивно";
                _detailsPriceText.color = Color.yellow;
                if (_priceIconObject != null) _priceIconObject.SetActive(false);
            }
            else if (item.IsSold && item.ItemId != ShopItemIds.RestoreArmor)
            {
                _detailsPriceText.text = isEnglish ? "Bought" : "Куплено";
                _detailsPriceText.color = Color.green;
                if (_priceIconObject != null) _priceIconObject.SetActive(false);
            }
            else
            {
                _detailsPriceText.text = $"{item.Price}";
                _detailsPriceText.color = GetCurrencyColor(item.CurrencyType);

                if (_detailsPriceCurrencyIcon != null)
                {
                    _detailsPriceCurrencyIcon.sprite = GetCurrencySprite(item.CurrencyType);
                    if (_priceIconObject != null) _priceIconObject.SetActive(true);
                }
            }
        }

        if (_buyButton != null) _buyButton.interactable = item.CanBePurchased();
    }

    private void ClearRightPanel(string message)
    {
        if (_purchaseFrameObject != null) _purchaseFrameObject.SetActive(false);
        if (_priceTextObject != null) _priceTextObject.SetActive(false);
        if (_priceIconObject != null) _priceIconObject.SetActive(false);
        if (_buyButtonObject != null) _buyButtonObject.SetActive(false);

        if (_detailsNameText != null) _detailsNameText.text = string.Empty;
        if (_detailsTitleDivider != null) _detailsTitleDivider.SetActive(false);
        if (_detailsDescriptionText != null) _detailsDescriptionText.text = message ?? string.Empty;

        if (_detailsItemIcon != null) _detailsItemIcon.gameObject.SetActive(false);
    }

    public void ShowPurchaseMessage(string message)
    {
        if (_detailsDescriptionText != null)
        {
            _detailsDescriptionText.text = $"<color=green>✓ {message}</color>";
        }
    }

    private void SetDetailsCurrencyIconActive(bool isActive)
    {
        if (_detailsPriceCurrencyIcon != null)
        {
            _detailsPriceCurrencyIcon.gameObject.SetActive(isActive);
        }
    }

    public void RefreshItemViews()
    {
        _isInitialized = false;

        Initialize();
    }

    public void RefreshLastChanceItems()
    {
        Initialize();

        foreach (List<ShopItemView> viewList in _itemViewsByCurrency.Values)
        {
            foreach (ShopItemView view in viewList)
            {
                if (view != null && view.ItemData != null && view.ItemData.ItemId == ShopItemIds.ActivateLastChance)
                {
                    view.UpdateView();
                }
            }
        }
    }

    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _itemViewsByCurrency.Clear();

        foreach (WalletManager.CoinType currency in System.Enum.GetValues(typeof(WalletManager.CoinType)))
        {
            _itemViewsByCurrency[currency] = new List<ShopItemView>();
        }

        FindAllItemViews();

        _isInitialized = true;
    }

    private void FindAllItemViews()
    {
        FindItemViewsInPanel(_bronzeMenuPanel, WalletManager.CoinType.Bronze);
        FindItemViewsInPanel(_silverMenuPanel, WalletManager.CoinType.Silver);
        FindItemViewsInPanel(_goldMenuPanel, WalletManager.CoinType.Gold);
    }

    private void FindItemViewsInPanel(GameObject panel, WalletManager.CoinType currencyType)
    {
        if (panel == null)
        {
            return;
        }

        ShopItemView[] itemViews = panel.GetComponentsInChildren<ShopItemView>(true);

        foreach (ShopItemView itemView in itemViews)
        {
            if (itemView != null)
            {
                _itemViewsByCurrency[currencyType].Add(itemView);
            }
        }
    }

    private void ResetAllItemSelection()
    {
        foreach (List<ShopItemView> itemViews in _itemViewsByCurrency.Values)
        {
            foreach (ShopItemView itemView in itemViews)
            {
                itemView?.SetSelected(false, true);
            }
        }
    }

    private void UpdateCurrencyDisplay()
    {
        if (WalletManager.Instance == null || _currentCurrencyText == null)
        {
            return;
        }

        _currentCurrencyText.text = WalletManager.Instance.GetCoins(_currentCurrency).ToString();

        if (_currentCurrencyIcon != null)
        {
            _currentCurrencyIcon.sprite = GetCurrencySprite(_currentCurrency);
        }
    }

    private void SetIndicatorColor(Image indicator, WalletManager.CoinType currency)
    {
        if (indicator != null)
        {
            indicator.color = _currentCurrency == currency ? _selectedCoinColor : _unselectedCoinColor;
        }
    }

    private void HideAllMenus()
    {
        if (_bronzeMenuPanel != null)
        {
            _bronzeMenuPanel.SetActive(false);
        }

        if (_silverMenuPanel != null)
        {
            _silverMenuPanel.SetActive(false);
        }

        if (_goldMenuPanel != null)
        {
            _goldMenuPanel.SetActive(false);
        }
    }

    private GameObject GetPanel(WalletManager.CoinType currencyType)
    {
        return currencyType switch
        {
            WalletManager.CoinType.Bronze => _bronzeMenuPanel,
            WalletManager.CoinType.Silver => _silverMenuPanel,
            WalletManager.CoinType.Gold => _goldMenuPanel,
            _ => null
        };
    }

    private Sprite GetCurrencySprite(WalletManager.CoinType coinType)
    {
        return coinType switch
        {
            WalletManager.CoinType.Bronze => _bronzeCoinSprite,
            WalletManager.CoinType.Silver => _silverCoinSprite,
            WalletManager.CoinType.Gold => _goldCoinSprite,
            _ => null
        };
    }

    private Color GetCurrencyColor(WalletManager.CoinType coinType)
    {
        return coinType switch
        {
            WalletManager.CoinType.Bronze => _bronzeTextColor,
            WalletManager.CoinType.Silver => _silverTextColor,
            WalletManager.CoinType.Gold => _goldTextColor,
            _ => Color.white
        };
    }
}
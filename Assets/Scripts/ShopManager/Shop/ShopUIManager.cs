using GameLogic;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopUIManager : MonoBehaviour
{
    [Header("Currency Panels")]
    [SerializeField] private GameObject _bronzeMenuPanel;
    [SerializeField] private GameObject _silverMenuPanel;
    [SerializeField] private GameObject _goldMenuPanel;

    [Header("Currency Indicators")]
    [SerializeField] private Image _bronzeCoinIndicator;
    [SerializeField] private Image _silverCoinIndicator;
    [SerializeField] private Image _goldCoinIndicator;
    [SerializeField] private Color _selectedCoinColor = Color.white;
    [SerializeField] private Color _unselectedCoinColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("Currency Display")]
    [SerializeField] private TextMeshProUGUI _currentCurrencyText;
    [SerializeField] private Image _currentCurrencyIcon;

    [Header("Description")]
    [SerializeField] private TextMeshProUGUI _selectedItemDescription;

    private const string CommandActiveLastChance = "6";

    private Dictionary<WalletManager.CoinType, List<ShopItemView>> _itemViewsByCurrency;
    private WalletManager.CoinType _currentCurrency = WalletManager.CoinType.Bronze;
    private bool _isInitialized = false;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _itemViewsByCurrency = new Dictionary<WalletManager.CoinType, List<ShopItemView>>();

        foreach (WalletManager.CoinType currency in System.Enum.GetValues(typeof(WalletManager.CoinType)))
        {
            _itemViewsByCurrency[currency] = new List<ShopItemView>();
        }

        FindAllItemViews();

        _isInitialized = true;
    }

    private void FindAllItemViews()
    {
        foreach (var list in _itemViewsByCurrency.Values)
        {
            list.Clear();
        }

        FindItemViewsInPanel(_bronzeMenuPanel, WalletManager.CoinType.Bronze);
        FindItemViewsInPanel(_silverMenuPanel, WalletManager.CoinType.Silver);
        FindItemViewsInPanel(_goldMenuPanel, WalletManager.CoinType.Gold);

        int totalViews = 0;

        foreach (var kvp in _itemViewsByCurrency)
        {
            totalViews += kvp.Value.Count;
        }
    }

    private void FindItemViewsInPanel(GameObject panel, WalletManager.CoinType currencyType)
    {
        if (panel == null)
        {
            return;
        }

        var itemViews = panel.GetComponentsInChildren<ShopItemView>(true);

        foreach (var itemView in itemViews)
        {
            if (itemView != null)
            {
                _itemViewsByCurrency[currencyType].Add(itemView);
            }
        }
    }

    public void SwitchCurrency(WalletManager.CoinType currencyType)
    {
        _currentCurrency = currencyType;

        UpdateCurrencyDisplay();
        UpdateCoinIndicators();
        ShowCurrencyMenu(currencyType);
    }

    public void ShowCurrencyMenu(WalletManager.CoinType currencyType)
    {
        HideAllMenus();

        switch (currencyType)
        {
            case WalletManager.CoinType.Bronze:
                if (_bronzeMenuPanel != null)
                {
                    _bronzeMenuPanel.SetActive(true);
                }

                break;

            case WalletManager.CoinType.Silver:
                if (_silverMenuPanel != null)
                {
                    _silverMenuPanel.SetActive(true);
                }

                break;

            case WalletManager.CoinType.Gold:
                if (_goldMenuPanel != null)
                {
                    _goldMenuPanel.SetActive(true);
                }

                break;
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

    public void UpdateItemSelection(WalletManager.CoinType currency, int selectedIndex)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        ResetAllItemSelection();

        if (_itemViewsByCurrency.TryGetValue(currency, out var itemViews))
        {
            if (selectedIndex >= 0 && selectedIndex < itemViews.Count)
            {
                var selectedView = itemViews[selectedIndex];

                if (selectedView != null)
                {
                    var itemData = selectedView.ItemData;

                    bool canPurchase = itemData != null && itemData.CanBePurchased();
                    selectedView.SetSelected(true, canPurchase);
                }
            }
        }
    }

    private void ResetAllItemSelection()
    {
        if (!_isInitialized)
        {
            return;
        }

        foreach (var currency in _itemViewsByCurrency.Keys)
        {
            if (!_itemViewsByCurrency.TryGetValue(currency, out var itemViews))
            {
                continue;
            }

            foreach (var itemView in itemViews)
            {
                if (itemView != null)
                {
                    itemView.SetSelected(false, true);
                }
            }
        }
    }

    public ShopItemView GetItemView(WalletManager.CoinType currency, int index)
    {
        if (!_isInitialized) Initialize();

        if (_itemViewsByCurrency.TryGetValue(currency, out var itemViews))
        {
            if (index >= 0 && index < itemViews.Count)
            {
                return itemViews[index];
            }
        }

        return null;
    }

    public ShopItemView GetItemViewById(WalletManager.CoinType currency, string itemId)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        if (_itemViewsByCurrency.TryGetValue(currency, out var itemViews))
        {
            foreach (var itemView in itemViews)
            {
                if (itemView != null && itemView.ItemData != null && itemView.ItemData.ItemId == itemId)
                {
                    return itemView;
                }
            }
        }

        return null;
    }

    private void UpdateCurrencyDisplay()
    {
        if (!WalletManager.Instance || !_currentCurrencyText)
        {
            return;
        }

        int coins = WalletManager.Instance.GetCoins(_currentCurrency);

        _currentCurrencyText.text = coins.ToString();

        if (_currentCurrencyIcon)
        {
            _currentCurrencyIcon.sprite = GetCurrencySprite(_currentCurrency);
        }
    }

    private void UpdateCoinIndicators()
    {
        if (_bronzeCoinIndicator)
        {
            _bronzeCoinIndicator.color = _currentCurrency == WalletManager.CoinType.Bronze ?
                _selectedCoinColor : _unselectedCoinColor;
        }

        if (_silverCoinIndicator)
        {
            _silverCoinIndicator.color = _currentCurrency == WalletManager.CoinType.Silver ?
                _selectedCoinColor : _unselectedCoinColor;
        }

        if (_goldCoinIndicator)
        {
            _goldCoinIndicator.color = _currentCurrency == WalletManager.CoinType.Gold ?
                _selectedCoinColor : _unselectedCoinColor;
        }
    }

    public void UpdateDescription(string description)
    {
        if (_selectedItemDescription)
        {
            _selectedItemDescription.text = description;
        }
    }

    public void ShowPurchaseMessage(string message)
    {
        if (_selectedItemDescription)
        {
            _selectedItemDescription.text = $"<color=green>✓ {message}</color>";
        }
    }

    private Sprite GetCurrencySprite(WalletManager.CoinType coinType)
    {
        return coinType switch
        {
            WalletManager.CoinType.Bronze => _bronzeCoinIndicator?.sprite,
            WalletManager.CoinType.Silver => _silverCoinIndicator?.sprite,
            WalletManager.CoinType.Gold => _goldCoinIndicator?.sprite
        };
    }

    public void RefreshItemViews()
    {
        _isInitialized = false;

        Initialize();
    }

    public void LogCurrentState()
    {
        if (!_isInitialized)
        {
            return;
        }

        foreach (var kvp in _itemViewsByCurrency)
        {
            int validCount = 0;
            int nullCount = 0;

            foreach (var view in kvp.Value)
            {
                if (view != null)
                {
                    validCount++;
                }
                else
                {
                    nullCount++;
                }
            }
        }
    }

    public void RefreshLastChanceItems()
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        foreach (var viewList in _itemViewsByCurrency.Values)
        {
            foreach (var view in viewList)
            {
                if (view != null && view.ItemData != null && view.ItemData.ItemId == CommandActiveLastChance)
                {
                    view.UpdateView();
                }
            }
        }
    }
}
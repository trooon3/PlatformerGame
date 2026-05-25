using GameLogic;
using Player.Abilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopItemView : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private GameObject _purchasedOverlay;

    private const string CommandActiveLastChance = "6";
    private const string CommandUnlockedArmor = "7";

    private Color _originalIconColor;
    private Color _originalNameColor;
    private Color _originalPriceColor;
    private Color _originalBackgroundColor;
    private Vector3 _originalScale;

    private IShopItem _itemData;

    public IShopItem ItemData => _itemData;
    public RectTransform RectTransform { get; private set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();

        CacheOriginalColors();

        if (_purchasedOverlay == null)
        {
            CreatePurchasedOverlay();
        }
    }

    private void CreatePurchasedOverlay()
    {
        _purchasedOverlay = new GameObject("PurchasedOverlay");
        _purchasedOverlay.transform.SetParent(transform, false);

        var overlayImage = _purchasedOverlay.AddComponent<Image>();

        overlayImage.color = new Color(0, 1, 0, 0.2f); 

        var rectTransform = _purchasedOverlay.GetComponent<RectTransform>();

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        _purchasedOverlay.SetActive(false);
    }

    private void CacheOriginalColors()
    {
        if (_iconImage)
        {
            _originalIconColor = _iconImage.color;
        }

        if (_nameText)
        {
            _originalNameColor = _nameText.color;
        }

        if (_priceText)
        {
            _originalPriceColor = _priceText.color;
        }

        if (_backgroundImage)
        {
            _originalBackgroundColor = _backgroundImage.color;
        }

        _originalScale = RectTransform.localScale;
    }

    public void Initialize(IShopItem itemData)
    {
        _itemData = itemData;

        UpdateView();
    }

    public void UpdateView()
    {
        if (_itemData == null)
        {
            return;
        }

        if (_nameText)
        {
            _nameText.text = _itemData.DisplayName ?? "нет имени";
        }

        if (_priceText)
        {
            if (_itemData.ItemId == CommandActiveLastChance)
            {
                var abilityManager = FindAbilityManager();

                if (abilityManager != null && abilityManager.IsLastChanceActive)
                {
                    _priceText.text = "<color=yellow>Активно</color>";
                }
                else
                {
                    _priceText.text = $"{_itemData.Price} {GetCurrencySymbol(_itemData.CurrencyType)}";
                }
            }
            else if (_itemData.IsSold && _itemData.ItemId != CommandUnlockedArmor)
            {
                _priceText.text = "<color=green>Куплено</color>";
            }
            else
            {
                _priceText.text = $"{_itemData.Price} {GetCurrencySymbol(_itemData.CurrencyType)}";
            }
        }

        if (_purchasedOverlay != null)
        {
            bool shouldShowOverlay = false;

            if (_itemData.ItemId == CommandActiveLastChance)
            {
                var abilityManager = FindAbilityManager();

                shouldShowOverlay = abilityManager != null && abilityManager.IsLastChanceActive;
            }
            else if (_itemData.ItemId != CommandUnlockedArmor)
            {
                shouldShowOverlay = _itemData.IsSold;
            }

            _purchasedOverlay.SetActive(shouldShowOverlay);
        }
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

    public void SetSelected(bool isSelected, bool isAvailable)
    {
        Color _soldSelectedColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);

        if (_backgroundImage)
        {
            if (_itemData != null && _itemData.IsSold && _itemData.ItemId != CommandUnlockedArmor)
            {
                _backgroundImage.color = isSelected ?
                    new Color(0.2f, 0.8f, 0.2f, 0.8f) : 
                    new Color(0, 0.6f, 0, 0.3f); 
            }
            else
            {
                _backgroundImage.color = isSelected ?
                    (isAvailable ? Color.green : Color.red) :
                    _originalBackgroundColor;
            }
        }

        if (RectTransform)
        {
            RectTransform.localScale = isSelected ? _originalScale * 1.05f : _originalScale;
        }
    }

    public void ResetVisuals()
    {
        if (_iconImage)
        {
            _iconImage.color = _originalIconColor;
        }

        if (_nameText)
        {
            _nameText.color = _originalNameColor;
        }

        if (_priceText)
        {
            _priceText.color = _originalPriceColor;
        }

        if (_backgroundImage)
        {
            _backgroundImage.color = _originalBackgroundColor;
        }

        if (RectTransform)
        {
            RectTransform.localScale = _originalScale;
        }
    }

    private string GetCurrencySymbol(WalletManager.CoinType coinType)
    {
        return coinType switch
        {
            WalletManager.CoinType.Gold => "G",
            WalletManager.CoinType.Silver => "S",
            WalletManager.CoinType.Bronze => "B"
        };
    }
}
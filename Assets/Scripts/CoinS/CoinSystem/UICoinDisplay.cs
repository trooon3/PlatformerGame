using GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UICoinDisplay : MonoBehaviour
{
    [System.Serializable]
    public sealed class CoinDisplay
    {
        public WalletManager.CoinType coinType;
        public Image coinIcon;
        public TMP_Text coinText;
        public string displayFormat = "{0}";
    }

    [Header("UI Elements")]
    [SerializeField] private CoinDisplay[] _coinDisplays;

    private void Start()
    {
        if (WalletManager.Instance == null)
        {
            return;
        }

        WalletManager.Instance.OnCoinsChanged += OnCoinsChanged;

        InitializeDisplay();
    }

    private void OnDestroy()
    {
        if (WalletManager.Instance != null)
        {
            WalletManager.Instance.OnCoinsChanged -= OnCoinsChanged;
        }
    }

    private void InitializeDisplay()
    {
        if (_coinDisplays == null || WalletManager.Instance == null)
        {
            return;
        }

        foreach (CoinDisplay display in _coinDisplays)
        {
            if (display == null)
            {
                continue;
            }

            int currentCoins = WalletManager.Instance.GetCoins(display.coinType);

            UpdateCoinDisplay(display, currentCoins);
        }
    }

    private void OnCoinsChanged(WalletManager.CoinType coinType, int newAmount)
    {
        if (_coinDisplays == null)
        {
            return;
        }

        foreach (CoinDisplay display in _coinDisplays)
        {
            if (display == null || display.coinType != coinType)
            {
                continue;
            }

            UpdateCoinDisplay(display, newAmount);

            break;
        }
    }

    private void UpdateCoinDisplay(CoinDisplay display, int amount)
    {
        if (display.coinText == null)
        {
            return;
        }

        display.coinText.text = FormatCoinAmount(display.displayFormat, amount);

    }

    private string FormatCoinAmount(string displayFormat, int amount)
    {
        if (string.IsNullOrEmpty(displayFormat))
        {
            return amount.ToString();
        }

        try
        {
            return string.Format(displayFormat, amount);
        }
        catch (System.FormatException)
        {
            return amount.ToString();
        }
    }
}
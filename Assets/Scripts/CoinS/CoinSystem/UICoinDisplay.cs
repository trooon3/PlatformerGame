using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameLogic;

public class UICoinDisplay : MonoBehaviour
{
    [System.Serializable]
    public class CoinDisplay
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
            var walletObject = GameObject.FindObjectOfType<WalletManager>();

            if (walletObject == null)
            {
                return;
            }
        }
        else
        {
            WalletManager.Instance.OnCoinsChanged += OnCoinsChanged;

            InitializeDisplay();
        }
    }

    private void InitializeDisplay()
    {
        foreach (var display in _coinDisplays)
        {
            int currentCoins = WalletManager.Instance.GetCoins(display.coinType);

            UpdateCoinDisplay(display, currentCoins);
        }
    }

    private void OnCoinsChanged(WalletManager.CoinType coinType, int newAmount)
    {
        foreach (var display in _coinDisplays)
        {
            if (display.coinType == coinType)
            {
                UpdateCoinDisplay(display, newAmount);

                break;
            }
        }
    }

    private void UpdateCoinDisplay(CoinDisplay display, int amount)
    {
        if (display.coinText != null)
        {
            display.coinText.text = amount.ToString();

            if (!string.IsNullOrEmpty(display.displayFormat))
            {
                try
                {
                    display.coinText.text = string.Format(display.displayFormat, amount);
                }
                catch (System.Exception except)
                {
                    display.coinText.text = amount.ToString(); 
                }
            }
            else
            {
                display.coinText.text = amount.ToString();
            }

        }

        UpdateDisplayColor(display, amount);
    }

    private void UpdateDisplayColor(CoinDisplay display, int amount)
    {
         int highThreshold = 100;
         int mediumThreshold = 50;

        if (display.coinText == null)
        {
            return;
        }

        if (amount >= highThreshold)
        {
            display.coinText.color = Color.yellow;
        }
        else if (amount >= mediumThreshold)
        {
            display.coinText.color = Color.white;
        }
        else
        {
            display.coinText.color = Color.gray;
        }
    }

    private void OnDestroy()
    {
        if (WalletManager.Instance != null)
        {
            WalletManager.Instance.OnCoinsChanged -= OnCoinsChanged;
        }
    }
}
using GameLogic;
using System.Collections.Generic;
using UnityEngine;

public sealed class ShopNavigationController
{
    private const float DefaultMoveDelay = 0.2f;

    private readonly float _moveDelay;
    private Dictionary<WalletManager.CoinType, List<IShopItem>> _itemsByCurrency;

    private WalletManager.CoinType _currentCurrency;
    private int _currentItemIndex;
    private float _lastMoveTime;

    public WalletManager.CoinType CurrentCurrency => _currentCurrency;
    public int CurrentItemIndex => _currentItemIndex;

    public ShopNavigationController(float moveDelay = DefaultMoveDelay)
    {
        _moveDelay = moveDelay;
        _itemsByCurrency = new Dictionary<WalletManager.CoinType, List<IShopItem>>();

        foreach (WalletManager.CoinType currency in System.Enum.GetValues(typeof(WalletManager.CoinType)))
        {
            _itemsByCurrency[currency] = new List<IShopItem>();
        }
    }

    public void AddItems(WalletManager.CoinType currency, List<IShopItem> items)
    {
        if (_itemsByCurrency.ContainsKey(currency))
        {
            _itemsByCurrency[currency] = items;
        }
    }

    public bool TryMove(Vector2 inputDirection)
    {
        float inputMagnitudeThreshold = 0.1f;

        if (Time.time - _lastMoveTime < _moveDelay)
        {
            return false;
        }

        bool moved = false;

        if (Mathf.Abs(inputDirection.x) > inputMagnitudeThreshold || Mathf.Abs(inputDirection.y) > inputMagnitudeThreshold)
        {
            moved = HandleNavigation(inputDirection);
        }

        if (moved)
        {
            _lastMoveTime = Time.time;
        }

        return moved;
    }

    private bool HandleNavigation(Vector2 inputDirection)
    {
        float inputThreshold = 0.1f;
        int leftDirection = -1;
        int rightDirection = 1;
        int resetItemIndex = -1;
        int firstItemIndex = 0;

        int itemCount = GetCurrentItemCount();

        if (_currentItemIndex < firstItemIndex)
        {
            if (inputDirection.x < -inputThreshold)
            {
                return SwitchCurrency(leftDirection);
            }
            else if (inputDirection.x > inputThreshold)
            {
                return SwitchCurrency(rightDirection);
            }
            else if (inputDirection.y < -inputThreshold && itemCount > firstItemIndex)
            {
                _currentItemIndex = firstItemIndex;

                return true;
            }
        }
        else
        {
            if (inputDirection.y > inputThreshold)
            {
                _currentItemIndex = leftDirection;

                return true;
            }
            else if (inputDirection.x < -inputThreshold)
            {
                if (_currentItemIndex > firstItemIndex)
                {
                    _currentItemIndex--;

                    return true;
                }
            }
            else if (inputDirection.x > inputThreshold)
            {
                if (_currentItemIndex < itemCount - rightDirection)
                {
                    _currentItemIndex++;

                    return true;
                }
            }
            else if (inputDirection.y < -inputThreshold)
            {
                if (_currentItemIndex < itemCount - rightDirection)
                {
                    _currentItemIndex++;

                    return true;
                }
            }
        }

        return false;
    }

    private bool SwitchCurrency(int direction)
    {
        int minimumIndex = 0;
        int resetItemIndex = -1;

        int currencyCount = System.Enum.GetValues(typeof(WalletManager.CoinType)).Length;

        int currentCurrencyIndex = (int)_currentCurrency;

        int newCurrencyIndex = currentCurrencyIndex + direction;

        if (newCurrencyIndex >= minimumIndex && newCurrencyIndex < currencyCount)
        {
            _currentCurrency = (WalletManager.CoinType)newCurrencyIndex;
            _currentItemIndex = resetItemIndex;

            return true;
        }

        return false;
    }

    public IShopItem GetCurrentItem()
    {
        int minimumValidIndex = 0;

        if (_currentItemIndex < minimumValidIndex || _currentItemIndex >= GetCurrentItemCount())
        {
            return null;
        }

        return _itemsByCurrency[_currentCurrency][_currentItemIndex];
    }

    private int GetCurrentItemCount()
    {
        int emptyCount = 0;

        return _itemsByCurrency.ContainsKey(_currentCurrency) ? _itemsByCurrency[_currentCurrency].Count : emptyCount;
    }

    public void Reset()
    {
        _currentCurrency = WalletManager.CoinType.Bronze;
        _currentItemIndex = -1;
        _lastMoveTime = 0f;
    }
}
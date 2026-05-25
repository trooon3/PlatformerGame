using GameLogic;
using UnityEngine;

public interface IShopItem
{
    string ItemId { get; }
    string DisplayName { get; }
    string Description { get; }
    int Price { get; }

    WalletManager.CoinType CurrencyType { get; }

    bool IsSold { get; set; }
    bool CanBePurchased();
    void Purchase();
}
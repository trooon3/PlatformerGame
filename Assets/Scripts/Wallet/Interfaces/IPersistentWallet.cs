using System;

public interface IPersistentWallet
{
    CoinData CurrentCoins { get; }

    event Action<CoinData> OnCoinsUpdated;

    void AddCoins(string coinType, int amount);
    void LoadCoinsFromSave(CoinData savedCoins);
    void SaveCoins();
    void LoadCoins();
}
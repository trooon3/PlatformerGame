using System;

[Serializable]
public struct CoinData
{
    private const int minimumCountBronzeCoin = 0; 
    private const int minimumCountSilverCoin = 0; 
    private const int minimumCountGoldCoin = 0; 

    public int bronze;
    public int silver;
    public int gold;
    public bool isInitialized;

    public CoinData(int bronze = minimumCountBronzeCoin, int silver = minimumCountSilverCoin, int gold = minimumCountGoldCoin)
    {
        this.bronze = bronze;
        this.silver = silver;
        this.gold = gold;

        this.isInitialized = true;
    }

    public static CoinData Empty => new CoinData(minimumCountBronzeCoin, minimumCountSilverCoin, minimumCountGoldCoin) { isInitialized = false };
}
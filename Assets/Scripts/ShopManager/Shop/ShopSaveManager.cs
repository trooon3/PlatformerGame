using Player.Abilities;
using UnityEngine;

public sealed class ShopSaveManager : MonoBehaviour
{
    private static ShopSaveManager _instance;
    public static ShopSaveManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnItemPurchased(string itemId, AbilityManager abilityManager)
    {
         int purchasedFlag = 1;

        PlayerPrefs.SetInt($"ShopItem_{itemId}_Purchased", purchasedFlag);
        PlayerPrefs.Save();

        if (abilityManager != null && SaveSystem.Instance != null)
        {
            SaveSystem.Instance.UpdateAbilityData(abilityManager);
        }
    }

    public bool IsItemPurchased(string itemId)
    {
         int purchasedValue = 1;
         int notPurchasedValue = 0;

        if (PlayerPrefs.HasKey($"ShopItem_{itemId}_Purchased"))
        {
            return PlayerPrefs.GetInt($"ShopItem_{itemId}_Purchased", notPurchasedValue) == purchasedValue;
        }

        if (SaveSystem.Instance != null && SaveSystem.Instance.CurrentSave?.purchasedItemIds != null)
        {
            return SaveSystem.Instance.CurrentSave.purchasedItemIds.Contains(itemId);
        }

        return false;
    }

    public void LoadAllPurchases()
    {
        if (SaveSystem.Instance == null || SaveSystem.Instance.CurrentSave == null)
        {
            return;
        }
    }

    public void ResetAllPurchases()
    {
        foreach (var key in PlayerPrefs.GetString("ShopItemKeys", "").Split(','))
        {
            if (!string.IsNullOrEmpty(key))
            {
                PlayerPrefs.DeleteKey($"ShopItem_{key}_Purchased");
            }
        }

        PlayerPrefs.DeleteKey("ShopItemKeys");
        PlayerPrefs.Save();
    }
}
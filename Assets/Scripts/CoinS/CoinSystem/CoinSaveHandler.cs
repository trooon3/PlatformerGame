using GameLogic;
using System.Collections;
using UnityEngine;

public sealed class CoinSaveHandler : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(InitializeCoins());
    }

    private IEnumerator InitializeCoins()
    {
        float initializationDelay = 0.3f;

        yield return new WaitForSeconds(initializationDelay);

        if (SaveSystem.Instance == null || !SaveSystem.Instance.HasSave())
        {
            yield break;
        }

        var saveData = SaveSystem.Instance.CurrentSave;

        if (saveData?.coins == null)
        {
            yield break;
        }

        InitializeFromSave(saveData.coins);
    }

    private void InitializeFromSave(CoinData savedCoins)
    {
        if (PersistentWallet.Instance != null)
        {
            PersistentWallet.Instance.LoadFromSaveData(savedCoins);
        }
        else if (WalletManager.Instance != null)
        {
            WalletManager.Instance.LoadFromSaveData(savedCoins);
        }
    }
}
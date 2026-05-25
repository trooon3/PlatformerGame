using System.Collections;
using UnityEngine;

public sealed class CoinSystemInitializer : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(InitializeCoins());
    }

    private IEnumerator InitializeCoins()
    {
        yield return new WaitForSeconds(0.3f);

        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave())
        {
            var saveData = SaveSystem.Instance.CurrentSave;

            if (saveData?.coins != null)
            {
                InitializeFromSave(saveData.coins);

                yield break;
            }
        }

        InitializeFromPlayerPrefs();
    }

    private void InitializeFromSave(CoinData savedCoins)
    {
        if (PersistentWallet.Instance != null)
        {
            PersistentWallet.Instance.LoadCoinsFromSave(savedCoins);
        }
    }

    private void InitializeFromPlayerPrefs()
    {
        if (PersistentWallet.Instance != null)
        {
        }
    }
}
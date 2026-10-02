using System;
using UnityEngine;

public static class LocalizationManager
{
    private const string LanguageKey = "GameLanguage";

    public enum Language { Russian, English, Turkish }

    public static Language CurrentLanguage { get; private set; }

    public static event Action<Language> OnLanguageChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadSavedLanguage()
    {
        int savedLang = PlayerPrefs.GetInt(LanguageKey, (int)Language.Russian);

        CurrentLanguage = (Language)savedLang;
    }

    public static void SetLanguage(Language newLanguage)
    {
        if (CurrentLanguage == newLanguage)
        {
            return;
        }

        CurrentLanguage = newLanguage;

        PlayerPrefs.SetInt(LanguageKey, (int)newLanguage);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke(CurrentLanguage);
    }
}
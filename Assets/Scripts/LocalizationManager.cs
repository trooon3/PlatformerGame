using System;
using UnityEngine;
using YG;

public static class LocalizationManager
{
    private const string LanguageKey = "GameLanguage";

    public enum Language { Russian, English, Turkish }

    public static Language CurrentLanguage { get; private set; }

    public static event Action<Language> OnLanguageChanged;

    private static bool _isSyncedWithYandex = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        int savedLang = PlayerPrefs.GetInt(LanguageKey, (int)Language.Russian);
        CurrentLanguage = (Language)savedLang;

        YG2.onGetSDKData += SyncWithYandex;
    }

    private static void SyncWithYandex()
    {
        if (_isSyncedWithYandex) return;
        _isSyncedWithYandex = true;

        string yandexLang = YG2.lang;
        Language newLang = Language.English;

        if (yandexLang == "ru") newLang = Language.Russian;
        else if (yandexLang == "tr") newLang = Language.Turkish;
        else if (yandexLang == "en") newLang = Language.English;

        CurrentLanguage = newLang;
        PlayerPrefs.SetInt(LanguageKey, (int)newLang);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke(CurrentLanguage);
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

        string ygLang = "en";
        if (newLanguage == Language.Russian) ygLang = "ru";
        else if (newLanguage == Language.Turkish) ygLang = "tr";

        YG2.SwitchLanguage(ygLang);
    }
}
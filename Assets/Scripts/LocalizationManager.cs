using System;
using UnityEngine;
using YG;

public static class LocalizationManager
{
    private const string LanguageKey = "GameLanguage";
    private const string ManualOverrideKey = "LangManualOverride";

    public enum Language { Russian = 0, English = 1, Turkish = 2 }

    public static Language CurrentLanguage { get; private set; }
    public static event Action<Language> OnLanguageChanged;

    private static bool _isApplyingLanguage = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (PlayerPrefs.GetInt(ManualOverrideKey, 0) == 1)
        {
            int savedLang = PlayerPrefs.GetInt(LanguageKey, (int)Language.Russian);
            CurrentLanguage = (Language)savedLang;
        }
        else
        {
            CurrentLanguage = CodeToLanguage(YG2.lang);
        }

        YG2.onGetSDKData += OnSDKDataReceived;
    }

    private static void OnSDKDataReceived()
    {
        if (PlayerPrefs.GetInt(ManualOverrideKey, 0) == 1)
        {
            int savedLang = PlayerPrefs.GetInt(LanguageKey, (int)Language.Russian);
            CurrentLanguage = (Language)savedLang;

            ApplyToYG(CurrentLanguage);
            OnLanguageChanged?.Invoke(CurrentLanguage);
        }
        else
        {
            CurrentLanguage = CodeToLanguage(YG2.lang);

            PlayerPrefs.SetInt(LanguageKey, (int)CurrentLanguage);
            PlayerPrefs.Save();

            ApplyToYG(CurrentLanguage);
            OnLanguageChanged?.Invoke(CurrentLanguage);
        }
    }

    public static void SetLanguage(Language newLanguage)
    {
        if (_isApplyingLanguage) return;

        CurrentLanguage = newLanguage;

        PlayerPrefs.SetInt(LanguageKey, (int)newLanguage);
        PlayerPrefs.SetInt(ManualOverrideKey, 1);
        PlayerPrefs.Save();

        ApplyToYG(newLanguage);
        OnLanguageChanged?.Invoke(CurrentLanguage);
    }

    private static void ApplyToYG(Language lang)
    {
        _isApplyingLanguage = true;
        string code = LanguageToCode(lang);

        if (YG2.lang != code)
        {
            YG2.SwitchLanguage(code);
        }
        else
        {
            YG2.SwitchLanguage(code);
        }
        _isApplyingLanguage = false;
    }

    private static Language CodeToLanguage(string code)
    {
        return code switch
        {
            "ru" => Language.Russian,
            "tr" => Language.Turkish,
            _ => Language.English
        };
    }

    private static string LanguageToCode(Language lang)
    {
        return lang switch
        {
            Language.Russian => "ru",
            Language.Turkish => "tr",
            _ => "en"
        };
    }

    public static string GetLocalizedVideoUrl(string fileNameRu, string fileNameEn, string fileNameTr = "")
    {
        string selectedFile = CurrentLanguage switch
        {
            Language.Russian => fileNameRu,
            Language.Turkish => string.IsNullOrEmpty(fileNameTr) ? fileNameEn : fileNameTr,
            _ => string.IsNullOrEmpty(fileNameEn) ? fileNameRu : fileNameEn
        };

        if (string.IsNullOrEmpty(selectedFile))
        {
            selectedFile = fileNameRu;
        }

        return Application.streamingAssetsPath + "/" + selectedFile.Trim();
    }
}
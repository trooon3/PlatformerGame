using UnityEngine;
using TMPro;

public sealed class LanguageSelector : MonoBehaviour
{
    [Header("Dropdown: 0 = Русский, 1 = English, 2 = Türkçe")]
    [SerializeField] private TMP_Dropdown _languageDropdown;

    private void OnEnable()
    {
        if (_languageDropdown != null)
        {
            _languageDropdown.SetValueWithoutNotify((int)LocalizationManager.CurrentLanguage);
            _languageDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        LocalizationManager.OnLanguageChanged += SyncDropdownUI;
    }

    private void OnDisable()
    {
        if (_languageDropdown != null)
        {
            _languageDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }

        LocalizationManager.OnLanguageChanged -= SyncDropdownUI;
    }

    private void SyncDropdownUI(LocalizationManager.Language lang)
    {
        if (_languageDropdown != null)
        {
            _languageDropdown.SetValueWithoutNotify((int)lang);
        }
    }

    private void OnDropdownChanged(int index)
    {
        LocalizationManager.SetLanguage((LocalizationManager.Language)index);
    }
}
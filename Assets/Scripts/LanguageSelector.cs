using UnityEngine;
using TMPro;

public sealed class LanguageSelector : MonoBehaviour
{
    [Header("Dropdown: 0 = Русский, 1 = English, 2 = Türkçe")]
    [SerializeField] private TMP_Dropdown _languageDropdown;

    private void Start()
    {
        if (_languageDropdown != null)
        {
            _languageDropdown.value = (int)LocalizationManager.CurrentLanguage;
            _languageDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
    }

    private void OnDestroy()
    {
        if (_languageDropdown != null)
        {
            _languageDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }
    }

    private void OnDropdownChanged(int index)
    {
        LocalizationManager.SetLanguage((LocalizationManager.Language)index);
    }
}
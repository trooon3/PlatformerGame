using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public sealed class LocalizedText : MonoBehaviour
{
    [TextArea(2, 5)][SerializeField] private string _russianText;
    [TextArea(2, 5)][SerializeField] private string _englishText;
    [TextArea(2, 5)][SerializeField] private string _turkishText; 

    private TMP_Text _textComponent;

    private void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += UpdateText;
        UpdateText(LocalizationManager.CurrentLanguage);
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }

    private void UpdateText(LocalizationManager.Language language)
    {
        if (_textComponent == null)
        {
            return;
        }

        switch (language)
        {
            case LocalizationManager.Language.Russian:
                _textComponent.text = _russianText;

                break;

            case LocalizationManager.Language.English:
                _textComponent.text = _englishText;

                break;

            case LocalizationManager.Language.Turkish:
                _textComponent.text = _turkishText;

                break;
        }
    }
}
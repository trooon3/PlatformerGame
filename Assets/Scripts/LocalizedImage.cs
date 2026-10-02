using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class LocalizedImage : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite _russianSprite;
    [SerializeField] private Sprite _englishSprite;
    [SerializeField] private Sprite _turkishSprite; 

    private Image _imageComponent;

    private void Awake()
    {
        _imageComponent = GetComponent<Image>();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += UpdateImage;
        UpdateImage(LocalizationManager.CurrentLanguage);
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateImage;
    }

    private void UpdateImage(LocalizationManager.Language language)
    {
        if (_imageComponent == null)
        {
            return;
        }

        switch (language)
        {
            case LocalizationManager.Language.Russian:
                if (_russianSprite != null)
                {
                    _imageComponent.sprite = _russianSprite;
                }

                break;

            case LocalizationManager.Language.English:
                if (_englishSprite != null)
                {
                    _imageComponent.sprite = _englishSprite;
                }

                break;

            case LocalizationManager.Language.Turkish:
                if (_turkishSprite != null)
                {
                    _imageComponent.sprite = _turkishSprite;
                }

                break;
        }
    }
}
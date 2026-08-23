using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class LocalizedSprite : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite _russianSprite;
    [SerializeField] private Sprite _englishSprite;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += UpdateSprite;

        UpdateSprite(LocalizationManager.CurrentLanguage);
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateSprite;
    }

    private void UpdateSprite(LocalizationManager.Language language)
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        switch (language)
        {
            case LocalizationManager.Language.Russian:
                if (_russianSprite != null)
                {
                    _spriteRenderer.sprite = _russianSprite;
                }

                break;

            case LocalizationManager.Language.English:
                if (_englishSprite != null)
                {
                    _spriteRenderer.sprite = _englishSprite;
                }

                break;
        }
    }
}
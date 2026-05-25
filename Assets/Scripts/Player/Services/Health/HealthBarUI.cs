using UnityEngine;
using UnityEngine.UI;

public sealed class HealthBarUI : MonoBehaviour
{
    private const float DefaultSpacing = 35f;

    [SerializeField] private Image _heartPrefab;
    [SerializeField] private Image _emptyHeartImage;
    [SerializeField] private int _maxHeartCount = 6;
    [SerializeField] private float _heartSpacing = DefaultSpacing;

    private Image[] _hearts;
    private bool _isInitialized = false;

    private void Start()
    {
        InitializeHearts();
    }

    private void InitializeHearts()
    {
        if (_isInitialized)
        {
            return;
        }

        _hearts = new Image[_maxHeartCount];

        for (int heartIndex = 0; heartIndex < _maxHeartCount; heartIndex++)
        {
            _hearts[heartIndex] = Instantiate(_heartPrefab, transform);
            _hearts[heartIndex].rectTransform.anchoredPosition = new Vector2(heartIndex * _heartSpacing, 0);
            _hearts[heartIndex].color = UnityEngine.Color.white;
        }

        _isInitialized = true;
    }

    public void SetHealth(int currentHealth)
    {
        InitializeHearts();

        for (int heartIndex = 0; heartIndex < _maxHeartCount; heartIndex++)
        {
            bool isHeartFull = heartIndex < currentHealth;

            _hearts[heartIndex].sprite = isHeartFull ? _heartPrefab.sprite : _emptyHeartImage.sprite;
            _hearts[heartIndex].color = UnityEngine.Color.white;
        }
    }

    public void ForceRefresh()
    {
        _isInitialized = false;

        InitializeHearts();
    }
}
using GeneralLogicEnemies;
using System.Reflection;
using UnityEngine;

public sealed class BossHealthBar : MonoBehaviour
{
    private const float HealthBarWidth = 300f;
    private const float HealthBarHeight = 30f;
    private const float HealthBarBorderThickness = 3f;
    private const float HealthBarPadding = 2f;
    private const float TextOffsetY = 25f;
    private const float FontSize = 16f;

    [SerializeField] private Vector2 _healthBarOffset = new Vector2(0f, 3f);

    private Camera _mainCamera;
    private Entity _bossEntity;
    private FieldInfo _livesFieldInfo;
    private int _maxHealth;

    private void Start()
    {
        InitializeComponents();
        InitializeHealthData();
    }

    private void Update()
    {
        EnsureCameraReference();
    }

    private void OnGUI()
    {
        if (!ShouldRenderHealthBar())
        {
            return;
        }

        RenderHealthBar();
    }

    private void InitializeComponents()
    {
        _bossEntity = GetComponent<Entity>();
        _mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
    }

    private void InitializeHealthData()
    {
        _livesFieldInfo = typeof(Entity).GetField("lives", BindingFlags.NonPublic | BindingFlags.Instance);

        if (_bossEntity != null && _livesFieldInfo != null)
        {
            _maxHealth = (int)_livesFieldInfo.GetValue(_bossEntity);
        }
    }

    private void EnsureCameraReference()
    {
        if (_mainCamera == null)
        {
            _mainCamera = FindFirstObjectByType<Camera>();
        }
    }

    private bool ShouldRenderHealthBar()
    {
        return _mainCamera != null && _bossEntity != null;
    }

    private void RenderHealthBar()
    {
        int currentHealth = GetCurrentHealth();
        Vector2 screenPosition = CalculateScreenPosition();

        if (!IsPositionOnScreen(screenPosition))
        {
            return;
        }

        Rect healthBarRect = CalculateHealthBarRect(screenPosition);
        float healthPercentage = CalculateHealthPercentage(currentHealth);

        DrawHealthBar(healthBarRect, healthPercentage);
        DrawHealthText(healthBarRect, currentHealth);
    }

    private int GetCurrentHealth()
    {
        int fallbackHealthValue = 0;

        if (_bossEntity != null && _livesFieldInfo != null)
        {
            return (int)_livesFieldInfo.GetValue(_bossEntity);
        }

        return fallbackHealthValue;
    }

    private Vector2 CalculateScreenPosition()
    {
        float zeroZOffset = 0f;

        Vector3 worldPosition = transform.position + new Vector3(_healthBarOffset.x, _healthBarOffset.y, zeroZOffset);
        Vector3 screenPosition = _mainCamera.WorldToScreenPoint(worldPosition);

        return new Vector2(screenPosition.x, Screen.height - screenPosition.y);
    }

    private bool IsPositionOnScreen(Vector2 screenPosition)
    {
         float minXBoundary = 0f;
         float minYBoundary = 0f;

        return screenPosition.x >= minXBoundary && screenPosition.x <= Screen.width && screenPosition.y >= minYBoundary && screenPosition.y <= Screen.height;
    }

    private Rect CalculateHealthBarRect(Vector2 screenPosition)
    {
        float positionOffset = 2f;

        float xPosition = screenPosition.x - HealthBarWidth / positionOffset;

        float yPosition = screenPosition.y - HealthBarHeight / positionOffset;

        return new Rect(xPosition, yPosition, HealthBarWidth, HealthBarHeight);
    }

    private float CalculateHealthPercentage(int currentHealth)
    {
        int minimumHealthForCalculation = 0;
        float minClampValue = 0f;

        return _maxHealth > minimumHealthForCalculation ? Mathf.Clamp01((float)currentHealth / _maxHealth) : minClampValue;
    }

    private void DrawHealthBar(Rect healthBarRect, float healthPercentage)
    {
        DrawHealthBarBackground(healthBarRect);
        DrawHealthBarFill(healthBarRect, healthPercentage);
        DrawHealthBarBorder(healthBarRect);
    }

    private void DrawHealthBarBackground(Rect healthBarRect)
    {
        DrawRectangle(healthBarRect, Color.black);
    }

    private void DrawHealthBarFill(Rect healthBarRect, float healthPercentage)
    {
        float minimumHealthPercentage = 0f;
        float paddingMultiplier = 2f;

        if (healthPercentage <= minimumHealthPercentage)
        {
            return;
        }

        float fillWidth = (healthBarRect.width - HealthBarPadding * paddingMultiplier) * healthPercentage;

        Rect fillRect = new Rect(
            healthBarRect.x + HealthBarPadding,
            healthBarRect.y + HealthBarPadding,
            fillWidth,
            healthBarRect.height - HealthBarPadding * paddingMultiplier
        );

        DrawRectangle(fillRect, Color.red);
    }

    private void DrawHealthBarBorder(Rect healthBarRect)
    {
        DrawRectangle(new Rect(healthBarRect.x, healthBarRect.y, healthBarRect.width, HealthBarBorderThickness), Color.white);
        DrawRectangle(new Rect(healthBarRect.x, healthBarRect.y + healthBarRect.height - HealthBarBorderThickness, healthBarRect.width, HealthBarBorderThickness), Color.white);
        DrawRectangle(new Rect(healthBarRect.x, healthBarRect.y, HealthBarBorderThickness, healthBarRect.height), Color.white);
        DrawRectangle(new Rect(healthBarRect.x + healthBarRect.width - HealthBarBorderThickness, healthBarRect.y, HealthBarBorderThickness, healthBarRect.height), Color.white);
    }

    private void DrawHealthText(Rect healthBarRect, int currentHealth)
    {
        Rect textRect = new Rect(healthBarRect.x, healthBarRect.y - TextOffsetY, healthBarRect.width, TextOffsetY);

        GUIStyle textStyle = CreateHealthTextStyle();

        string healthText = $"{gameObject.name}: {currentHealth}/{_maxHealth}";

        GUI.Label(textRect, healthText, textStyle);
    }

    private GUIStyle CreateHealthTextStyle()
    {
        return new GUIStyle
        {
            normal = { textColor = Color.yellow },
            fontSize = (int)FontSize,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
    }

    private void DrawRectangle(Rect rect, Color color)
    {
        Texture2D texture = CreateSolidTexture(color);

        GUI.DrawTexture(rect, texture);
    }

    private Texture2D CreateSolidTexture(Color color)
    {
         int textureWidth = 1;
         int textureHeight = 1;

         int pixelX = 0;
         int pixelY = 0;

        Texture2D texture = new Texture2D(textureWidth, textureHeight);
        texture.SetPixel(pixelX, pixelY, color);
        texture.Apply();

        return texture;
    }
}
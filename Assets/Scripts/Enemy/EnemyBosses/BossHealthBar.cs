using GeneralLogicEnemies;
using UnityEngine;

public sealed class BossHealthBar : MonoBehaviour
{
    private const float HealthBarWidth = 300f;
    private const float HealthBarHeight = 30f;
    private const float HealthBarBorderThickness = 3f;
    private const float HealthBarPadding = 2f;
    private const float TextOffsetY = 25f;
    private const int FontSize = 16;

    [SerializeField] private Vector2 _healthBarOffset = new Vector2(0f, 3f);

    private Camera _mainCamera;
    private Entity _bossEntity;
    private int _maxHealth;

    private void Start()
    {
        InitializeComponents();
        InitializeHealthData();
    }

    private void Update()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
        }
    }

    private void OnGUI()
    {
        if (ShouldRenderHealthBar() == false)
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
        if (_bossEntity == null)
        {
            _maxHealth = 1;

            return;
        }

        _maxHealth = Mathf.Max(1, _bossEntity.CurrentLives);
    }

    private bool ShouldRenderHealthBar()
    {
        return _mainCamera != null &&
               _bossEntity != null &&
               _bossEntity.IsDead == false;
    }

    private void RenderHealthBar()
    {
        int currentHealth = Mathf.Max(0, _bossEntity.CurrentLives);
        Vector2 screenPosition = CalculateScreenPosition();

        if (IsPositionOnScreen(screenPosition) == false)
        {
            return;
        }

        Rect healthBarRect = CalculateHealthBarRect(screenPosition);
        float healthPercentage = CalculateHealthPercentage(currentHealth);

        DrawHealthBar(healthBarRect, healthPercentage);
        DrawHealthText(healthBarRect, currentHealth);
    }

    private Vector2 CalculateScreenPosition()
    {
        Vector3 worldPosition = transform.position + new Vector3(_healthBarOffset.x, _healthBarOffset.y, 0f);
        Vector3 screenPosition = _mainCamera.WorldToScreenPoint(worldPosition);

        return new Vector2(screenPosition.x, Screen.height - screenPosition.y);
    }

    private bool IsPositionOnScreen(Vector2 screenPosition)
    {
        return screenPosition.x >= 0f &&
               screenPosition.x <= Screen.width &&
               screenPosition.y >= 0f &&
               screenPosition.y <= Screen.height;
    }

    private Rect CalculateHealthBarRect(Vector2 screenPosition)
    {
        return new Rect(
            screenPosition.x - HealthBarWidth / 2f,
            screenPosition.y - HealthBarHeight / 2f,
            HealthBarWidth,
            HealthBarHeight);
    }

    private float CalculateHealthPercentage(int currentHealth)
    {
        return _maxHealth > 0
            ? Mathf.Clamp01((float)currentHealth / _maxHealth)
            : 0f;
    }

    private void DrawHealthBar(Rect healthBarRect, float healthPercentage)
    {
        DrawRectangle(healthBarRect, Color.black);
        DrawHealthBarFill(healthBarRect, healthPercentage);
        DrawHealthBarBorder(healthBarRect);
    }

    private void DrawHealthBarFill(Rect healthBarRect, float healthPercentage)
    {
        if (healthPercentage <= 0f)
        {
            return;
        }

        float fillWidth = (healthBarRect.width - HealthBarPadding * 2f) * healthPercentage;

        Rect fillRect = new Rect(
            healthBarRect.x + HealthBarPadding,
            healthBarRect.y + HealthBarPadding,
            fillWidth,
            healthBarRect.height - HealthBarPadding * 2f);

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
        Rect textRect = new Rect(
            healthBarRect.x,
            healthBarRect.y - TextOffsetY,
            healthBarRect.width,
            TextOffsetY);

        GUI.Label(textRect, $"{currentHealth}/{_maxHealth}", CreateHealthTextStyle());
    }

    private GUIStyle CreateHealthTextStyle()
    {
        return new GUIStyle
        {
            normal = { textColor = Color.yellow },
            fontSize = FontSize,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
    }

    private void DrawRectangle(Rect rect, Color color)
    {
        Texture2D texture = Texture2D.whiteTexture;
        Color previousColor = GUI.color;

        GUI.color = color;
        GUI.DrawTexture(rect, texture);
        GUI.color = previousColor;
    }
}
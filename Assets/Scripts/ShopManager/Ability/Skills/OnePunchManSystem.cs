using GeneralLogicEnemies;
using Player.Abilities;
using TMPro; 
using UnityEngine;

public sealed class OnePunchManSystem : MonoBehaviour, IOnePunchManSystem
{
    private const float DefaultInstakillChance = 0.5f;
    private const float DefaultInstakillTextHeight = 1f;
    private const float DefaultTextLifetime = 2f;
    private const int InstakillFontSize = 16;

    [SerializeField] private float _instakillChance = DefaultInstakillChance;
    [SerializeField] private bool _showInstakillEffect = true;
    [SerializeField] private AudioClip _instakillSound;

    [Header("Visuals")]
    [SerializeField] private TMP_FontAsset _textFont; 

    private Hero _hero;
    private AbilityManager _abilityManager;
    private AudioController _audioController;
    private bool _isActive;

    public float InstakillChance => _instakillChance;
    public bool IsActive => _isActive;

    private void Awake()
    {
        InitializeReferences();
    }

    private void Start()
    {
        CheckIfAbilityPurchased();
    }

    public void Activate()
    {
        _isActive = true;

        RefreshAbilityManagerReference();

        if (_abilityManager != null)
        {
            _abilityManager.HasOnePunchManAbility = true;
        }
    }

    public void Deactivate()
    {
        _isActive = false;
    }

    public bool CheckForInstakill(Entity enemy)
    {
        if (_isActive == false || enemy == null)
        {
            return false;

        }

        if (Random.value > _instakillChance)
        {
            return false;
        }

        PerformInstakill(enemy);

        return true;
    }

    private void InitializeReferences()
    {
        _hero = GetComponent<Hero>() ?? FindFirstObjectByType<Hero>();
        _audioController = GetComponent<AudioController>() ?? GetComponentInChildren<AudioController>();
        _abilityManager = _hero?.AbilityManager;
    }

    private void CheckIfAbilityPurchased()
    {
        RefreshAbilityManagerReference();

        _isActive = _abilityManager?.HasOnePunchManAbility ?? false;
    }

    private void RefreshAbilityManagerReference()
    {
        if (_abilityManager == null && _hero != null)
        {
            _abilityManager = _hero.AbilityManager;
        }
    }

    private void PerformInstakill(Entity enemy)
    {
        if (_instakillSound != null && _audioController != null)
        {
            _audioController.PlayOneShot(_instakillSound);
        }

        if (_showInstakillEffect)
        {
            Vector3 effectPosition = enemy.transform.position;
            ShowFloatingText(effectPosition);
        }

        enemy.Die();
    }


    private void ShowFloatingText(Vector3 position)
    {
        GameObject textObject = new GameObject("InstakillText");
        textObject.transform.position = position + Vector3.up * DefaultInstakillTextHeight;

        TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();

        bool isEnglish = LocalizationManager.CurrentLanguage == LocalizationManager.Language.English;

        textMesh.text = isEnglish ? "InstaKill" : "Уничтожен";

        textMesh.color = Color.red;
        textMesh.fontSize = InstakillFontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold | FontStyles.Italic;
        textMesh.sortingOrder = 10000;

        if (_textFont != null)
        {
            textMesh.font = _textFont;
        }

        Rigidbody2D rb = textObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = -0.5f; 

        Destroy(textObject, DefaultTextLifetime);
    }
}
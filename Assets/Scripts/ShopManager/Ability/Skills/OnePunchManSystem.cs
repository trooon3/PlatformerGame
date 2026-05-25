using GeneralLogicEnemies;
using Player.Abilities;
using UnityEngine;

public sealed class OnePunchManSystem : MonoBehaviour, IOnePunchManSystem
{
    private const float DefaultInstakillChance = 0.5f;
    private const float DefaultInstakillTextHeight = 1f;
    private const float DefaultTextLifetime = 2f;

    [SerializeField] private float _instakillChance = DefaultInstakillChance;
    [SerializeField] private bool _showInstakillEffect = true;
    [SerializeField] private AudioClip _instakillSound;

    private Hero _hero;
    private AbilityManager _abilityManager;
    private AudioController _audioController;
    private bool _isActive = false;

    public float InstakillChance => _instakillChance;
    public bool IsActive => _isActive;

    private void Awake()
    {
        _hero = GetComponent<Hero>();

        if (_hero == null)
        {
            _hero = FindObjectOfType<Hero>();
        }

        _audioController = GetComponent<AudioController>() ?? GetComponentInChildren<AudioController>();
    }

    private void Start()
    {
        CheckIfAbilityPurchased();
    }

    private void CheckIfAbilityPurchased()
    {
        if (_hero != null && _abilityManager == null)
        {
            _abilityManager = _hero.AbilityManager;
        }

        _isActive = (_abilityManager?.HasOnePunchManAbility ?? false);
    }

    public void Activate()
    {
        _isActive = true;

        if (_abilityManager == null && _hero != null)
        {
            _abilityManager = _hero.AbilityManager;
        }

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
        float minRandomValue = 0f;
        float maxRandomValue = 1f;

        if (!_isActive || enemy == null)
        {
            return false;
        }

        float randomValue = Random.Range(minRandomValue, maxRandomValue);

        if (randomValue <= _instakillChance)
        {
            PerformInstakill(enemy);

            return true;
        }

        return false;
    }

    private void PerformInstakill(Entity enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (_showInstakillEffect)
        {
            ShowInstakillEffect(enemy.transform.position);
        }
    }

    private void ShowInstakillEffect(Vector3 position)
    {
        int fontSize = 16;

        GameObject textObj = new GameObject("InstakillEffect");

        textObj.transform.position = position + Vector3.up * DefaultInstakillTextHeight;

        TextMesh textMesh = textObj.AddComponent<TextMesh>();

        textMesh.text = "OneShot";
        textMesh.color = Color.magenta;
        textMesh.fontSize = fontSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.fontStyle = FontStyle.Bold;

        Destroy(textObj, DefaultTextLifetime);
    }
}
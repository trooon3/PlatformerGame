using GeneralLogicEnemies;
using Player.Abilities;
using UnityEngine;

public sealed class VampireHealthSystem : MonoBehaviour, IVampireHealthSystem
{
    private const int DefaultHealthPerKill = 1;
    private const float CheckInterval = 2f;

    [SerializeField] private int _healthPerKill = DefaultHealthPerKill;
    [SerializeField] private bool _showHealEffect = true;

    private Hero _hero;
    private HealthManager _healthManager;
    private AbilityManager _abilityManager;
    private bool _isActive = false;
    private float _checkTimer = 0f;

    public int HealthPerKill => _healthPerKill;
    public bool IsActive => _isActive;

    private void Awake()
    {
        _hero = GetComponent<Hero>();
        _healthManager = GetComponent<HealthManager>();
    }

    private void Start()
    {
        CheckIfAbilityPurchased();
    }

    private void OnEnable()
    {
        if (_isActive)
        {
            SubscribeToAllEnemies();
        } 
    }

    private void OnDisable()
    {
        UnsubscribeFromAllEnemies();
    }

    private void Update()
    {
        if (!_isActive)
        {
            return;
        }

        _checkTimer += Time.deltaTime;

        if (_checkTimer >= CheckInterval)
        {
            _checkTimer = 0f;

            SubscribeToAllEnemies();
        }
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
            _abilityManager.HasVampireAbility = true;
        }

        SubscribeToAllEnemies();
    }

    private void CheckIfAbilityPurchased()
    {
        if (_hero != null && _abilityManager == null)
        {
            _abilityManager = _hero.AbilityManager;
        }

        _isActive = (_abilityManager?.HasVampireAbility ?? false);

        if (_isActive)
        {
            SubscribeToAllEnemies();
        }
    }

    public void Deactivate()
    {
        _isActive = false;

        UnsubscribeFromAllEnemies();
    }

    private void SubscribeToAllEnemies()
    {
        Entity[] allEnemies = FindObjectsOfType<Entity>();

        foreach (Entity enemy in allEnemies)
        {
            SubscribeToEnemy(enemy);
        }
    }

    private void UnsubscribeFromAllEnemies()
    {
        Entity[] allEnemies = FindObjectsOfType<Entity>();

        foreach (Entity enemy in allEnemies)
        {
            if (enemy != null)
            {
                enemy.OnEntityDeath -= OnEnemyKilled;
            }
        }
    }

    private void SubscribeToEnemy(Entity enemy)
    {
        if (enemy != null)
        {
            enemy.OnEntityDeath -= OnEnemyKilled;
            enemy.OnEntityDeath += OnEnemyKilled;
        }
    }

    private void OnEnemyKilled(Entity enemy)
    {
        if (!_isActive || _healthManager == null || _hero == null)
        {
            return;
        }

        if (_healthManager.IsFullHealth || !_hero.IsAlive())
        {
            return;
        }

        _healthManager.Heal(_healthPerKill);
    }
}
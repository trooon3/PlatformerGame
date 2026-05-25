using Player.Abilities;
using UnityEngine;

public sealed class PassiveHealthRegeneration : MonoBehaviour, IPassiveHealthRegeneration
{
    private const float DefaultRegenerationInterval = 5f;
    private const int DefaultHealAmount = 1;

    [SerializeField] private float _regenerationInterval = DefaultRegenerationInterval;
    [SerializeField] private int _healAmount = DefaultHealAmount;

    private Hero _hero;
    private HealthManager _healthManager;
    private AbilityManager _abilityManager;
    private float _timer;

    public float RegenerationInterval => _regenerationInterval;
    public int HealAmount => _healAmount;
    public bool IsActive => _abilityManager?.HasPassiveHealthRegeneration ?? false;

    private void Awake()
    {
        _hero = GetComponent<Hero>() ?? FindObjectOfType<Hero>();
        _healthManager = GetComponent<HealthManager>() ?? FindObjectOfType<HealthManager>();
    }

    private void Start()
    {
        InitializeReferences();
    }

    private void Update()
    {
        if (_abilityManager == null)
        {
            _abilityManager = _hero?.AbilityManager;
        }

        if (_healthManager == null)
        {
            return;
        }

        if (!IsActive || _healthManager.IsFullHealth || _hero == null || !_hero.IsAlive())
        {
            return;
        }

        _timer += Time.deltaTime;

        if (_timer >= _regenerationInterval)
        {
            _timer = 0f;

            _healthManager.Heal(_healAmount);
        }
    }

    private void InitializeReferences()
    {
        _hero ??= GetComponent<Hero>() ?? FindObjectOfType<Hero>();
        _healthManager ??= GetComponent<HealthManager>() ?? FindObjectOfType<HealthManager>();

        _abilityManager ??= _hero?.AbilityManager;
    }

    public void EnableRegeneration()
    {
        enabled = true;
    }

    public void DisableRegeneration()
    {
        enabled = false;

        _timer = 0f;
    }
}
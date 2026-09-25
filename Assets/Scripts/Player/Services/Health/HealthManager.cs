using UnityEngine;

public sealed class HealthManager : MonoBehaviour
{
    private const int MinimumHealth = 0;

    [SerializeField] private int _maxHealth = 6;
    [SerializeField] private HealthBarUI _healthBarUI;
    [SerializeField] private ArmorManager _armorManager;
    [SerializeField] private SfxPlayer _sfxPlayer;
    [SerializeField] private SoundConfiguration _soundConfiguration;

    [Header("Effects")]
    [SerializeField] private LowHealthEffect _lowHealthEffect;

    public static event System.Action<int> OnDamageTakenGlobal;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => _maxHealth;
    public bool IsFullHealth => CurrentHealth >= _maxHealth;

    public event System.Action<int> OnHealthChanged;
    public event System.Action OnDeath;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        if (CurrentHealth <= MinimumHealth)
        {
            CurrentHealth = _maxHealth;
        }

        UpdateHealthUI();
    }

    public void SetHealth(int health)
    {
        int newHealth = Mathf.Clamp(health, MinimumHealth, _maxHealth);

        if (CurrentHealth == newHealth)
        {
            UpdateHealthUI();

            return;
        }

        CurrentHealth = newHealth;

        OnHealthChanged?.Invoke(CurrentHealth);
        UpdateHealthUI();

        if (CurrentHealth <= MinimumHealth)
        {
            OnDeath?.Invoke();
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (damageAmount <= MinimumHealth || CurrentHealth <= MinimumHealth)
        {
            return;
        }

        OnDamageTakenGlobal?.Invoke(damageAmount);

        int remainingDamage = damageAmount;

        if (_armorManager != null && _armorManager.HasArmor)
        {
            remainingDamage = _armorManager.TakeArmorDamage(damageAmount);
        }

        if (remainingDamage <= MinimumHealth)
        {
            return;
        }

        SetHealth(CurrentHealth - remainingDamage);

        if (_sfxPlayer != null && _soundConfiguration != null)
            _sfxPlayer.Play(_soundConfiguration.TakeDamageSound);
    }

    public void Heal(int healAmount)
    {
        if (healAmount <= MinimumHealth)
        {
            return;
        }

        SetHealth(CurrentHealth + healAmount);
    }

    public void FullHeal()
    {
        SetHealth(_maxHealth);
    }

    public void AddArmor(int armorAmount)
    {
        _armorManager?.AddArmor(armorAmount);
    }

    public void FillArmor()
    {
        _armorManager?.FillArmor();
    }

    public void ResetArmor()
    {
        _armorManager?.ResetArmor();
    }

    public void RestoreOneArmor()
    {
        _armorManager?.AddArmor(1);
    }

    private void InitializeComponents()
    {
        if (_healthBarUI == null)
        {
            _healthBarUI = FindFirstObjectByType<HealthBarUI>();
        }

        if (_armorManager == null)
        {
            _armorManager = GetComponent<ArmorManager>();

            if (_armorManager == null)
            {
                _armorManager = FindFirstObjectByType<ArmorManager>();
            }
        }

        if (_sfxPlayer == null)
        {
            _sfxPlayer = GetComponent<SfxPlayer>();
            if (_sfxPlayer == null)
                _sfxPlayer = FindFirstObjectByType<SfxPlayer>();
        }

        if (_lowHealthEffect == null)
        {
            _lowHealthEffect = FindFirstObjectByType<LowHealthEffect>();
        }
    }

    private void UpdateHealthUI()
    {
        _healthBarUI?.SetHealth(CurrentHealth);

        _lowHealthEffect?.UpdateHealthState(CurrentHealth, _maxHealth);
    }
}
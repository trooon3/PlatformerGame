using UnityEngine;

public sealed class HealthManager : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 6;
    [SerializeField] private HealthBarUI _healthBarUI;
    [SerializeField] private ArmorManager _armorManager;
    [SerializeField] private AudioController _audioController;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => _maxHealth;
    public bool IsFullHealth => CurrentHealth >= MaxHealth;

    public event System.Action<int> OnHealthChanged;
    public event System.Action OnDeath;

    private Hero _hero;
    private AudioSource _audioSource;

    private void Awake()
    {
        if (_audioController == null)
        {
            _audioController = GetComponent<AudioController>();

            if (_audioController == null)
            {
                _audioController = FindObjectOfType<AudioController>();
            }
        }

        InitializeComponents();
        FindHero();
    }

    private void Start()
    {
        if (CurrentHealth == 0)
        {
            CurrentHealth = _maxHealth;
        }

        UpdateHealthUI();
    }

    private void InitializeComponents()
    {
        if (_audioController == null)
        {
            _audioController = GetComponent<AudioController>();

            if (_audioController == null)
            {
                _audioController = FindObjectOfType<AudioController>();
            }
        }
    }

    private void FindHero()
    {
        _hero = GetComponent<Hero>();

        if (_hero == null)
        {
            _hero = FindObjectOfType<Hero>();
        }
    }

    public void SetHealth(int health)
    {
         int minimumHealth = 0;
         int deathThreshold = 0;

        int newHealth = Mathf.Clamp(health, minimumHealth, _maxHealth);

        if (newHealth == CurrentHealth)
        {
            return;
        }

        CurrentHealth = newHealth;
        OnHealthChanged?.Invoke(CurrentHealth);

        UpdateHealthUI();

        if (CurrentHealth <= deathThreshold)
        {
            OnDeath?.Invoke();
        }
    }

    public void TakeDamage(int damageAmount)
    {
        int remainingDamage = damageAmount;

        int noDamageRemaining = 0;

        if (_armorManager != null && _armorManager.HasArmor)
        {
            remainingDamage = _armorManager.TakeArmorDamage(damageAmount);

            if (remainingDamage > noDamageRemaining)
            {
                SetHealth(CurrentHealth - remainingDamage);

                _audioController?.PlayTakeDamageSound();
            }
        }
        else
        {
            SetHealth(CurrentHealth - damageAmount);

            _audioController?.PlayTakeDamageSound();
        }
    }

    public void Heal(int healAmount)
    {
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

    private void UpdateHealthUI()
    {
        _healthBarUI?.SetHealth(CurrentHealth);
    }
}
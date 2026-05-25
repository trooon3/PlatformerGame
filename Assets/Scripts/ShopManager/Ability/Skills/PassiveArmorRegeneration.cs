using Player.Abilities;
using UnityEngine;

public sealed class PassiveArmorRegeneration : MonoBehaviour, IPassiveArmorRegeneration
{
    private const float DefaultRegenInterval = 10f;
    private const int DefaultRegenAmount = 1;
    private const float CheckInterval = 1f;

    [SerializeField] private float _regenInterval = DefaultRegenInterval;
    [SerializeField] private int _regenAmount = DefaultRegenAmount;

    private Hero _hero;
    private IArmorManager _armorManager;
    private AbilityManager _abilityManager;
    private float _timer;
    private bool _isActive = false;
    private float _checkTimer;

    public float RegenInterval => _regenInterval;
    public int RegenAmount => _regenAmount;
    public bool IsActive => _isActive;

    private void Awake()
    {
        _hero = GetComponent<Hero>() ?? FindObjectOfType<Hero>();

        _armorManager = GetComponent<IArmorManager>() ?? GetComponent<ArmorManager>() as IArmorManager ?? FindObjectOfType<ArmorManager>() as IArmorManager;
    }

    private void Update()
    {
        if (!_isActive)
        {
            _checkTimer += Time.deltaTime;

            if (_checkTimer >= CheckInterval)
            {
                _checkTimer = 0f;

                CheckIfAbilityPurchased();
            }

            return;
        }

        if (_armorManager == null || _hero == null)
        {
            return;
        }

        if (!_armorManager.IsArmorUnlocked() || _armorManager.CurrentArmor >= _armorManager.MaxArmor)
        {
            return;
        } 

        _timer += Time.deltaTime;

        if (_timer >= _regenInterval)
        {
            _timer = 0f;

            _armorManager.AddArmor(_regenAmount);
        }
    }

    private void CheckIfAbilityPurchased()
    {
        _isActive = _hero?.AbilityManager?.HasRobocopRegeneration ?? false;
    }

    public void Activate()
    {
        _isActive = true;

        _timer = 0f;
    }

    public void Deactivate()
    {
        _isActive = false;
    }
}
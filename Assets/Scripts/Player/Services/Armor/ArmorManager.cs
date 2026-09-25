using UnityEngine;
using UnityEngine.UI;

public sealed class ArmorManager : MonoBehaviour, IArmorManager
{
    private const string ArmorPanelName = "ArmorPanel_Runtime";
    private const int MinimumArmorValue = 0;
    private const float DisabledArmorAlpha = 0.3f;

    [Header("Armor Settings")]
    [SerializeField] private int _maxArmor = 6;
    [SerializeField] private int _currentArmor = 6;
    [SerializeField] private bool _startEnabled = false;

    [Header("UI References")]
    [SerializeField] private GameObject _armorPanelPrefab;

    [Header("Sound Settings")]
    [SerializeField] private bool _useAudioController = true;
    [SerializeField] private SfxPlayer _sfxPlayer;

    [Header("Sound Configuration")]
    [SerializeField] private SoundConfiguration _soundConfiguration;

    private GameObject _armorPanelInstance;
    private Image[] _armorIcons;
    private Hero _hero;
    private bool _isArmorUnlocked;

    public event System.Action<int, int> OnArmorChanged;

    public int CurrentArmor => _currentArmor;
    public int MaxArmor => _maxArmor;
    public bool HasArmor => IsArmorUnlocked() && _currentArmor > MinimumArmorValue;

    private void Start()
    {
        InitializeReferences();
        InitializeArmorState();
    }

    public bool IsArmorUnlocked()
    {
        return _isArmorUnlocked ||
               (_hero != null &&
                _hero.AbilityManager != null &&
                _hero.AbilityManager.HasArmor);
    }

    public void SetArmor(int amount)
    {
        if (IsArmorUnlocked() == false)
        {
            return;
        }

        int validArmor = Mathf.Clamp(amount, MinimumArmorValue, _maxArmor);

        if (_currentArmor == validArmor)
        {
            UpdateArmorUI();

            return;
        }

        _currentArmor = validArmor;

        UpdateArmorUI();
        OnArmorChanged?.Invoke(_currentArmor, _maxArmor);
    }

    public void AddArmor(int amount)
    {
        if (amount <= MinimumArmorValue || IsArmorUnlocked() == false)
        {
            return;
        }

        SetArmor(_currentArmor + amount);
    }

    public int TakeArmorDamage(int damageAmount)
    {
        if (damageAmount <= MinimumArmorValue)
        {
            return MinimumArmorValue;
        }

        if (HasArmor == false)
        {
            return damageAmount;
        }

        int oldArmor = _currentArmor;
        int absorbedDamage = Mathf.Min(damageAmount, _currentArmor);
        int remainingDamage = damageAmount - absorbedDamage;

        SetArmor(_currentArmor - absorbedDamage);
        PlayArmorDamageSound(oldArmor, _currentArmor);

        return remainingDamage;
    }

    public void FillArmor()
    {
        if (IsArmorUnlocked() == false)
        {
            return;
        }

        SetArmor(_maxArmor);
    }

    public void ResetArmor()
    {
        if (IsArmorUnlocked() == false)
        {
            return;
        }

        SetArmor(MinimumArmorValue);
    }

    public void UnlockArmor()
    {
        if (_isArmorUnlocked)
        {
            return;
        }

        _isArmorUnlocked = true;

        ShowArmorUI();
        SetArmor(_maxArmor);
    }

    public void UnlockArmorAbility()
    {
        UnlockArmor();
    }

    public void LoadArmorFromSave(int armorFromSave)
    {
        if (IsArmorUnlocked() == false)
        {
            return;
        }

        int validArmor = Mathf.Clamp(armorFromSave, MinimumArmorValue, _maxArmor);

        SetArmor(validArmor);
    }

    public void UpdateArmorUI()
    {
        if (_armorIcons == null)
        {
            return;
        }

        int iconCount = Mathf.Min(_armorIcons.Length, _maxArmor);

        for (int i = 0; i < iconCount; i++)
        {
            Image armorIcon = _armorIcons[i];

            if (armorIcon == null)
            {
                continue;
            }

            armorIcon.enabled = true;
            armorIcon.color = i < _currentArmor
                ? Color.white
                : new Color(1f, 1f, 1f, DisabledArmorAlpha);
        }
    }

    public bool CanPurchaseArmorPlates()
    {
        return IsArmorUnlocked() && _currentArmor < _maxArmor;
    }

    public bool NeedArmorPlates()
    {
        return CanPurchaseArmorPlates();
    }

    private void InitializeReferences()
    {
        _hero = GetComponent<Hero>();

        if (_hero == null)
        {
            _hero = FindFirstObjectByType<Hero>();
        }

        if (_sfxPlayer == null)
            _sfxPlayer = FindFirstObjectByType<SfxPlayer>();
    }

    private void InitializeArmorState()
    {
        _isArmorUnlocked = _startEnabled ||
                           (_hero != null &&
                            _hero.AbilityManager != null &&
                            _hero.AbilityManager.HasArmor);

        if (IsArmorUnlocked())
        {
            ShowArmorUI();
            UpdateArmorUI();
        }
        else
        {
            HideArmorUI();
        }
    }

    private void ShowArmorUI()
    {
        if (_armorPanelInstance == null)
        {
            InitializeArmorUI();
        }

        if (_armorPanelInstance != null)
        {
            _armorPanelInstance.SetActive(true);
        }
    }

    private void HideArmorUI()
    {
        if (_armorPanelInstance != null)
        {
            _armorPanelInstance.SetActive(false);
        }
    }

    private void InitializeArmorUI()
    {
        if (_armorPanelPrefab == null)
        {
            return;
        }

        GameObject armorContainer = GameObject.FindGameObjectWithTag("ArmorContainer");

        if (armorContainer == null)
        {
            return;
        }

        _armorPanelInstance = Instantiate(_armorPanelPrefab, armorContainer.transform);

        RectTransform rectTransform = _armorPanelInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;
        }

        _armorPanelInstance.name = ArmorPanelName;
        _armorPanelInstance.SetActive(true);

        _armorIcons = _armorPanelInstance.GetComponentsInChildren<Image>(true);
    }

    private void PlayArmorDamageSound(int oldArmor, int newArmor)
    {
        if (_useAudioController == false || _sfxPlayer == null || _soundConfiguration == null)
        {
            return;
        }

        if (newArmor <= MinimumArmorValue && oldArmor > MinimumArmorValue)
        {
            _sfxPlayer.Play(_soundConfiguration.ArmorBreakSound, _soundConfiguration.ArmorSoundVolume);
            return;
        }

        _sfxPlayer.Play(_soundConfiguration.ArmorDamageSound, _soundConfiguration.ArmorSoundVolume);
    }
}
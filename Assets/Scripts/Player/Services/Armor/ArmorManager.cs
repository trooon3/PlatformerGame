using UnityEngine;
using UnityEngine.UI;

public sealed class ArmorManager : MonoBehaviour, IArmorManager
{
    private const string ArmorPanelName = "ArmorPanel_Runtime";

    [Header("Armor Settings")]
    [SerializeField] private int _maxArmor = 6;
    [SerializeField] private int _currentArmor = 6;
    [SerializeField] private bool _startEnabled = false;

    [Header("UI References")]
    [SerializeField] private GameObject _armorPanelPrefab;

    [Header("Sound Settings")]
    [SerializeField] private bool _useAudioController = true;

    private GameObject _armorPanelInstance;
    private Image[] _armorIcons;
    private AudioController _audioController;
    private Hero _hero;

    private bool _isArmorUnlocked = false;

    public int CurrentArmor => _currentArmor;
    public int MaxArmor => _maxArmor;

    public bool HasArmor => _currentArmor > 0 && IsArmorUnlocked();

    private void Start()
    {
        InitializeAudioController();
        FindHero();

        _isArmorUnlocked = _startEnabled || (_hero != null && _hero.AbilityManager != null && _hero.AbilityManager.HasArmor);

        if (IsArmorUnlocked())
        {
            InitializeArmorUI();
            UpdateArmorUI();
        }
        else
        {
            HideArmorUI();
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

    public bool IsArmorUnlocked()
    {
        return _isArmorUnlocked ||
               (_hero != null && _hero.AbilityManager != null && _hero.AbilityManager.HasArmor);
    }

    private void InitializeAudioController()
    {
        if (_useAudioController)
        {
            _audioController = GetComponentInParent<AudioController>();

            if (_audioController == null)
            {
                _audioController = FindObjectOfType<AudioController>();
            }
        }
    }

    private void InitializeArmorUI()
    {
        if (_armorPanelPrefab == null)
        {
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            return;
        }

        _armorPanelInstance = Instantiate(_armorPanelPrefab, canvas.transform);
        _armorPanelInstance.name = ArmorPanelName;

        SetActiveRecursively(_armorPanelInstance, true);

        _armorIcons = _armorPanelInstance.GetComponentsInChildren<Image>(true);
    }

    private void HideArmorUI()
    {
        if (_armorPanelInstance != null)
        {
            _armorPanelInstance.SetActive(false);
        }
    }

    private void ShowArmorUI()
    {
        if (_armorPanelInstance != null)
        {
            _armorPanelInstance.SetActive(true);
        }
        else
        {
            InitializeArmorUI();
        }
    }

    private void SetActiveRecursively(GameObject root, bool active)
    {
        root.SetActive(active);

        foreach (Transform child in root.transform)
        {
            SetActiveRecursively(child.gameObject, active);

            Image image = child.GetComponent<Image>();

            if (image != null)
            {
                image.enabled = active;
            }
        }
    }

    public void UpdateArmorUI()
    {
        if (_armorIcons == null)
        {
            return;
        }

        for (int i = 0; i < Mathf.Min(_armorIcons.Length, _maxArmor); i++)
        {
            if (_armorIcons[i] != null)
            {
                _armorIcons[i].enabled = true;
                _armorIcons[i].color = i < _currentArmor ? Color.white : new Color(1, 1, 1, 0.3f);
            }
        }
    }

    public void SetArmor(int armorAmount)
    {
        int minimumArmorValue = 0;

        if (!IsArmorUnlocked())
        {
            return;
        }

        _currentArmor = Mathf.Clamp(armorAmount, minimumArmorValue, _maxArmor);

        UpdateArmorUI();

        OnArmorChanged?.Invoke(_currentArmor, _maxArmor);
    }

    public int TakeArmorDamage(int damageAmount)
    {
        if (!IsArmorUnlocked() || !HasArmor)
        {
            return damageAmount;
        }

        int oldArmor = _currentArmor;
        int damageToArmor = Mathf.Min(damageAmount, _currentArmor);
        int remainingDamage = damageAmount - damageToArmor;

        SetArmor(_currentArmor - damageToArmor);
        PlayArmorDamageSound(oldArmor, _currentArmor);

        return remainingDamage;
    }

    private void PlayArmorDamageSound(int oldArmor, int newArmor)
    {
        if (!_useAudioController || _audioController == null)
        {
            return;
        }
    }

    public void AddArmor(int amount)
    {
        if (!IsArmorUnlocked())
        {
            return;
        }

        SetArmor(_currentArmor + amount);
    }

    public void FillArmor()
    {
        if (IsArmorUnlocked())
        {
            int oldArmor = _currentArmor;

            SetArmor(_maxArmor);

            OnArmorChanged?.Invoke(_currentArmor, _maxArmor);
        }
    }

    public event System.Action<int, int> OnArmorChanged;

    public void ResetArmor()
    {
        if (IsArmorUnlocked())
        {
            SetArmor(0);
        }
    }

    public void UnlockArmor()
    {
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
        int minimumArmorValue = 0;

        if (!IsArmorUnlocked())
        {
            return;
        }

        int validArmor = Mathf.Clamp(armorFromSave, minimumArmorValue, _maxArmor);

        if (validArmor != _currentArmor)
        {
            SetArmor(validArmor);
        }
    }

    public bool CanPurchaseArmorPlates()
    {
        return IsArmorUnlocked() && CurrentArmor < MaxArmor;
    }

    public bool NeedArmorPlates()
    {
        return CanPurchaseArmorPlates();
    }
}
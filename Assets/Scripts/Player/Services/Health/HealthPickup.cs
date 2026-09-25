using System.Collections;
using UnityEngine;

public sealed class HealthPickup : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const float DestroyDelay = 2f;
    private const float GizmoRadius = 0.5f;
    private const float FlashDuration = 0.2f;
    private const int SmallHealAmount = 2;
    private const int FrameCheckInterval = 60;

    [Header("Health Settings")]
    [SerializeField] private bool _isFullHeal = true;
    [SerializeField] private string _pickupId;
    [SerializeField] private bool _requiresAnatomyAbility = true;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem _collectEffect;
    [SerializeField] private GameObject _lockedHint; 
    [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color _collectedColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);

    [Header("Audio")]
    [SerializeField] private SfxPlayer _sfxPlayer;
    [SerializeField] private SoundConfiguration _soundConfiguration;

    private bool _isCollected;
    private bool _hasValidId;
    private bool _canPickup;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private Hero _playerHero;
    private Color _originalColor;

    private void Start()
    {
        InitializeComponents();
        EnsurePickupId();
        LoadCollectionState();
        FindPlayerHero();
        UpdatePickupState();
    }

    private void Update()
    {
        if (_isCollected || _requiresAnatomyAbility == false)
        {
            return;
        }

        if (_playerHero == null || _playerHero.AbilityManager == null)
        {
            FindPlayerHero();
        }

        if (Time.frameCount % FrameCheckInterval != 0)
        {
            return;
        }

        bool canPickup = CheckPickupAbility();

        if (canPickup != _canPickup)
        {
            _canPickup = canPickup;

            UpdateVisualState();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isCollected || other.CompareTag(PlayerTag) == false)
        {
            return;
        }

        CollectHealth(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_isCollected || other.CompareTag(PlayerTag) == false)
        {
            return;
        }

        UpdatePickupState();
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_pickupId))
        {
            _pickupId = GenerateUniqueId();
            _hasValidId = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _isCollected ? Color.red : _canPickup ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, GizmoRadius);
    }

    public void OnAnatomyAbilityPurchased()
    {
        if (_isCollected)
        {
            return;
        }

        UpdatePickupState();
    }

    public void RefreshPickupState()
    {
        FindPlayerHero();
        UpdatePickupState();
    }

    private void InitializeComponents()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

        if (_collider != null)
        {
            _collider.isTrigger = true;
        }

        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
    }

    private void EnsurePickupId()
    {
        if (string.IsNullOrEmpty(_pickupId))
        {
            _pickupId = GenerateUniqueId();
        }

        _hasValidId = string.IsNullOrEmpty(_pickupId) == false;
    }

    private void FindPlayerHero()
    {
        if (_playerHero != null && _playerHero.AbilityManager != null)
        {
            return;
        }

        if (Hero.Instance != null)
        {
            _playerHero = Hero.Instance;

            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);

        if (player != null)
        {
            _playerHero = player.GetComponent<Hero>();
        }

        if (_playerHero == null)
        {
            _playerHero = FindFirstObjectByType<Hero>();
        }
    }

    private void LoadCollectionState()
    {
        if (string.IsNullOrEmpty(_pickupId))
        {
            return;
        }

        if (SaveSystem.Instance == null || SaveSystem.Instance.HasSave() == false)
        {
            return;
        }

        GameSaveData saveData = SaveSystem.Instance.CurrentSave;

        if (saveData?.collectedHealthPickups == null)
        {
            return;
        }

        if (saveData.collectedHealthPickups.Contains(_pickupId) == false)
        {
            return;
        }

        _isCollected = true;

        HidePickup();
    }

    private string GenerateUniqueId()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        float positionX = Mathf.Round(transform.position.x * 100f) / 100f;
        float positionY = Mathf.Round(transform.position.y * 100f) / 100f;

        return $"health_{sceneName}_{gameObject.GetInstanceID()}_{positionX}_{positionY}";
    }

    private void CollectHealth(GameObject player)
    {
        UpdatePickupState();

        if (_requiresAnatomyAbility && _canPickup == false)
        {
            ShowLockedMessage();

            return;
        }

        _isCollected = true;

        if (_hasValidId)
        {
            SaveSystem.Instance?.MarkHealthPickupCollected(_pickupId);
        }

        HidePickup();
        ApplyHealing(player);
        PlayCollectEffects();

        Destroy(gameObject, DestroyDelay);
    }

    private bool CheckPickupAbility()
    {
        if (_requiresAnatomyAbility == false)
        {
            return true;
        }

        if (_playerHero == null)
        {
            FindPlayerHero();
        }

        return _playerHero != null &&
               _playerHero.AbilityManager != null &&
               _playerHero.AbilityManager.HasAnatomy;
    }

    private void UpdatePickupState()
    {
        if (_isCollected)
        {
            _canPickup = false;
            UpdateVisualState();

            return;
        }

        _canPickup = CheckPickupAbility();

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (_spriteRenderer != null)
        {
            if (_isCollected)
            {
                _spriteRenderer.color = _collectedColor;
            }
            else
            {
                _spriteRenderer.color = _canPickup || _requiresAnatomyAbility == false
                    ? _originalColor
                    : _lockedColor;
            }
        }

        if (_lockedHint != null)
        {
            bool shouldShowHint = _isCollected == false && _requiresAnatomyAbility && _canPickup == false;

            _lockedHint.SetActive(shouldShowHint);
        }
    }

    private void ShowLockedMessage()
    {
        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        if (_spriteRenderer == null)
        {
            yield break;
        }

        Color previousColor = _spriteRenderer.color;

        _spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(FlashDuration);

        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = previousColor;
        }
    }

    private void HidePickup()
    {
        if (_collider != null)
        {
            _collider.enabled = false;
        }

        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
        }

        if (_lockedHint != null)
        {
            _lockedHint.SetActive(false);
        }
    }

    private void ApplyHealing(GameObject player)
    {
        HealthManager healthManager = player.GetComponent<HealthManager>();

        if (healthManager == null)
        {
            healthManager = FindFirstObjectByType<HealthManager>();
        }

        if (healthManager == null)
        {
            return;
        }

        if (_isFullHeal)
        {
            healthManager.FullHeal();
        }
        else
        {
            healthManager.Heal(SmallHealAmount);
        }

        PlayHealSound(player);
    }

    private void PlayHealSound(GameObject player)
    {
        if (_sfxPlayer == null)
            _sfxPlayer = player.GetComponent<SfxPlayer>();

        if (_sfxPlayer == null)
            _sfxPlayer = FindFirstObjectByType<SfxPlayer>();

        if (_sfxPlayer != null && _soundConfiguration != null)
            _sfxPlayer.Play(_soundConfiguration.HealSound);
    }

    private void PlayCollectEffects()
    {
        if (_collectEffect == null)
        {
            return;
        }

        ParticleSystem effectInstance = Instantiate(_collectEffect, transform.position, Quaternion.identity);

        effectInstance.Play();

        Destroy(effectInstance.gameObject, DestroyDelay);
    }
}
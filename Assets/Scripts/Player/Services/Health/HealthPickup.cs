using UnityEngine;

public sealed class HealthPickup : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const float DestroyDelay = 2f;
    private const float GizmoRadius = 0.5f;

    [Header("Health Settings")]
    [SerializeField] private bool _isFullHeal = true;
    [SerializeField] private string _pickupId;
    [SerializeField] private bool _requiresAnatomyAbility = true;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem _collectEffect;
    [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color _collectedColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);

    private bool _isCollected = false;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private Hero _playerHero;

    private bool _hasValidId = false;
    private bool _canPickup = false;

    private Color _originalColor;

    private void Start()
    {
        InitializeComponents();
        LoadCollectionState();
        FindPlayerHero();

        if (string.IsNullOrEmpty(_pickupId))
        {
            _pickupId = GenerateUniqueId();

            _hasValidId = true;
        }
        else
        {
            _hasValidId = true;
        }

        UpdatePickupState();
    }

    private void Update()
    {
         int frameCheckInterval = 60;
         int frameModuloZero = 0;

        if (!_isCollected && _requiresAnatomyAbility && Time.frameCount % frameCheckInterval == frameModuloZero)
        {
            bool newCanPickup = CheckPickupAbility();

            if (newCanPickup != _canPickup)
            {
                _canPickup = newCanPickup;

                UpdateVisualState();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isCollected || !other.CompareTag(PlayerTag))
        {
            return;
        }

        CollectHealth(other.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _isCollected ? Color.red : (_canPickup ? Color.green : Color.yellow);
        Gizmos.DrawWireSphere(transform.position, GizmoRadius);
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_pickupId))
        {
            _pickupId = GenerateUniqueId();

            _hasValidId = true;
        }
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

    private void FindPlayerHero()
    {
        GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);

        if (player != null)
        {
            _playerHero = player.GetComponent<Hero>();
        }
    }

    private void LoadCollectionState()
    {
        if (string.IsNullOrEmpty(_pickupId))
        {
            return;
        }

        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave())
        {
            var saveData = SaveSystem.Instance.CurrentSave;

            if (saveData.collectedHealthPickups != null)
            {
                if (saveData.collectedHealthPickups.Contains(_pickupId))
                {
                    _isCollected = true;

                    HidePickup();
                }
            }
        }
    }

    private string GenerateUniqueId()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        float posX = Mathf.Round(transform.position.x * 100f) / 100f;
        float posY = Mathf.Round(transform.position.y * 100f) / 100f;

        return $"health_{sceneName}_{gameObject.GetInstanceID()}_{posX}_{posY}";
    }

    private void CollectHealth(GameObject player)
    {
        UpdatePickupState(); 

        if (_requiresAnatomyAbility && !_canPickup)
        {
            ShowLockedMessage();

            return;
        }

        _isCollected = true;

        if (_hasValidId && !string.IsNullOrEmpty(_pickupId))
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
        if (!_requiresAnatomyAbility)
        {
            return true;
        }

        if (_playerHero == null)
        {
            FindPlayerHero();
        }

        return _playerHero != null && _playerHero.AbilityManager != null && _playerHero.AbilityManager.HasAnatomy;
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
        if (_spriteRenderer == null)
        {
            return;
        }

        if (_isCollected)
        {
            _spriteRenderer.color = _collectedColor;

            return;
        }

        if (!_canPickup && _requiresAnatomyAbility)
        {
            _spriteRenderer.color = _lockedColor;
        }
        else
        {
            _spriteRenderer.color = _originalColor;
        }
    }

    private void ShowLockedMessage()
    {
        StartCoroutine(FlashRed());
    }

    private System.Collections.IEnumerator FlashRed()
    {
        if (_spriteRenderer == null)
        {
            yield break;
        }

        Color originalColor = _spriteRenderer.color;

        _spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = originalColor;
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
    }

    private void ApplyHealing(GameObject player)
    {
        HealthManager healthManager = player.GetComponent<HealthManager>() ?? FindObjectOfType<HealthManager>();

        if (healthManager != null)
        {
            if (_isFullHeal)
            {
                healthManager.FullHeal();
            }
            else
            {
                healthManager.Heal(2);
            }

            PlayHealSound(player);
        }
    }

    private void PlayHealSound(GameObject player)
    {
        AudioController audioController = player.GetComponent<AudioController>();

        audioController?.PlayHealSound();
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

    public void OnAnatomyAbilityPurchased()
    {
        if (!_isCollected)
        {
            UpdatePickupState();
        }
    }

    public void RefreshPickupState()
    {
        FindPlayerHero(); 
        UpdatePickupState();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(PlayerTag) && !_isCollected)
        {
            UpdatePickupState();
        }
    }
}
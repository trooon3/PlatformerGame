using Player;
using Player.Abilities;
using Player.Input;
using Player.StateMachine;
using Shared.Damage;
using Shared.Sensors;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public sealed class Hero : MonoBehaviour, IDamageable
{
    private const string PlayerChildName = "Player";
    private const float ZeroDirectionThreshold = 0f;
    private const float WallCheckDistance = 0.5f;

    [Header("Core Hero Parameters")]
    [SerializeField] private HeroData _heroData;

    [Header("Ground Detection")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private LayerMask _groundLayerMask;

    [Header("Attack Settings")]
    [SerializeField] private Transform _attackPoint;

    [Header("Wall Detection")]
    [SerializeField] private Transform _leftWallCheckPoint;
    [SerializeField] private Transform _rightWallCheckPoint;

    [Header("Passive Health Regeneration")]
    [SerializeField] private PassiveHealthRegeneration _passiveHealthRegen;

    [Header("Audio")]
    [SerializeField] private SfxPlayer _sfxPlayer;
    [SerializeField] private MusicPlayer _musicPlayer;
    [SerializeField] private FootstepPlayer _footstepPlayer;
    [SerializeField] private SoundConfiguration _soundConfiguration;

    public static event System.Action OnHeroDiedGlobal;

    private bool _hasPerformedAirAttack;
    private bool _isGrounded;
    private bool _wasMoving;
    private bool _isDead;

    private DamageService _damageService;
    private SpriteRenderer _spriteRenderer;
    private HealthManager _healthManager;
    private ArmorManager _armorManager;
    private GroundCheck _groundCheck;
    private IInputProvider _inputProvider;

    public SfxPlayer SfxPlayer { get; private set; }
    public MusicPlayer MusicPlayer { get; private set; }
    public FootstepPlayer FootstepPlayer { get; private set; }
    public SoundConfiguration SoundConfiguration { get; private set; }
    public AbilityManager AbilityManager { get; private set; }
    public HeroStateMachine StateMachine { get; private set; }
    public AnimationService AnimationService { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public static Hero Instance { get; private set; }

    public HeroData Data => _heroData;
    public Transform AttackPoint => _attackPoint;
    public int Lives => _healthManager != null ? _healthManager.CurrentHealth : 0;
    public float FacingDirection => _spriteRenderer != null && _spriteRenderer.flipX ? -1f : 1f;
    public bool HasPerformedAirAttack => _hasPerformedAirAttack;

    private void Awake()
    {
        Instance = this;

        InitializeAbilityManager();
        InitializeComponents();
        InitializeStateMachine();
        InitializeSensors();
        SubscribeToEvents();
    }

    private void Start()
    {
        if (_passiveHealthRegen == null)
        {
            _passiveHealthRegen = GetComponent<PassiveHealthRegeneration>();
        }
    }

    private void Update()
    {
        StateMachine?.Tick();

        if (_isDead)
        {
            return;
        }

        UpdateMovementSounds();
    }

    private void FixedUpdate()
    {
        if (_isDead)
        {
            return;
        }

        StateMachine?.FixedTick();
        UpdateGroundCheck();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        StopMovementSounds();
    }

    public void SetAirAttackPerformed(bool isPerformed)
    {
        _hasPerformedAirAttack = isPerformed;
    }

    public void DecreaseLives(int amount)
    {
        _healthManager?.TakeDamage(amount);
    }

    public void SetHealth(int health)
    {
        _healthManager?.SetHealth(health);
    }

    public void OnAttackHit()
    {
        if (StateMachine.Current is IDamageDealer damageDealer)
        {
            damageDealer.DealDamage();
        }
    }

    public void OnAttackEnd()
    {
        if (StateMachine.Current is IAnimationEndHandler animationEndHandler)
        {
            animationEndHandler.OnAnimationEnd();
        }
    }

    public void TakeDamage(int amount)
    {
        if (_isDead)
        {
            return;
        }

        _damageService.ApplyDamage(amount);
    }

    public void SetFacing(float direction)
    {
        if (_spriteRenderer == null || Mathf.Abs(direction) <= ZeroDirectionThreshold)
        {
            return;
        }

        _spriteRenderer.flipX = direction < ZeroDirectionThreshold;
    }

    public bool HasArmor()
    {
        return _armorManager != null && _armorManager.HasArmor;
    }

    public void AddArmor(int amount)
    {
        _armorManager?.AddArmor(amount);
    }

    public bool IsAlive()
    {
        return _isDead == false && _healthManager != null && _healthManager.CurrentHealth > 0;
    }

    public bool NeedsHealing()
    {
        return _healthManager != null && _healthManager.CurrentHealth < _healthManager.MaxHealth;
    }

    public bool IsTouchingWall()
    {
        if (_leftWallCheckPoint == null || _rightWallCheckPoint == null)
        {
            return false;
        }

        bool leftWallDetected = Physics2D.Raycast(
            _leftWallCheckPoint.position,
            Vector2.left,
            WallCheckDistance,
            _groundLayerMask);

        bool rightWallDetected = Physics2D.Raycast(
            _rightWallCheckPoint.position,
            Vector2.right,
            WallCheckDistance,
            _groundLayerMask);

        return leftWallDetected || rightWallDetected;
    }

    private void InitializeComponents()
    {
        _healthManager = GetComponent<HealthManager>();

        if (_healthManager == null)
        {
            _healthManager = FindFirstObjectByType<HealthManager>();
        }

        _armorManager = GetComponent<ArmorManager>();

        if (_armorManager == null)
        {
            _armorManager = FindFirstObjectByType<ArmorManager>();
        }

        Rigidbody = GetComponent<Rigidbody2D>();
        AnimationService = new AnimationService(GetComponent<Animator>());

        if (_sfxPlayer == null) _sfxPlayer = FindFirstObjectByType<SfxPlayer>();
        if (_musicPlayer == null) _musicPlayer = FindFirstObjectByType<MusicPlayer>();
        if (_footstepPlayer == null) _footstepPlayer = FindFirstObjectByType<FootstepPlayer>();
        if (_soundConfiguration == null) _soundConfiguration = FindFirstObjectByType<SoundConfiguration>();

        SfxPlayer = _sfxPlayer;
        MusicPlayer = _musicPlayer;
        FootstepPlayer = _footstepPlayer;
        SoundConfiguration = _soundConfiguration;

        _inputProvider = GetComponent<IInputProvider>();
        _groundCheck = _groundCheckPoint != null ? _groundCheckPoint.GetComponent<GroundCheck>() : null;

        InitializeSpriteRenderer();

        _damageService = new DamageService(this, _healthManager);
    }

    private void InitializeSpriteRenderer()
    {
        Transform playerTransform = transform.Find(PlayerChildName);

        _spriteRenderer = playerTransform != null
            ? playerTransform.GetComponent<SpriteRenderer>()
            : null;

        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }
    }

    private void InitializeAbilityManager()
    {
        AbilityManager = new AbilityManager(this);
    }

    private void InitializeStateMachine()
    {
        StateMachine = new HeroStateMachine(this);
        StateMachine.Change<IdleState>();
    }

    private void InitializeSensors()
    {
        InitializeGroundSensor();
        InitializeEnemyScanner();
        EnsureInputProvider();
    }

    private void InitializeGroundSensor()
    {
        if (_groundCheckPoint == null)
        {
            return;
        }

        if (_groundCheck == null)
        {
            _groundCheck = _groundCheckPoint.gameObject.AddComponent<GroundCheck>();
        }

        _groundCheck.SetLayers(_groundLayerMask);
    }

    private void InitializeEnemyScanner()
    {
        if (GetComponentInChildren<EnemyScanner>() != null)
        {
            return;
        }

        gameObject.AddComponent<EnemyScanner>();
    }

    private void EnsureInputProvider()
    {
        if (_inputProvider != null)
        {
            return;
        }

        _inputProvider = gameObject.AddComponent<OldInputProvider>();
    }

    private void SubscribeToEvents()
    {
        if (_healthManager != null)
        {
            _healthManager.OnDeath += HandleHeroDeath;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_healthManager != null)
        {
            _healthManager.OnDeath -= HandleHeroDeath;
        }
    }

    private void HandleHeroDeath()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;
        OnHeroDiedGlobal?.Invoke();

        if (SfxPlayer != null && SoundConfiguration != null)
            SfxPlayer.Play(SoundConfiguration.DeathSound);
        StopMovementSounds();

        StateMachine.Change<DieState>();
    }

    private void UpdateGroundCheck()
    {
        _isGrounded = _groundCheck != null && _groundCheck.IsGrounded;
    }

    private void UpdateMovementSounds()
    {
        if (FootstepPlayer == null || _isDead)
        {
            return;
        }

        float horizontalInput = _inputProvider != null ? _inputProvider.HorizontalAxis : 0f;

        bool hasMovementInput = Mathf.Abs(horizontalInput) > ZeroDirectionThreshold;
        bool shouldPlayFootsteps = hasMovementInput && _isGrounded;

        if (shouldPlayFootsteps && _wasMoving == false)
        {
            FootstepPlayer.StartFootsteps();
            _wasMoving = true;
        }
        else if (shouldPlayFootsteps == false && _wasMoving)
        {
            FootstepPlayer.StopFootsteps();
            _wasMoving = false;
        }
    }

    private void StopMovementSounds()
    {
        FootstepPlayer?.StopFootsteps();
    }
}
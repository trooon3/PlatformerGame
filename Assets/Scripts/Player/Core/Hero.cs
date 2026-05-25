using Player;
using Player.Abilities;
using Player.Input;
using Player.StateMachine;
using Shared.Damage;
using Shared.Sensors;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public sealed class Hero : MonoBehaviour, IDamageable
{
    private const string PlayerChildName = "Player";
    private const float ZeroDirectionThreshold = 0.0f;
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

    public AudioController AudioController { get; private set; }
    public AbilityManager AbilityManager { get; private set; }

    public HeroData Data => _heroData;

    private bool _hasPerformedAirAttack;
    private DamageService _damageService;
    private SpriteRenderer _spriteRenderer;
    private HealthManager _healthManager;
    private ArmorManager _armorManager;
    private GroundCheck _groundCheck;

    private IInputProvider _inputProvider;

    private bool _isGrounded;
    private bool _wasMoving;
    private bool _isDead;

    public int Lives => _healthManager?.CurrentHealth ?? 0;

    public HeroStateMachine StateMachine { get; private set; }
    public AnimationService AnimationService { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public Transform AttackPoint => _attackPoint;
    public float FacingDirection => _spriteRenderer.flipX ? -1.0f : 1.0f;
    public bool HasPerformedAirAttack => _hasPerformedAirAttack;

    private void Awake()
    {
        InitializeComponents();
        InitializeStateMachine();
        InitializeSensors();
        SubscribeToEvents();

        AbilityManager = new AbilityManager(this);
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
        if (_isDead)
        {
            return;
        }

        StateMachine.Tick();

        UpdateMovementSounds();
    }

    private void FixedUpdate()
    {
        StateMachine.FixedTick();

        UpdateGroundCheck();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        StopMovementSounds();
    }

    private void InitializeComponents()
    {
        _healthManager = GetComponent<HealthManager>() ?? FindObjectOfType<HealthManager>();
        _armorManager = GetComponent<ArmorManager>() ?? FindObjectOfType<ArmorManager>();
        _damageService = new DamageService(this, _healthManager);
        Rigidbody = GetComponent<Rigidbody2D>();
        AnimationService = new AnimationService(GetComponent<Animator>());

        AudioController = GetComponent<AudioController>() ??
                         GetComponentInChildren<AudioController>(true) ??
                         FindObjectOfType<AudioController>();

        _inputProvider = GetComponent<IInputProvider>();
        _groundCheck = _groundCheckPoint.GetComponent<GroundCheck>();

        InitializeSpriteRenderer();
    }

    private void InitializeSpriteRenderer()
    {
        Transform playerTransform = transform.Find(PlayerChildName);

        _spriteRenderer = playerTransform?.GetComponent<SpriteRenderer>() ??
                         GetComponentInChildren<SpriteRenderer>(true);
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
        if (_groundCheck == null)
        {
            _groundCheck = _groundCheckPoint.gameObject.AddComponent<GroundCheck>();
            _groundCheck.SetLayers(_groundLayerMask);
        }
    }

    private void InitializeEnemyScanner()
    {
        if (GetComponentInChildren<EnemyScanner>() == null)
        {
            gameObject.AddComponent<EnemyScanner>();
        }
    }

    private void EnsureInputProvider()
    {
        if (_inputProvider == null)
        {
            gameObject.AddComponent<OldInputProvider>();
        }
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
        AudioController?.PlayDeathSound();

        StopMovementSounds();

        StateMachine.Change<DieState>();
    }

    private void UpdateGroundCheck()
    {
        _isGrounded = _groundCheck != null && _groundCheck.IsGrounded;
    }

    private void UpdateMovementSounds()
    {
        float noMovementThreshold = 0f;

        if (AudioController == null || _isDead)
        {
            return;
        }

        float horizontalInput = _inputProvider?.HorizontalAxis ?? noMovementThreshold;

        bool hasMovementInput = Mathf.Abs(horizontalInput) > ZeroDirectionThreshold;
        bool shouldPlayFootsteps = hasMovementInput && _isGrounded && !_isDead;

        if (shouldPlayFootsteps && !_wasMoving)
        {
            AudioController.StartFootsteps();

            _wasMoving = true;
        }
        else if (!shouldPlayFootsteps && _wasMoving)
        {
            AudioController.StopFootsteps();

            _wasMoving = false;
        }
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
        (StateMachine.Current as IDamageDealer)?.DealDamage();
    }

    public void OnAttackEnd()
    {
        (StateMachine.Current as IAnimationEndHandler)?.OnAnimationEnd();
    }

    public void TakeDamage(int amount)
    {
        _damageService.ApplyDamage(amount);
    }

    public void SetFacing(float direction)
    {
        if (Mathf.Abs(direction) > ZeroDirectionThreshold)
        {
            _spriteRenderer.flipX = direction < ZeroDirectionThreshold;
        }
    }

    public bool HasArmor()
    {
        return _armorManager?.HasArmor ?? false;
    }

    public void AddArmor(int amount)
    {
        _armorManager?.AddArmor(amount);
    }

    public bool IsAlive()
    {
        int deathThreshold = 0;

        return _healthManager?.CurrentHealth > deathThreshold;
    }

    public bool NeedsHealing()
    {
        return _healthManager?.CurrentHealth < _healthManager.MaxHealth;
    }

    public bool IsTouchingWall()
    {
        bool leftWallDetected = Physics2D.Raycast(_leftWallCheckPoint.position, Vector2.left, WallCheckDistance, _groundLayerMask);
        bool rightWallDetected = Physics2D.Raycast(_rightWallCheckPoint.position, Vector2.right, WallCheckDistance, _groundLayerMask);

        return leftWallDetected || rightWallDetected;
    }

    private void StopMovementSounds()
    {
        AudioController?.StopFootsteps();
    }
}
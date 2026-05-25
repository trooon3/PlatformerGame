using GeneralEnemyPatrolSystem;
using GeneralLogicEnemies;
using Shared.Damage;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public sealed class EnemyCombatController : Entity, IDamageable
{
    private const float StrongAttackProbability = 0.5f;
    private const float AttackCooldownDuration = 2f;
    private const int RegularAttackAnimationCount = 4;
    private const float DamageAnimationDelay = 0.3f;
    private const float AttackCheckRadius = 1.5f;
    private const float HitAnimationDuration = 0.5f;
    private const string AnimationStateParameterName = "state";
    private const string PlayerLayerName = "Player";
    private const float BoxRotationAngle = 0f;
    private const float MovementThreshold = 0.01f;
    private const float MinimumDistanceToResetPlayer = 0.01f;
    private const float DirectionThreshold = 0f;
    private const float MaxDetectionRadiusMultiplier = 1.5f;

    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 2;
    [SerializeField] private int _currentHealth = 2;

    [Header("Attack Settings")]
    [SerializeField] private int _attackDamage = 1;
    [SerializeField] private int _strongAttackDamage = 2;
    [SerializeField] private float _attackCooldown = AttackCooldownDuration;

    [Header("Attack Box Settings")]
    [SerializeField] private Vector2 _attackOffset = new Vector2(1.5f, 0.8f);
    [SerializeField] private Vector2 _attackSize = new Vector2(2.5f, 1.6f);

    [Header("Detection Settings")]
    [SerializeField] private float _detectionRadius = 5f;
    [SerializeField] private LayerMask _playerLayerMask;

    [Header("Hurt Settings")]
    [SerializeField] private float _hurtInvulnerabilityDuration = 0.1f;

    [Header("Component References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private PatrolAI _patrolAI;
    [SerializeField] private EnemyAudioController _audioController;

    private Transform _playerTransform;
    private HealthManager _playerHealthManager;
    private bool _isAttacking = false;
    private bool _isTakingDamage = false;
    private float _lastAttackTime = 0f;
    private Vector2 _lastPosition;
    private bool _isMoving = false;
    private Coroutine _attackCoroutine;
    private Vector2 _facingDirection = Vector2.right;
    private bool _isHurt = false;
    private bool _isPlayerInRange = false;
    private EnemyAnimationState _currentAnimationState = EnemyAnimationState.Idle;

    public new int MaxLives => _maxHealth;
    public new bool IsAlive => _currentHealth > 0;

    protected override void Awake()
    {
        base.Awake();

        InitializeHealth();
        InitializeComponents();

        _lastPosition = transform.position;
    }

    private void Start()
    {
        ValidateAnimatorParameters();
        SubscribeToPatrolAIEvents();
    }

    private void InitializeHealth()
    {
        _currentHealth = _maxHealth;
    }

    private void InitializeComponents()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        if (_patrolAI == null)
        {
            _patrolAI = GetComponent<PatrolAI>();
        }

        if (_audioController == null)
        {
            _audioController = GetComponent<EnemyAudioController>();
        }
    }

    private void ValidateAnimatorParameters()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            return;
        }

        foreach (var parameter in _animator.parameters)
        {
            if (parameter.name == AnimationStateParameterName && parameter.type == AnimatorControllerParameterType.Int)
            {
                break;
            }
        }
    }

    private void SubscribeToPatrolAIEvents()
    {
        if (_patrolAI != null)
        {
            var patrolAIComponent = _patrolAI as MonoBehaviour;
        }
    }

    private void Update()
    {
        if (IsDead)
        {
            return;
        }

        if (_isHurt || _isAttacking)
        {
            return;
        }

        UpdateMovementState();
        UpdatePlayerDetection();
        UpdateCombatState();
        UpdateAnimationState();
        UpdateFacingDirection();
    }

    private void UpdateMovementState()
    {
        if (_rigidbody != null)
        {
            _isMoving = Mathf.Abs(_rigidbody.velocity.x) > MovementThreshold;
        }
        else
        {
            Vector2 currentPosition = transform.position;
            _isMoving = Vector2.Distance(currentPosition, _lastPosition) > MinimumDistanceToResetPlayer;
            _lastPosition = currentPosition;
        }
    }

    private void UpdatePlayerDetection()
    {
        if (_playerTransform != null && _playerHealthManager != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            if (distanceToPlayer > _detectionRadius * MaxDetectionRadiusMultiplier)
            {
                _playerTransform = null;
                _playerHealthManager = null;
                _isPlayerInRange = false;
            }
            else
            {
                _isPlayerInRange = distanceToPlayer <= AttackCheckRadius;
            }
            return;
        }

        Collider2D playerCollider = Physics2D.OverlapCircle(
            transform.position,
            _detectionRadius,
            _playerLayerMask
        );

        if (playerCollider != null && playerCollider.CompareTag("Player"))
        {
            _playerTransform = playerCollider.transform;
            _playerHealthManager = playerCollider.GetComponent<HealthManager>();

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            _isPlayerInRange = distanceToPlayer <= AttackCheckRadius;
        }
    }

    private void UpdateCombatState()
    {
        if (_isHurt || _isAttacking)
        {
            return;
        }

        if (_playerTransform == null || _playerHealthManager == null)
        {
            return;
        }

        if (_isPlayerInRange && CanAttack())
        {
            ExecuteAttack();
        }
    }

    private void UpdateFacingDirection()
    {
        if (_isHurt || _isAttacking || IsDead)
        {
            return;
        }

        if (_rigidbody != null && Mathf.Abs(_rigidbody.velocity.x) > MovementThreshold)
        {
            _facingDirection = _rigidbody.velocity.x > DirectionThreshold ? Vector2.right : Vector2.left;

            UpdateSpriteFlip();
        }
        else if (_playerTransform != null)
        {
            float directionToPlayer = _playerTransform.position.x - transform.position.x;

            _facingDirection = directionToPlayer > DirectionThreshold ? Vector2.right : Vector2.left;

            UpdateSpriteFlip();
        }
    }

    private void UpdateSpriteFlip()
    {
        if (_facingDirection.x > DirectionThreshold)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (_facingDirection.x < DirectionThreshold)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private bool CanAttack()
    {
        return Time.time - _lastAttackTime >= _attackCooldown;
    }

    private void ExecuteAttack()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
        }

        _attackCoroutine = StartCoroutine(PerformAttackSequence());
        _lastAttackTime = Time.time;
    }

    private IEnumerator PerformAttackSequence()
    {
        float minimumRemainingTime = 0.1f;

        _isAttacking = true;

        EnemyAnimationState selectedAttack = SelectAttackType();
        ForceAnimationState(selectedAttack);

        yield return new WaitForSeconds(DamageAnimationDelay);

        float animationLength = GetCurrentAnimationLength();
        float remainingAnimationTime = Mathf.Max(animationLength - DamageAnimationDelay, minimumRemainingTime);

        yield return new WaitForSeconds(remainingAnimationTime);

        _isAttacking = false;
        _attackCoroutine = null;

        if (_isPlayerInRange && CanAttack())
        {
            ExecuteAttack();
        }
    }

    public void OnAttackAnimationEvent()
    {
        Vector2 attackCenter = CalculateAttackCenter(_attackOffset);

        bool hitConnected = CheckForHit(attackCenter, _attackSize,
            _currentAnimationState == EnemyAnimationState.StrongAttack ? _strongAttackDamage : _attackDamage);

        PlayAttackSound(hitConnected);
    }

    private Vector2 CalculateAttackCenter(Vector2 offset)
    {
        float directionSign = Mathf.Sign(transform.localScale.x);

        return (Vector2)transform.position + new Vector2(directionSign * offset.x, offset.y);
    }

    private bool CheckForHit(Vector2 center, Vector2 size, int damage)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, BoxRotationAngle, LayerMask.GetMask(PlayerLayerName));

        bool hitConnected = false;

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out HealthManager healthManager))
            {
                healthManager.TakeDamage(damage);
                hitConnected = true;
            }
        }

        return hitConnected;
    }

    private void PlayAttackSound(bool hitConnected)
    {
        if (_audioController == null)
        {
            return;
        }

        if (_currentAnimationState == EnemyAnimationState.StrongAttack)
        {
            if (hitConnected)
            {
                _audioController.PlaySpecialAttackHitSound();
            }
            else
            {
                _audioController.PlaySpecialAttackMissSound();

            }
        }
        else
        {
            if (hitConnected)
            {
                _audioController.PlayAttackHitSound();
            }
            else
            {
                _audioController.PlayAttackMissSound();
            }
        }
    }

    private EnemyAnimationState SelectAttackType()
    {
        if (Random.value <= StrongAttackProbability)
        {
            return EnemyAnimationState.StrongAttack;
        }

        int attackIndex = Random.Range(0, RegularAttackAnimationCount);

        return attackIndex switch
        {
            0 => EnemyAnimationState.Attack,
            1 => EnemyAnimationState.Attack2,
            2 => EnemyAnimationState.Kick,
            3 => EnemyAnimationState.DoubleAttack,
            _ => EnemyAnimationState.Attack
        };
    }

    private float GetCurrentAnimationLength()
    {
        float defaultAnimationLength = 1f;
        int minimumValidLength = 0;

        if (_animator == null)
        {
            return defaultAnimationLength;
        }

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.length > minimumValidLength ? stateInfo.length : defaultAnimationLength;
    }

    private void UpdateAnimationState()
    {
        if (IsDead || _isAttacking || _isHurt)
        {
            return;
        }

        EnemyAnimationState newState = DetermineAppropriateAnimation();

        if (newState != _currentAnimationState)
        {
            ChangeAnimationState(newState);
        }
    }

    private EnemyAnimationState DetermineAppropriateAnimation()
    {
        if (IsDead)
        {
            return EnemyAnimationState.Dead;
        }

        if (_isAttacking || _isHurt)
        {
            return _currentAnimationState;
        }

        return _isMoving ? EnemyAnimationState.Walk : EnemyAnimationState.Idle;
    }

    private void ChangeAnimationState(EnemyAnimationState newState)
    {
        if (_currentAnimationState == newState)
        {
            return;

        }

        _currentAnimationState = newState;

        if (_animator != null && _animator.isActiveAndEnabled)
        {
            _animator.SetInteger(AnimationStateParameterName, (int)newState);
        }
    }

    private void ForceAnimationState(EnemyAnimationState newState)
    {
        int animationLayer = 0;
        float normalizedTime = 0f;

        _currentAnimationState = newState;

        if (_animator != null && _animator.isActiveAndEnabled)
        {
            _animator.SetInteger(AnimationStateParameterName, (int)newState);

            _animator.Play(_currentAnimationState.ToString(), animationLayer, normalizedTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDead || _isHurt)
        {
            return;
        }

        if (other.CompareTag("Weapon") || other.CompareTag("PlayerAttack"))
        {
            TakeDamage(1);
        }
    }

    public override void TakeDamage(int amount)
    {
        int minimumHealthValue = 0;

        if (IsDead || _isHurt)
        {
            return;
        }

        _currentHealth -= amount;

        if (_audioController != null)
        {
            _audioController.PlayHurtSound();
        }

        StartCoroutine(PlayHitState());

        if (_currentHealth <= minimumHealthValue)
        {
            _currentHealth = minimumHealthValue;

            Die();
        }
    }

    private IEnumerator PlayHitState()
    {
        _isHurt = true;

        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);

            _isAttacking = false;
            _attackCoroutine = null;
        }

        bool wasPatrolAIEnabled = _patrolAI != null && _patrolAI.enabled;

        if (_patrolAI != null)
        {
            _patrolAI.enabled = false;
        }

        if (_rigidbody != null)
        {
            _rigidbody.velocity = Vector2.zero;
        }

        ForceAnimationState(EnemyAnimationState.Hit);

        yield return new WaitForSeconds(_hurtInvulnerabilityDuration);

        if (_patrolAI != null && wasPatrolAIEnabled && !IsDead)
        {
            _patrolAI.enabled = true;
        }

        _isHurt = false;

        if (!IsDead && !_isAttacking)
        {
            UpdateAnimationState();
        }
    }

    public override void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        if (_audioController != null)
        {
            _audioController.PlayDeathSound();
        }

        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);

            _attackCoroutine = null;
        }

        if (_patrolAI != null)
        {
            _patrolAI.enabled = false;
        }

        if (_rigidbody != null)
        {
            _rigidbody.velocity = Vector2.zero;
            _rigidbody.isKinematic = true;
        }

        ForceAnimationState(EnemyAnimationState.Dead);

        base.Die();

        Collider2D collider = GetComponent<Collider2D>();

        if (collider != null)
        {
            collider.enabled = false;
        }

        this.enabled = false;

        Destroy(gameObject, GetCurrentAnimationLength());
    }

    public void Heal(int amount)
    {
        if (IsDead)
        {
            return;
        }

        _currentHealth += amount;

        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        _maxHealth = newMaxHealth;

        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
    }

    public int CurrentHealth => _currentHealth;

    private enum EnemyAnimationState
    {
        Idle = 0,
        Walk = 1,
        Attack = 2,
        Attack2 = 3,
        Kick = 4,
        DoubleAttack = 5,
        StrongAttack = 6,
        Hit = 7,
        Dead = 8
    }
}
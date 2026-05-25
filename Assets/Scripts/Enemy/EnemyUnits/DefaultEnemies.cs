using GeneralEnemyPatrolSystem;
using GeneralLogicEnemies;
using Shared.Damage;
using UnityEngine;

namespace EnemyLogicDefault
{
    [RequireComponent(typeof(Animator))]
    public sealed class DefaultEnemies : Entity, IDamageable
    {
        private const string PlayerLayerName = "Player";
        private const float DestroyDelay = 1f;
        private const float MovementThreshold = 0.01f;
        private const float DirectionThreshold = 0f;
        private const float BoxRotationAngle = 0f;
        private const int HealthThreshold = 0;

        [Header("Attack Settings")]
        [SerializeField] private float _attackCooldown = 1f;
        [SerializeField] private int _attackDamage = 1;

        [Header("Attack Box Settings")]
        [SerializeField] private Vector2 _attackOffset = new Vector2(1.5f, 0.8f);
        [SerializeField] private Vector2 _attackSize = new Vector2(2.5f, 1.6f);

        [Header("Attack Timing")]
        [SerializeField] private float _attackDuration = 0.5f;
        [SerializeField] private float _attackPause = 0.4f;

        [Header("Hurt Settings")]
        [SerializeField] private float _hurtInvulnerabilityDuration = 0.1f;

        private Animator _animator;
        private PatrolAI _patrolAI;
        private EnemyAudioController _audioController;

        private bool _isHurt;
        private bool _isAttacking;
        private float _nextAttackTime;
        private bool _isPlayerInRange;
        private bool _wasPlayerInRangeBeforeHurt;

        public float AttackCooldown => _attackCooldown;
        public float AttackDuration => _attackDuration;
        public float AttackPause => _attackPause;
        public Vector2 AttackOffset => _attackOffset;
        public Vector2 AttackSize => _attackSize;
        public int AttackDamage => _attackDamage;

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            _patrolAI = GetComponent<PatrolAI>();
            _audioController = GetComponent<EnemyAudioController>();

            _patrolAI.OnMoveDirectionChanged += HandleMovementDirectionChanged;
            _patrolAI.OnInAttackRange += HandleAttackRangeChanged;
        }

        private void Update()
        {
            if (_isHurt || _isAttacking)
            {
                return;
            }

            if (_isPlayerInRange && Time.time >= _nextAttackTime)
            {
                StartAttack();
            }
        }

        private void StartAttack()
        {
            _isAttacking = true;
            _nextAttackTime = Time.time + _attackDuration + _attackPause + _attackCooldown;

            SetAnimationState(AnimationState.Attack);
            Invoke(nameof(OnAttackEnded), _attackDuration);
        }

        public override void TakeDamage(int amount)
        {
            if (_isHurt)
            {
                return;
            }

            base.TakeDamage(amount);

            if (lives > HealthThreshold)
            {
                StartHurtState();
                PlayHurtSound();
            }
        }

        private void StartHurtState()
        {
            int fullBodyAnimationLayer = -1;
            float resetToAnimationStart = 0f;

            string hurtAnimationName = "Hurt";
            string stateParametr = "state";

            _wasPlayerInRangeBeforeHurt = _isPlayerInRange;

            _isHurt = true;
            _patrolAI.enabled = false;

            _animator.Play(hurtAnimationName, -fullBodyAnimationLayer, resetToAnimationStart);
            _animator.SetInteger(stateParametr, (int)AnimationState.Hurt);

            Invoke(nameof(EndHurtState), _hurtInvulnerabilityDuration);
        }

        public void OnAttack()
        {
            Vector2 attackCenter = CalculateAttackCenter(_attackOffset);

            bool hitConnected = CheckForHit(attackCenter, _attackSize, _attackDamage);

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
                if (hit.TryGetComponent(out Hero hero))
                {
                    hero.TakeDamage(damage);

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

            if (hitConnected)
            {
                _audioController.PlayAttackHitSound();
            }
            else
            {
                _audioController.PlayAttackMissSound();
            }
        }

        private void PlayHurtSound()
        {
            _audioController?.PlayHurtSound();
        }

        private void PlayDeathSound()
        {
            _audioController?.PlayDeathSound();
        }

        public void OnAttackEnded()
        {
            _isAttacking = false;

            if (_isPlayerInRange)
            {
                _nextAttackTime = Time.time + _attackCooldown;
            }

            UpdateAnimation(_patrolAI.CurrentDirection);
        }

        private void EndHurtState()
        {
            int fullBodyAnimationLayer = -1;
            float resetToAnimationStart = 0f;

            string idleAnimationName = "Idle";

            _isHurt = false;
            _patrolAI.enabled = true;

            _animator.Play(idleAnimationName, fullBodyAnimationLayer, resetToAnimationStart);

            if (_wasPlayerInRangeBeforeHurt && Time.time >= _nextAttackTime)
            {
                StartAttack();
            }
            else
            {
                UpdateAnimation(_patrolAI.CurrentDirection);
            }
        }

        private void HandleAttackRangeChanged(bool isInRange)
        {
            _isPlayerInRange = isInRange;

            if (_isHurt)
            {
                return;
            }

            if (isInRange && !_isAttacking && !_isHurt && Time.time >= _nextAttackTime)
            {
                StartAttack();
            }
        }

        private void HandleMovementDirectionChanged(Vector2 direction)
        {
            UpdateAnimation(direction);
        }

        private void UpdateAnimation(Vector2 direction)
        {
            if (_isHurt || _isAttacking)
            {
                return;
            }

            var animationState = direction.magnitude > MovementThreshold ? AnimationState.Walk : AnimationState.Idle;

            SetAnimationState(animationState);
            UpdateFacingDirection(direction);
        }

        private void UpdateFacingDirection(Vector2 direction)
        {
            if (direction.x != DirectionThreshold)
            {
                float scaleSign = Mathf.Sign(direction.x);

                transform.localScale = new Vector3(scaleSign * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }

        public override void Die()
        {
            CancelInvoke(nameof(EndHurtState));
            CancelInvoke(nameof(OnAttackEnded));

            _patrolAI.enabled = false;
            enabled = false;

            SetAnimationState(AnimationState.Death);
            PlayDeathSound();

            base.Die();

            Destroy(gameObject, DestroyDelay);
        }

        private void OnDestroy()
        {
            if (_patrolAI != null)
            {
                _patrolAI.OnMoveDirectionChanged -= HandleMovementDirectionChanged;
                _patrolAI.OnInAttackRange -= HandleAttackRangeChanged;
            }
        }

        private void SetAnimationState(AnimationState state)
        {
            _animator.SetInteger("state", (int)state);
        }

        private enum AnimationState
        {
            Idle,
            Walk,
            Attack,
            Hurt,
            Death
        }
    }
}
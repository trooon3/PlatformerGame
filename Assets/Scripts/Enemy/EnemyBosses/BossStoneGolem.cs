using GeneralLogicEnemies;
using Shared.Damage;
using System;
using UnityEngine;

namespace HardBossLogic
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BossStoneGolem : Entity, IDamageable
    {
        private const float TimerEndThreshold = 0f;
        private const float GroundedVelocityThreshold = 0.05f;
        private const float BoxRotationAngle = 0f;
        private const float MaximumBeamLength = 20f;
        private const int RightDirectionThreshold = 0;
        private const int BossHealthAfterImmunity = 10;
        private const int BossMeleeDamage = 1;
        private const int BossLaserDamage = 1;
        private const float MaximumHealth = 4f;
        private const float DestroyDelay = 3f;
        private const float LaserSoundVolumeMultiplier = 0.8f;
        private const float MoveSoundVolumeMultiplier = 0.4f;
        private const float AttackMissSoundVolumeMultiplier = 0.7f;

        [Header("References")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private LayerMask _playerLayerMask;

        [Header("Combat Settings")]
        [SerializeField] private Transform _laserFirePoint;
        [SerializeField] private float _moveSpeed = 2.5f;
        [SerializeField] private float _aggroDistance = 6f;
        [SerializeField] private float _attackDistance = 1.8f;
        [SerializeField] private float _immuneHealthThreshold = 0.4f;
        [SerializeField] private float _immuneDuration = 3f;
        [SerializeField] private float _laserCheckInterval = 0.15f;

        [Header("Boss Music")]
        [SerializeField] private AudioClip _bossMusic;

        [Header("Boss Sound Effects")]
        [SerializeField] private AudioClip _attackSound;
        [SerializeField] private AudioClip _attackMissSound;
        [SerializeField] private AudioClip _takeDamageSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField] private AudioClip _laserChargeSound;
        [SerializeField] private AudioClip _laserShootSound;
        [SerializeField] private AudioClip _immuneActivateSound;
        [SerializeField] private AudioClip _moveSound;
        [SerializeField] private float _soundEffectVolume = 1f;

        [Header("Sound Variations")]
        [SerializeField] private AudioClip[] _attackSoundVariations;
        [SerializeField] private AudioClip[] _takeDamageSoundVariations;
        [SerializeField] private AudioClip[] _moveSoundVariations;
        [SerializeField] private bool _useSoundVariations = false;

        [Header("Sound Timing")]
        [SerializeField] private float _moveSoundInterval = 0.5f;
        [SerializeField] private float _laserChargeSoundDelay = 0.2f;

        private Rigidbody2D _rigidbody;
        private Animator _animator;
        private AudioController _audioController;
        private BossStoneGolemState _currentState = BossStoneGolemState.Idle;
        private bool _isAggro;
        private bool _isInvulnerable;
        private bool _canCheckLaser = true;
        private bool _playerWasGrounded = true;
        private bool _wasAttackSuccessful;
        private float _immuneTimer;
        private float _lastLaserCheckTime;
        private float _lastMoveSoundTime;

        private readonly Action[] _stateUpdateMethods;

        private BossHealthBar _healthBar;

        public BossStoneGolem()
        {
            _stateUpdateMethods = new Action[Enum.GetValues(typeof(BossStoneGolemState)).Length];

            _stateUpdateMethods[(int)BossStoneGolemState.Idle] = UpdateIdleState;
            _stateUpdateMethods[(int)BossStoneGolemState.Attack] = UpdateAttackState;
            _stateUpdateMethods[(int)BossStoneGolemState.Immune] = UpdateImmuneState;
            _stateUpdateMethods[(int)BossStoneGolemState.LaserAttack] = UpdateLaserAttackState;
            _stateUpdateMethods[(int)BossStoneGolemState.Death] = () => { };
        }

        void Start()
        {
            _healthBar = GetComponent<BossHealthBar>();
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _audioController = FindFirstObjectByType<AudioController>();
        }

        private void Update()
        {
            if (_currentState == BossStoneGolemState.Death)
            {
                return;
            }

            CheckAggro();

            if (!_isAggro)
            {
                return;
            }

            _stateUpdateMethods[(int)_currentState]?.Invoke();
        }

        private void CheckAggro()
        {
            if (_isAggro)
            {
                return;
            }

            if (Vector2.Distance(transform.position, _playerTransform.position) <= _aggroDistance)
            {
                _isAggro = true;

                StartBossMusic();
                SwitchState(BossStoneGolemState.Idle);
            }
        }

        private void StartBossMusic()
        {
            if (_audioController != null && _bossMusic != null)
            {
                _audioController.PlayBossMusic(_bossMusic);
            }
        }

        private void StopBossMusic()
        {
            _audioController?.StopBossMusic();
        }

        private void UpdateIdleState()
        {
            MoveTowardsPlayer();
            PlayMoveSound();

            float healthRatio = (float)lives / MaximumHealth;

            if (!_isInvulnerable && (float)lives / MaximumHealth <= _immuneHealthThreshold)
            {
                SwitchState(BossStoneGolemState.Immune);

                return;
            }

            if (_canCheckLaser && Time.time - _lastLaserCheckTime > _laserCheckInterval && IsPlayerJumping())
            {
                _lastLaserCheckTime = Time.time;

                _canCheckLaser = false;

                SwitchState(BossStoneGolemState.LaserAttack);

                return;
            }

            if (Vector2.Distance(transform.position, _playerTransform.position) <= _attackDistance)
            {
                _wasAttackSuccessful = false;

                SwitchState(BossStoneGolemState.Attack);

                return;
            }
        }

        private void UpdateAttackState() { }

        private void UpdateImmuneState()
        {
            _immuneTimer -= Time.deltaTime;

            if (_immuneTimer <= TimerEndThreshold)
            {
                EndImmunity();
            }
        }

        private void UpdateLaserAttackState() { }

        private void MoveTowardsPlayer()
        {
            float direction = Mathf.Sign(_playerTransform.position.x - transform.position.x);

            _rigidbody.velocity = new Vector2(direction * _moveSpeed, _rigidbody.velocity.y);
            transform.localScale = new Vector3(direction, 1f, 1f);
        }

        private bool IsPlayerJumping()
        {
            var playerRigidbody = _playerTransform.GetComponent<Rigidbody2D>();
            bool isGrounded = Mathf.Abs(playerRigidbody.velocity.y) < GroundedVelocityThreshold;
            bool justJumped = _playerWasGrounded && !isGrounded;

            _playerWasGrounded = isGrounded;

            return justJumped;
        }

        private void SwitchState(BossStoneGolemState newState)
        {
            _currentState = newState;
            _animator.SetInteger("state", (int)newState);

            switch (newState)
            {
                case BossStoneGolemState.Immune:
                    _isInvulnerable = true;
                    _immuneTimer = _immuneDuration;
                    PlayImmuneActivateSound();

                    break;

                case BossStoneGolemState.Death:
                    _rigidbody.velocity = Vector2.zero;
                    PlayDeathSound();

                    break;

                case BossStoneGolemState.LaserAttack:
                    PlayLaserChargeSound();

                    break;
            }
        }

        private void EndImmunity()
        {
            lives = BossHealthAfterImmunity;

            _isInvulnerable = false;

            SwitchState(BossStoneGolemState.Idle);
        }

        public override void TakeDamage(int amount)
        {
            if (_isInvulnerable)
            {
                return;
            }

            PlayTakeDamageSound();

            base.TakeDamage(amount);
        }

        public void DealMeleeDamage()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, _attackDistance, _playerLayerMask);

            if (hit?.GetComponent<IDamageable>() != null)
            {
                hit.GetComponent<IDamageable>()?.TakeDamage(BossMeleeDamage);
                _wasAttackSuccessful = true;
            }
        }

        public void DealLaserDamage()
        {
            const float laserBeamWidth = 0.05f;
            const float laserBeamHeight = 0.05f;

            Vector2 origin = _laserFirePoint.position;
            Vector2 direction = transform.localScale.x > RightDirectionThreshold ? Vector2.right : Vector2.left;

            RaycastHit2D hit = Physics2D.BoxCast(origin, new Vector2(laserBeamWidth, laserBeamHeight), BoxRotationAngle, direction, MaximumBeamLength, _playerLayerMask);

            if (hit.collider?.GetComponent<IDamageable>() != null)
            {
                hit.collider.GetComponent<IDamageable>()?.TakeDamage(BossLaserDamage);
            }
        }

        public override void Die()
        {
            StopBossMusic();
            SwitchState(BossStoneGolemState.Death);

            base.Die();

            Destroy(gameObject, DestroyDelay);
        }

        private void PlayAttackSound()
        {
            if (_audioController == null)
            {
                return;
            }

            AudioClip clipToPlay = _attackSound;

            if (clipToPlay != null)
            {
                _audioController.PlayOneShotWithVolume(clipToPlay, _soundEffectVolume);
            }
        }

        private void PlayAttackMissSound()
        {
            if (_attackMissSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_attackMissSound, _soundEffectVolume * AttackMissSoundVolumeMultiplier);
            }
        }

        private void PlayTakeDamageSound()
        {
            if (_audioController == null)
            {
                return;
            }

            AudioClip clipToPlay = _takeDamageSound;

            if (clipToPlay != null)
            {
                _audioController.PlayOneShotWithVolume(clipToPlay, _soundEffectVolume);
            }
        }

        private void PlayDeathSound()
        {
            if (_deathSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_deathSound, _soundEffectVolume);
            }
        }

        private void PlayLaserChargeSound()
        {
            if (_laserChargeSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_laserChargeSound, _soundEffectVolume * LaserSoundVolumeMultiplier);
            }
        }

        private void PlayLaserShootSound()
        {
            if (_laserShootSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_laserShootSound, _soundEffectVolume);
            }
        }

        private void PlayImmuneActivateSound()
        {
            if (_immuneActivateSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_immuneActivateSound, _soundEffectVolume);
            }
        }

        private void PlayMoveSound()
        {
            if (_audioController == null || _currentState != BossStoneGolemState.Idle)
            {
                return;
            }

            if (Time.time - _lastMoveSoundTime >= _moveSoundInterval)
            {
                AudioClip clipToPlay = _moveSound;

                if (clipToPlay != null)
                {
                    _audioController.PlayOneShotWithVolume(clipToPlay, _soundEffectVolume * MoveSoundVolumeMultiplier);

                    _lastMoveSoundTime = Time.time;
                }
            }
        }

        public void AnimEvent_AttackHit()
        {
            PlayAttackSound();
            DealMeleeDamage();
        }

        public void AnimEvent_AttackFinish()
        {
            if (!_wasAttackSuccessful)
            {
                PlayAttackMissSound();
            }

            SwitchState(BossStoneGolemState.Idle);
        }

        public void AnimEvent_AttackStart()
        {
            PlayAttackSound();
        }

        public void AnimEvent_LaserShot()
        {
            PlayLaserShootSound();
            DealLaserDamage();
        }

        public void AnimEvent_LaserFinish()
        {
            SwitchState(BossStoneGolemState.Idle);

            _canCheckLaser = true;
        }

        public void AnimEvent_LaserCharge()
        {
            PlayLaserChargeSound();
        }

        public void AnimEvent_Footstep()
        {
            PlayMoveSound();
        }

        public void AnimEvent_ImmuneActivate()
        {
            PlayImmuneActivateSound();
        }

        private void OnDestroy()
        {
            if (_isAggro)
            {
                StopBossMusic();
            }
        }

        public enum BossStoneGolemState
        {
            Idle,
            Attack,
            Immune,
            LaserAttack,
            Death
        }
    }
}
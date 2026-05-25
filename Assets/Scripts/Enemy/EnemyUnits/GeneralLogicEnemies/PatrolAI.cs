using UnityEngine;

namespace GeneralEnemyPatrolSystem
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PatrolAI : MonoBehaviour
    {
        private const float ArrivalThreshold = 0.3f;
        private const float ChaseSpeedMultiplier = 1.5f;
        private const float BoxRotationAngle = 0f;
        private const int LayerNotFound = -1;

        [Header("Patrol Settings")]
        [SerializeField] private Transform _pointA;
        [SerializeField] private Transform _pointB;
        [SerializeField] private float _patrolSpeed = 2f;

        [Header("Chase Settings")]
        [SerializeField] private float _visionRadius = 5f;
        [SerializeField] private float _loseRadius = 5f;
        [SerializeField] private LayerMask _playerLayerMask;

        [Header("Attack Box Settings")]
        [SerializeField] private Vector2 _attackOffset = new Vector2(1.5f, 0.8f);
        [SerializeField] private Vector2 _attackSize = new Vector2(2.5f, 1.6f);

        [Header("Door Interaction")]
        [SerializeField] private float _doorCheckDistance = 1.2f;
        [SerializeField] private LayerMask _doorLayerMask;

        private Rigidbody2D _rigidbody;
        private Transform _playerTransform;
        private HealthManager _playerHealthManager;

        private Vector3 _nextPatrolPoint;

        private bool _isPlayerDead = false;

        public System.Action<Vector2> OnMoveDirectionChanged;
        public System.Action<bool> OnInAttackRange;

        public Vector2 CurrentDirection { get; private set; }
        public bool InAttackRange { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            _nextPatrolPoint = _pointB.position;
            CurrentDirection = (_nextPatrolPoint - transform.position).normalized;
        }

        private void Update()
        {
            if (_isPlayerDead)
            {
                HandlePlayerDeadState();
                return;
            }

            if (_playerTransform == null)
            {
                SearchForPlayer();
            }

            if (_playerTransform != null)
            {
                HandlePlayerChase();
            }
            else
            {
                HandlePatrolState();
            }

            NotifyStateChanges();
        }

        private void HandlePlayerDeadState()
        {
            _playerTransform = null;
            _playerHealthManager = null;

            InAttackRange = false;

            Patrol();
        }

        private void HandlePlayerChase()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            Vector2 attackCenter = CalculateAttackCenter(_attackOffset);

            Collider2D playerInAttackRange = Physics2D.OverlapBox(
                attackCenter,
                _attackSize,
                BoxRotationAngle,
                _playerLayerMask
            );

            InAttackRange = playerInAttackRange != null;

            if (distanceToPlayer > _loseRadius)
            {
                _playerTransform = null;
                _playerHealthManager = null;
                InAttackRange = false;

                return;
            }

            if (InAttackRange)
            {
                StopMovement();
                CheckPlayerDeath();
            }
            else
            {
                MoveToward(_playerTransform.position, _patrolSpeed * ChaseSpeedMultiplier);
            }
        }

        private Vector2 CalculateAttackCenter(Vector2 offset)
        {
            float directionSign = Mathf.Sign(transform.localScale.x);

            return (Vector2)transform.position + new Vector2(directionSign * offset.x, offset.y);
        }

        private void HandlePatrolState()
        {
            InAttackRange = false;

            Patrol();
        }

        private void NotifyStateChanges()
        {
            OnMoveDirectionChanged?.Invoke(CurrentDirection);
            OnInAttackRange?.Invoke(InAttackRange);
        }

        private void Patrol()
        {
            if (Vector2.Distance(transform.position, _nextPatrolPoint) < ArrivalThreshold)
            {
                _nextPatrolPoint = _nextPatrolPoint == _pointA.position ? _pointB.position : _pointA.position;
            }

            MoveToward(_nextPatrolPoint, _patrolSpeed);
        }

        private void MoveToward(Vector3 target, float speed)
        {
            Vector2 direction = (target - transform.position).normalized;

            if (TryInteractWithDoor(direction, out IOpenable door) && door.IsClosed)
            {
                door.Open();

                return;
            }

            _rigidbody.velocity = new Vector2(direction.x * speed, _rigidbody.velocity.y);
            CurrentDirection = direction;
        }

        private bool TryInteractWithDoor(Vector2 direction, out IOpenable door)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, _doorCheckDistance, _doorLayerMask);

            door = hit.collider?.GetComponent<IOpenable>();

            return door != null;
        }

        private void StopMovement()
        {
            _rigidbody.velocity = Vector2.zero;
            CurrentDirection = Vector2.zero;
        }

        private void SearchForPlayer()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, _visionRadius, _playerLayerMask);

            if (hit != null)
            {
                _playerTransform = hit.transform;
                _playerHealthManager = _playerTransform.GetComponent<HealthManager>();

                if (_playerHealthManager != null)
                {
                    _playerHealthManager.OnDeath += HandlePlayerDeath;
                }
            }
            else
            {
                _playerTransform = null;
                _playerHealthManager = null;
            }
        }

        private void CheckPlayerDeath()
        {
            if (_playerHealthManager != null && _playerHealthManager.CurrentHealth <= 0)
            {
                _isPlayerDead = true;
            }
        }

        private void HandlePlayerDeath()
        {
            _isPlayerDead = true;
            _playerTransform = null;

            if (_playerHealthManager != null)
            {
                _playerHealthManager.OnDeath -= HandlePlayerDeath;
                _playerHealthManager = null;
            }
        }

        public void ResetPlayerState()
        {
            _isPlayerDead = false;
        }

        private void OnDestroy()
        {
            if (_playerHealthManager != null)
            {
                _playerHealthManager.OnDeath -= HandlePlayerDeath;
            }
        }
    }
}
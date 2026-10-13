using GameLogic;
using UnityEngine;

public sealed class CoinPhysicsHandler : MonoBehaviour, ICoinPhysicsHandler
{
    private const float DefaultStopVelocityThreshold = 0.1f;
    private const float DefaultBounceDamping = 0.7f;
    private const float MaxAirTime = 5f;
    private const float StopDelaySeconds = 0.5f;
    private const float CollectableStopVelocityMultiplier = 2f;

    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _stopVelocityThreshold = DefaultStopVelocityThreshold;
    [SerializeField] private float _bounceDamping = DefaultBounceDamping;

    private Rigidbody2D _rigidbody;
    private Collider2D _collider;
    private ICoin _coin;
    private bool _isOnGround;
    private bool _isStopped;
    private float _timeInAir;

    public void Initialize(LayerMask groundLayer)
    {
        _groundLayer = groundLayer;

        InitializeComponents();
    }

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        if (_isStopped || _rigidbody == null)
        {
            return;
        }

        UpdateAirTimer();
        StopWhenRestingOnGround();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGroundLayer(collision.gameObject.layer) == false)
        {
            return;
        }

        _isOnGround = true;
        _timeInAir = 0f;

        ApplyBounceDamping();

        if (ShouldStopAfterGroundHit())
        {
            Invoke(nameof(StopPhysics), StopDelaySeconds);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsGroundLayer(collision.gameObject.layer))
        {
            _isOnGround = false;
        }
    }

    private void StopPhysics()
    {
        if (_isStopped || _rigidbody == null)
        {
            return;
        }

        _isStopped = true;

        _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody.velocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;

        if (_collider != null)
        {
            _collider.isTrigger = true;
        }

        Destroy(this);
    }

    private void InitializeComponents()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _coin = GetComponent<ICoin>();
    }

    private void UpdateAirTimer()
    {
        if (_isOnGround)
        {
            return;
        }

        _timeInAir += Time.deltaTime;

        if (_timeInAir > MaxAirTime)
        {
            StopPhysics();
        }
    }

    private void StopWhenRestingOnGround()
    {
        if (_isOnGround == false)
        {
            return;
        }

        if (_rigidbody.velocity.magnitude < _stopVelocityThreshold)
        {
            StopPhysics();
        }
    }

    private bool IsGroundLayer(int layer)
    {
        return (_groundLayer.value & (1 << layer)) != 0;
    }

    private void ApplyBounceDamping()
    {
        if (_rigidbody == null)
        {
            return;
        }

        _rigidbody.velocity *= _bounceDamping;
        _rigidbody.angularVelocity *= _bounceDamping;
    }

    private bool ShouldStopAfterGroundHit()
    {
        return _coin != null &&
               _coin.IsCollectable &&
               _rigidbody != null &&
               _rigidbody.velocity.magnitude < _stopVelocityThreshold * CollectableStopVelocityMultiplier;
    }
}
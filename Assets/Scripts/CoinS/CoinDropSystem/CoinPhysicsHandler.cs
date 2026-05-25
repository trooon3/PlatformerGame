using GameLogic;
using UnityEngine;

public sealed class CoinPhysicsHandler : MonoBehaviour, ICoinPhysicsHandler
{
    private const float DefaultStopVelocityThreshold = 0.1f;
    private const float DefaultBounceDamping = 0.7f;
    private const float MaxAirTime = 5f;

    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _stopVelocityThreshold = DefaultStopVelocityThreshold;
    [SerializeField] private float _bounceDamping = DefaultBounceDamping;

    private Rigidbody2D _rigidbody;
    private Collider2D _collider;
    private ICoin _coin;
    private bool _isOnGround = false;
    private float _timeInAir = 0f;

    public void Initialize(LayerMask groundLayer)
    {
        _groundLayer = groundLayer;

        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _coin = GetComponent<ICoin>();
    }

    private void Start()
    {
        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        if (_collider == null)
        {
            _collider = GetComponent<Collider2D>();
        }

        if (_coin == null) 
        { 
            _coin = GetComponent<ICoin>();
        }
    }

    private void Update()
    {
        if (!_isOnGround)
        {
            _timeInAir += Time.deltaTime;

            if (_timeInAir > MaxAirTime)
            {
                StopPhysics();
            }
        }

        if (_rigidbody.velocity.magnitude < _stopVelocityThreshold && _isOnGround)
        {
            StopPhysics();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int singleBitShift = 1;
        int noLayerMatch = 0;
        float velocityMultiplierFactor = 2f;
        float stopDelaySeconds = 0.5f;

        if (((singleBitShift << collision.gameObject.layer) & _groundLayer) != noLayerMatch)
        {
            _isOnGround = true;

            _rigidbody.velocity *= _bounceDamping;
            _rigidbody.angularVelocity *= _bounceDamping;

            if (_coin != null && _coin.IsCollectable && _rigidbody.velocity.magnitude < _stopVelocityThreshold * velocityMultiplierFactor)
            {
                Invoke(nameof(StopPhysics), stopDelaySeconds);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        int singleBitShift = 1;
        int noLayerMatch = 0;

        if (((singleBitShift << collision.gameObject.layer) & _groundLayer) != noLayerMatch)
        {
            _isOnGround = false;
        }
    }

    public void StopPhysics()
    {
        float zeroAngularVelocity = 0f;

        if (_rigidbody == null)
        {
            return;
        }

        _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody.velocity = Vector2.zero;

        _rigidbody.angularVelocity = zeroAngularVelocity;

        if (_collider != null)
        {
            _collider.isTrigger = true;
        }

        Destroy(this);
    }
}
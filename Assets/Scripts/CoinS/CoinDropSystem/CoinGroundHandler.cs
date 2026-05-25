using GameLogic;
using UnityEngine;

public class CoinGroundHandler : MonoBehaviour
{
    [Header("Ground Settings")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _stopVelocityThreshold = 0.1f;
    [SerializeField] private float _timeToStop = 2f;
    [SerializeField] private float _bounceDamping = 0.7f;

    private Rigidbody2D _rb;
    private Collider2D _collider;
    private bool _isOnGround = false;
    private float _timeInAir = 0f;
    private Coin _coinScript;

    public void Initialize(LayerMask groundLayer)
    {
        _groundLayer = groundLayer;

        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _coinScript = GetComponent<Coin>();
    }

    private void Start()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        if (_collider == null)
        {
            _collider = GetComponent<Collider2D>();
        }

        if (_coinScript == null)
        {
            _coinScript = GetComponent<Coin>();
        }
    }

    private void Update()
    {
        float maxAirTime = 5f;

        if (!_isOnGround)
        {
            _timeInAir += Time.deltaTime;

            if (_timeInAir > maxAirTime)
            {
                StopPhysics();
            }
        }

        if (_rb.velocity.magnitude < _stopVelocityThreshold && _isOnGround)
        {
            StopPhysics();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int singleBitShift = 1;
        int noLayerMatch = 0;
        float velocityMultiplierFactor = 2f;
        float stopDelay = 0.5f;

        if (((singleBitShift << collision.gameObject.layer) & _groundLayer) != noLayerMatch)
        {
            _isOnGround = true;

            _rb.velocity *= _bounceDamping;
            _rb.angularVelocity *= _bounceDamping;

            if (_coinScript != null && _coinScript.IsCollectable &&
                _rb.velocity.magnitude < _stopVelocityThreshold * velocityMultiplierFactor)
            {
                Invoke(nameof(StopPhysics), stopDelay);
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

    private void StopPhysics()
    {
        float rotationSpeed = 0f;

        if (_rb == null)
        {
            return;
        }

        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.velocity = Vector2.zero;

        _rb.angularVelocity = rotationSpeed;

        if (_collider != null)
        {
            _collider.isTrigger = true;
        }

        Destroy(this);
    }
}
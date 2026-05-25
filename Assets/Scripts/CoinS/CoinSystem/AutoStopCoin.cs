using GameLogic;
using UnityEngine;

public sealed class AutoStopCoin : MonoBehaviour
{
    private const float DefaultStopDelay = 0.5f;
    private const float DefaultMinVelocity = 0.1f;
    private const float DefaultMaxLifetime = 10f;

    [Header("Auto Stop Settings")]
    [SerializeField] private float _stopDelay = DefaultStopDelay;
    [SerializeField] private float _minVelocity = DefaultMinVelocity;
    [SerializeField] private float _maxLifetime = DefaultMaxLifetime;

    private Rigidbody2D _rigidbody;
    private Collider2D _collider;
    private ICoin _coin;
    private float _spawnTime;

    private bool _isStopped = false;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        _coin = GetComponent<ICoin>();

        _spawnTime = Time.time;

        Invoke(nameof(StartStopCheck), _stopDelay);

        Destroy(gameObject, _maxLifetime);
    }

    private void StartStopCheck()
    {
        float initialDelay = 0.1f;
        float repeatInterval = 0.2f;

        InvokeRepeating(nameof(CheckForStop), initialDelay, repeatInterval);
    }

    private void CheckForStop()
    {
        float maxLifetimeSeconds = 3f;

        if (_isStopped || _rigidbody == null)
        {
            return;
        }

        if (_rigidbody.velocity.magnitude < _minVelocity || Time.time - _spawnTime > maxLifetimeSeconds)
        {
            StopCoin();
        }
    }

    private void StopCoin()
    {
        float angularVelocity = 0f;
        float selfDestructDelay = 0.1f;

        if (_isStopped || _rigidbody == null)
        {
            return;
        }

        _isStopped = true;

        _rigidbody.velocity = Vector2.zero;
        _rigidbody.angularVelocity = angularVelocity;
        _rigidbody.bodyType = RigidbodyType2D.Kinematic;

        if (_collider != null)
        {
            _collider.isTrigger = true;
        }

        if (_coin != null)
        {
            _coin.EnableCollection();
        }

        CancelInvoke(nameof(CheckForStop));

        Destroy(this, selfDestructDelay);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float velocityDampingFactor = 0.7f;
        float checkDelay = 0.2f;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && _rigidbody != null)
        {
            _rigidbody.velocity *= velocityDampingFactor;

            Invoke(nameof(CheckForStop), checkDelay);
        }
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(CheckForStop));
        CancelInvoke(nameof(StartStopCheck));
    }
}
using System.Collections;
using UnityEngine;

namespace GameLogic
{
    public sealed class Coin : MonoBehaviour, ICoin
    {
        private const float DefaultFloatHeight = 0.1f;
        private const float DefaultFloatSpeed = 2f;
        private const float DefaultCollectableDelay = 1f;

        [Header("Coin Settings")]
        [SerializeField] private WalletManager.CoinType _coinType = WalletManager.CoinType.Bronze;
        [SerializeField] private int _coinValue = 1;
        [SerializeField] private float _collectableDelay = DefaultCollectableDelay;
        [SerializeField] private AudioClip _collectSound;

        [Header("Visual Effects")]
        [SerializeField] private float _floatHeight = DefaultFloatHeight;
        [SerializeField] private float _floatSpeed = DefaultFloatSpeed;

        private bool _isCollectable = false;

        private SpriteRenderer _spriteRenderer;
        private Vector3 _originalPosition;
        private Rigidbody2D _rigidbody;

        public WalletManager.CoinType CoinType => _coinType;
        public int CoinValue => _coinValue;
        public bool IsCollectable => _isCollectable;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rigidbody = GetComponent<Rigidbody2D>();

            _originalPosition = transform.position;
        }

        private void Start()
        {
            Invoke(nameof(EnableCollection), _collectableDelay);
        }

        public void EnableCollection()
        {
            _isCollectable = true;

            float velocityThreshold = 0.5f;
            float retryDelay = 0.5f;

            if (_rigidbody != null && _rigidbody.bodyType == RigidbodyType2D.Dynamic)
            {
                if (_rigidbody.velocity.magnitude < velocityThreshold)
                {
                    _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                    _rigidbody.velocity = Vector2.zero;

                    if (TryGetComponent<Collider2D>(out var collider))
                    {
                        collider.isTrigger = true;
                    }
                }
                else
                {
                    Invoke(nameof(EnableCollection), retryDelay);

                    return;
                }
            }

            StartCoroutine(FloatAnimation());
        }

        private IEnumerator FloatAnimation()
        {
            while (true)
            {
                float yOffset = Mathf.Sin(Time.time * _floatSpeed) * _floatHeight;

                transform.position = _originalPosition + new Vector3(0, yOffset, 0);

                yield return null;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isCollectable || !other.CompareTag("Player"))
            {
                return;
            }

            Collect();
        }

        public void Collect()
        {
            if (WalletManager.Instance != null)
            {
                WalletManager.Instance.AddCoins(_coinType, _coinValue);
            }

            if (_collectSound != null)
            {
                AudioSource.PlayClipAtPoint(_collectSound, transform.position);
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            if (_isCollectable)
            {
                Gizmos.color = Color.green;

                float gizmoRadius = 0.3f;

                Vector3 sphereCenter = transform.position;

                Gizmos.DrawWireSphere(sphereCenter, gizmoRadius);
            }
        }
    }
}
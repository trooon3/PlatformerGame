using System.Collections;
using UnityEngine;

namespace GameLogic
{
    public sealed class Coin : MonoBehaviour, ICoin
    {
        private const string PlayerTag = "Player";
        private const float DefaultFloatHeight = 0.1f;
        private const float DefaultFloatSpeed = 2f;
        private const float DefaultCollectableDelay = 1f;
        private const float StopVelocityThreshold = 0.5f;
        private const float RetryDelay = 0.5f;
        private const float GizmoRadius = 0.3f;

        [Header("Coin Settings")]
        [SerializeField] private WalletManager.CoinType _coinType = WalletManager.CoinType.Bronze;
        [SerializeField] private int _coinValue = 1;
        [SerializeField] private float _collectableDelay = DefaultCollectableDelay;
        [SerializeField] private AudioClip _collectSound;

        [Header("Magnet Settings")]
        [SerializeField] private float _magnetRadius = 4f;
        [SerializeField] private float _magnetInitialSpeed = 3f;
        [SerializeField] private float _magnetAcceleration = 15f;
        [SerializeField] private float _magnetTargetOffsetY = 1f; 
        [SerializeField] private float _collectionDistance = 0.5f; 

        [Header("Visual Effects")]
        [SerializeField] private float _floatHeight = DefaultFloatHeight;
        [SerializeField] private float _floatSpeed = DefaultFloatSpeed;

        private bool _isCollectable;
        private Vector3 _originalPosition;
        private Rigidbody2D _rigidbody;
        private Coroutine _animationCoroutine;

        public WalletManager.CoinType CoinType => _coinType;
        public int CoinValue => _coinValue;
        public bool IsCollectable => _isCollectable;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _originalPosition = transform.position;
        }

        private void Start()
        {
            Invoke(nameof(EnableCollection), _collectableDelay);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isCollectable == false || other.CompareTag(PlayerTag) == false)
            {
                return;
            }

            Collect();
        }

        private void OnDrawGizmos()
        {
            if (_isCollectable)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, GizmoRadius);
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _magnetRadius);
        }

        public void EnableCollection()
        {
            if (_isCollectable)
            {
                return;
            }

            if (TryStopDynamicPhysics() == false)
            {
                Invoke(nameof(EnableCollection), RetryDelay);
                return;
            }

            _isCollectable = true;
            _originalPosition = transform.position;

            StartAnimation();
        }

        public void Collect()
        {
            AddCoinToWallets();
            PlayCollectSound();

            Destroy(gameObject);
        }

        private bool TryStopDynamicPhysics()
        {
            if (_rigidbody == null || _rigidbody.bodyType != RigidbodyType2D.Dynamic)
            {
                return true;
            }

            if (_rigidbody.velocity.magnitude >= StopVelocityThreshold)
            {
                return false;
            }

            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.velocity = Vector2.zero;

            if (TryGetComponent(out Collider2D coinCollider))
            {
                coinCollider.isTrigger = true;
            }

            return true;
        }

        private void StartAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            _animationCoroutine = StartCoroutine(FloatAndMagnetizeAnimation());
        }

        private IEnumerator FloatAndMagnetizeAnimation()
        {
            bool isMagnetizing = false;
            float currentSpeed = _magnetInitialSpeed;

            while (true)
            {
                bool canMagnetize = Hero.Instance != null && Hero.Instance.IsAlive();

                if (canMagnetize && isMagnetizing == false)
                {
                    float distanceToHero = Vector2.Distance(transform.position, Hero.Instance.transform.position);
                    if (distanceToHero <= _magnetRadius)
                    {
                        isMagnetizing = true;
                    }
                }
                else if (canMagnetize == false && isMagnetizing)
                {
                    isMagnetizing = false;
                    _originalPosition = transform.position;
                }

                if (isMagnetizing)
                {
                    Vector3 targetPosition = Hero.Instance.transform.position + new Vector3(0f, _magnetTargetOffsetY, 0f);

                    currentSpeed += _magnetAcceleration * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        targetPosition,
                        currentSpeed * Time.deltaTime
                    );

                    if (Vector3.Distance(transform.position, targetPosition) <= _collectionDistance)
                    {
                        Collect();
                        yield break; 
                    }
                }
                else
                {
                    float yOffset = Mathf.Sin(Time.time * _floatSpeed) * _floatHeight;
                    transform.position = _originalPosition + new Vector3(0f, yOffset, 0f);
                }

                yield return null;
            }
        }

        private void AddCoinToWallets()
        {
            WalletManager.Instance?.AddCoins(_coinType, _coinValue);

            if (PersistentWallet.Instance != null)
            {
                PersistentWallet.Instance.AddCoins(_coinType.ToString(), _coinValue);
            }
        }

        private void PlayCollectSound()
        {
            if (_collectSound == null)
            {
                return;
            }

            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            AudioSource.PlayClipAtPoint(_collectSound, transform.position, sfxVolume);
        }
    }
}
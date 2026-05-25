using System.Collections;
using Player.Input;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerDropOnPlatform
{
    public sealed class PlayerDropThrough : MonoBehaviour
    {
        private const string PlatformLayerName = "Platform";
        private const KeyCode DropKey = KeyCode.S;
        private const float DefaultCheckRadius = 0.5f;
        private const float DefaultDropOffsetY = 1f;
        private const float DefaultDropVelocityY = -0.3f;
        private const float DefaultIgnoreDuration = 0.3f;

        [SerializeField] private float _checkRadius = DefaultCheckRadius;
        [SerializeField] private float _dropOffsetY = DefaultDropOffsetY;
        [SerializeField] private float _dropVelocityY = DefaultDropVelocityY;
        [SerializeField] private float _ignoreDuration = DefaultIgnoreDuration;
        [SerializeField] private Button _mobileDropKey;
        [SerializeField] private IInputProvider _inputProvider;

        private Collider2D _playerCollider;
        private Rigidbody2D _rigidbody;
        private LayerMask _platformLayerMask;
        private bool _dropPressed;

        private void Awake()
        {
            _playerCollider = GetComponent<Collider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _platformLayerMask = LayerMask.GetMask(PlatformLayerName);
            _inputProvider = GetComponentInParent<IInputProvider>() ?? FindObjectOfType<OldInputProvider>();
        }

        private void Start()
        {

            if (_mobileDropKey != null)
                _mobileDropKey.onClick.AddListener(() => _dropPressed = true);
        }

        private void Update()
        {
            if (_inputProvider != null && _inputProvider.IsDropHeroPressed)
            {
                StartCoroutine(PerformDropThrough());
            }
        }

        private void OnDestroy()
        {
            if (_mobileDropKey != null) _mobileDropKey.onClick.RemoveAllListeners();
        }

        private IEnumerator PerformDropThrough()
        {
            Collider2D platform = FindPlatformBelow();

            if (platform == null)
            {
                yield break;
            }

            IgnorePlatformCollision(platform, true);

            ApplyDropMovement();

            yield return new WaitForSeconds(_ignoreDuration);

            IgnorePlatformCollision(platform, false);
        }

        private Collider2D FindPlatformBelow()
        {
            return Physics2D.OverlapCircle(transform.position, _checkRadius, _platformLayerMask);
        }

        private void IgnorePlatformCollision(Collider2D platform, bool ignore)
        {
            Physics2D.IgnoreCollision(_playerCollider, platform, ignore);
        }

        private void ApplyDropMovement()
        {
            Vector2 newPosition = new Vector2(
                _rigidbody.position.x,
                _rigidbody.position.y - _dropOffsetY);

            Vector2 newVelocity = new Vector2(
                _rigidbody.velocity.x,
                _dropVelocityY);

            _rigidbody.position = newPosition;
            _rigidbody.velocity = newVelocity;
        }
    }
}
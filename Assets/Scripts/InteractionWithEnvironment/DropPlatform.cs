using UnityEngine;

namespace PlatformController
{
    public sealed class DropPlatform : MonoBehaviour
    {
        private const KeyCode DropKey = KeyCode.S;
        private const float DefaultDropDuration = 0.3f;

        [SerializeField] private float _dropDuration = DefaultDropDuration;

        private Collider2D _platformCollider;
        private bool _isDropping = false;

        private void Awake()
        {
            _platformCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(DropKey) && !_isDropping)
            {
                StartDrop();
            }
        }

        private void StartDrop()
        {
            _isDropping = true;
            _platformCollider.enabled = false;

            Invoke(nameof(EndDrop), _dropDuration);
        }

        private void EndDrop()
        {
            _platformCollider.enabled = true;
            _isDropping = false;
        }
    }
}
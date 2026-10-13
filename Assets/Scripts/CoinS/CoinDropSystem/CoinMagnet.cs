using UnityEngine;

public class CoinMagnet : MonoBehaviour
{
    [SerializeField] private float _radius = 4f;
    [SerializeField] private float _initialSpeed = 3f;
    [SerializeField] private float _acceleration = 15f;
    [SerializeField] private float _targetOffsetY = 1f;
    [SerializeField] private float _collectDistance = 0.5f;

    private Transform _target;
    private bool _isActive;
    private float _currentSpeed;

    public event System.Action OnReachedTarget;
    public bool IsActive => _isActive;
    public void SetTarget(Transform target) => _target = target;

    public bool TryStartMagnet()
    {
        if (_target == null || _isActive) return false;
        if (Vector2.Distance(transform.position, _target.position) > _radius) return false;

        _isActive = true;
        _currentSpeed = _initialSpeed;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        return true;
    }

    public void StopMagnet() => _isActive = false;

    public void Tick()
    {
        if (!_isActive || _target == null) return;

        _currentSpeed += _acceleration * Time.deltaTime;

        Vector3 targetPos = _target.position + new Vector3(0f, _targetOffsetY, 0f);

        if (transform.position.y > targetPos.y + _collectDistance)
        {
            Vector3 horizontalOnly = new Vector3(transform.position.x, targetPos.y, transform.position.z);
            transform.position = Vector3.MoveTowards(
                horizontalOnly,
                targetPos,
                _currentSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                _currentSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, targetPos) <= _collectDistance)
        {
            _isActive = false;
            OnReachedTarget?.Invoke();
        }
    }
}

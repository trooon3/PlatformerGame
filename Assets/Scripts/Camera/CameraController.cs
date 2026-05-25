using Player;
using UnityEngine;

public sealed class CameraController : MonoBehaviour
{
    private const float DefaultLerpSpeed = 5f;
    private const float DefaultZPosition = -10f;

    [Header("Camera Settings")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, DefaultZPosition);
    [SerializeField] private float _lerpSpeed = DefaultLerpSpeed;

    private void Awake()
    {
        InitializePlayerReference();
    }

    private void LateUpdate()
    {
        FollowPlayer();
    }

    private void InitializePlayerReference()
    {
        if (_playerTransform == null)
        {
            Hero hero = FindFirstObjectByType<Hero>();

            if (hero != null)
            {
                _playerTransform = hero.transform;
            }
        }
    }

    private void FollowPlayer()
    {
        if (_playerTransform == null)
        {
            return;
        }

        Vector3 targetPosition = CalculateTargetPosition();
        UpdateCameraPosition(targetPosition);
    }

    private Vector3 CalculateTargetPosition()
    {
        Vector3 targetPosition = _playerTransform.position + _offset;
        targetPosition.z = DefaultZPosition;
        return targetPosition;
    }

    private void UpdateCameraPosition(Vector3 targetPosition)
    {
        float interpolationFactor = _lerpSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, targetPosition, interpolationFactor);
    }

    public void SetPlayerTransform(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }
}
using UnityEngine;

public class CoinFloating : MonoBehaviour
{
    [SerializeField] private float _height = 0.1f;
    [SerializeField] private float _speed = 2f;

    private Vector3 _originalPosition;
    private bool _isActive;

    public void StartFloating()
    {
        _originalPosition = transform.position;
        _isActive = true;
    }

    public void StopFloating() => _isActive = false;

    public void Tick()
    {
        if (!_isActive) return;

        float yOffset = Mathf.Sin(Time.time * _speed) * _height;
        transform.position = _originalPosition + new Vector3(0f, yOffset, 0f);
    }
}

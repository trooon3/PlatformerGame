using UnityEngine;

public sealed class Checkpoint : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [SerializeField] private string _checkpointId;
    [SerializeField] private AudioClip _activationSound;

    private bool _isActivated = false;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        InitializeCheckpoint();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActivated || !other.CompareTag(PlayerTag))
        {
            return;
        }

        ActivateCheckpoint(other.transform.position);
    }

    private void InitializeCheckpoint()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave())
        {
            var saveData = SaveSystem.Instance.CurrentSave;

            _isActivated = saveData.checkpointId == _checkpointId;
        }

        UpdateVisualState();
    }

    private void ActivateCheckpoint(Vector3 playerPosition)
    {
        _isActivated = true;

        SaveSystem.Instance.SaveGame(_checkpointId, playerPosition);

        PlayActivationEffects();
        UpdateVisualState();
    }

    private void PlayActivationEffects()
    {
        if (_activationSound != null)
        {
            AudioSource.PlayClipAtPoint(_activationSound, transform.position);
        }
    }

    private void UpdateVisualState()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _isActivated ? Color.green : Color.white;
        }
    }

    public string GetCheckpointId() => _checkpointId;
    public bool IsActivated() => _isActivated;
    public Vector3 GetSpawnPosition() => transform.position;
}
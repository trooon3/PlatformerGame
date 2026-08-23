using TMPro;
using UnityEngine;

public sealed class Checkpoint : MonoBehaviour
{
    private const string PlayerTag = "Player";

    private const float TextVerticalOffset = 1.5f;
    private const float TextDestroyDelay = 2f;
    private const int TextFontSize = 10;
    private const float CooldownDuration = 5f;

    [SerializeField] private string _checkpointId;
    [SerializeField] private AudioClip _activationSound;
    [SerializeField] private TMP_FontAsset _saveTextFont;

    private bool _isActivated;
    private bool _isOnCooldown;
    private SpriteRenderer _spriteRenderer;

    private float _nextActivationTime;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        InitializeCheckpoint();
    }

    private void Update()
    {
        if (_isOnCooldown && Time.time >= _nextActivationTime)
        {
            _isOnCooldown = false; 
            UpdateVisualState();   
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerTag))
        {
            return;
        }

        if (Time.time < _nextActivationTime)
        {
            return;
        }

        ActivateCheckpoint(other.transform.position);
    }

    private void InitializeCheckpoint()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave())
        {
            GameSaveData saveData = SaveSystem.Instance.CurrentSave;
            _isActivated = saveData != null && saveData.checkpointId == _checkpointId;
        }

        UpdateVisualState();
    }

    private void ActivateCheckpoint(Vector3 playerPosition)
    {
        _isActivated = true;
        _isOnCooldown = true; 

        _nextActivationTime = Time.time + CooldownDuration;

        UpdateVisualState();
        PlayActivationEffects();
        ShowSaveText();

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame(_checkpointId, playerPosition);
        }

        GameStateManager.MarkGameSaved();
    }

    private void PlayActivationEffects()
    {
        if (_activationSound != null)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            AudioSource.PlayClipAtPoint(_activationSound, transform.position, sfxVolume);
        }
    }

    private void ShowSaveText()
    {
        GameObject textObject = new GameObject("SaveText");
        textObject.transform.position = transform.position + Vector3.up * TextVerticalOffset;

        TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();

        bool isEnglish = LocalizationManager.CurrentLanguage == LocalizationManager.Language.English;
        textMesh.text = isEnglish ? "game Saved" : "игра сохранена";

        if (_saveTextFont != null)
        {
            textMesh.font = _saveTextFont;
        }

        textMesh.color = Color.green;
        textMesh.fontSize = TextFontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.sortingOrder = 10000;

        Destroy(textObject, TextDestroyDelay);
    }

    private void UpdateVisualState()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _isOnCooldown ? Color.green : Color.white;
        }
    }

    public string GetCheckpointId() => _checkpointId;
    public bool IsActivated() => _isActivated;
    public Vector3 GetSpawnPosition() => transform.position;
}
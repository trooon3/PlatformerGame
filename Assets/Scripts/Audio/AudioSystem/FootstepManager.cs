using UnityEngine;

public  class FootstepManager
{
    private readonly AudioSource _audioSource;
    private AudioClip _footstepSound;
    private bool _isLooping;
    private bool _isMoving;

    public FootstepManager(AudioClip footstepSound, bool loop)
    {
        _footstepSound = footstepSound;
        _isLooping = loop;

        _audioSource = new GameObject("FootstepAudioSource").AddComponent<AudioSource>();
        _audioSource.transform.SetParent(Camera.main?.transform ?? null);

        _audioSource.playOnAwake = false;

        _audioSource.loop = _isLooping;
        _audioSource.clip = _footstepSound;
    }

    public bool IsPlaying => _isMoving && _audioSource.isPlaying;

    public void Start()
    {
        if (_footstepSound == null || _isMoving)
        {
            return;
        }

        _isMoving = true;

        if (!_audioSource.isPlaying)
        {
            _audioSource.Play();
        }
    }

    public void Stop()
    {
        if (!_isMoving)
        {
            return;
        }

        _isMoving = false;

        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    public void Pause()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Pause();
        }
    }

    public void Resume()
    {
        if (_isMoving && !_audioSource.isPlaying)
        {
            _audioSource.Play();
        }
    }

    public void SetSound(AudioClip newClip, bool playImmediately = false)
    {
        bool wasPlaying = _audioSource.isPlaying;

        if (wasPlaying)
        {
            _audioSource.Stop();
        }

        _footstepSound = newClip;
        _audioSource.clip = newClip;

        if (playImmediately && _isMoving)
        {
            _audioSource.Play();
        }
    }

    public void SetVolume(float volume)
    {
        _audioSource.volume = Mathf.Clamp01(volume);
    }
}

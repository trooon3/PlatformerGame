using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class FootstepPlayer : MonoBehaviour, IFootstepPlayer
{
    [SerializeField] private AudioClip _footstepSound;
    [SerializeField] private float _volumeMultiplier = 0.7f;
    [SerializeField] private bool _loop = true;

    private FootstepManager _footstepManager;
    private float _sfxVolume = 1f;

    public bool IsPlaying => _footstepManager != null && _footstepManager.IsPlaying;

    private void Awake()
    {
        _footstepManager = new FootstepManager(_footstepSound, _loop);
        UpdateVolume();
    }

    public void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        UpdateVolume();
    }

    public void SetVolumeMultiplier(float multiplier)
    {
        _volumeMultiplier = Mathf.Clamp01(multiplier);
        UpdateVolume();
    }

    public void StartFootsteps() => _footstepManager.Start();
    public void StopFootsteps() => _footstepManager.Stop();
    public void PauseFootsteps() => _footstepManager.Pause();
    public void ResumeFootsteps() => _footstepManager.Resume();
    public void SetSound(AudioClip clip, bool playImmediately = false)
        => _footstepManager.SetSound(clip, playImmediately);

    private void UpdateVolume()
    {
        _footstepManager?.SetVolume(_sfxVolume * _volumeMultiplier);
    }
}

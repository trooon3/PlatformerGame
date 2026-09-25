using UnityEngine;

public interface IFootstepPlayer
{
    bool IsPlaying { get; }

    void SetSfxVolume(float volume);
    void SetVolumeMultiplier(float multiplier);
    void StartFootsteps();
    void StopFootsteps();
    void PauseFootsteps();
    void ResumeFootsteps();
    void SetSound(AudioClip clip, bool playImmediately = false);
}

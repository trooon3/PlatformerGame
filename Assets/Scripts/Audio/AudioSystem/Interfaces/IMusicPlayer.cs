using UnityEngine;

public interface IMusicPlayer
{
    float Volume { get; }

    void SetVolume(float volume);
    void StartBackgroundMusic();
    void PauseBackgroundMusic();
    void ResumeBackgroundMusic();
    void PlayBossMusic(AudioClip bossMusic);
    void StopBossMusic();
    void PlayNextTrack();
    void Update();
}

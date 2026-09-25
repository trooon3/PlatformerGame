using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;
using UnityEngine.Audio;

public sealed class MusicPlayer : MonoBehaviour, IMusicPlayer
{
    [SerializeField] private AudioMixerGroup _musicMixerGroup;
    [SerializeField] private List<AudioClip> _backgroundTracks;

    private AudioSource _musicSource;
    private MusicManager _musicManager;
    private float _volume = 0.5f;

    public float Volume => _volume;

    private void Awake()
    {
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = false;
        _musicSource.playOnAwake = false;
        _musicSource.outputAudioMixerGroup = _musicMixerGroup;

        var playlist = new MusicPlaylist();
        playlist.Initialize(_backgroundTracks);
        _musicManager = new MusicManager(new AudioPlayer(_musicSource), playlist);
    }

    public void SetVolume(float volume)
    {
        _volume = Mathf.Clamp01(volume);
        _musicManager.SetVolume(_volume);
    }

    public void StartBackgroundMusic() => _musicManager.StartBackgroundMusic();
    public void PauseBackgroundMusic() => _musicManager.PauseBackgroundMusic();
    public void ResumeBackgroundMusic() => _musicManager.ResumeBackgroundMusic();
    public void PlayBossMusic(AudioClip bossMusic) => _musicManager.PlayBossMusic(bossMusic);
    public void StopBossMusic() => _musicManager.StopBossMusic();
    public void PlayNextTrack() => _musicManager.PlayNextTrack();

    public void Update() => _musicManager?.Update();
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;

public class AudioController : MonoBehaviour
{
    private const float MinimumVolume = 0f;
    private const float MaximumVolume = 1f;

    [Header("Configuration")]
    [SerializeField] private SoundConfiguration _soundConfiguration;

    [Header("Health Sounds")]
    [SerializeField] private AudioClip _healSound;
    [SerializeField] private AudioClip _takeDamageSound;
    [SerializeField] private AudioClip _deathSound;

    [Header("Detection Sounds")]
    [SerializeField] private AudioClip _enemyDetectedSound;

    [Header("Victory Sound")]
    [SerializeField] private AudioClip _victorySound;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip _buttonClickSound;

    [Header("Background Music")]
    [SerializeField] private List<AudioClip> _backgroundMusicTracks = new List<AudioClip>();

    [Header("Trap Sounds")]
    [SerializeField] private AudioClip _lightningTrapActivationSound;
    [SerializeField] private AudioClip _lightningTrapDamageSound;
    [SerializeField] private AudioClip _lightningTrapDeactivationSound;

    [Header("Movement Sounds")]
    [SerializeField] private AudioClip _footstepSound;
    [SerializeField] private float _footstepVolumeMultiplier = 0.7f;
    [SerializeField] private bool _loopFootstepSound = true;

    [Header("Volume Settings")]
    [Range(0, 1)]
    [SerializeField] private float _musicVolume = 0.5f;
    [Range(0, 1)]
    [SerializeField] private float _soundEffectsVolume = 1f;

    [Header("Door Sounds")]
    [SerializeField] private AudioClip _defaultDoorOpenSound;
    [SerializeField] private AudioClip _defaultDoorCloseSound;

    [Header("Armor Sounds")]
    [SerializeField] private AudioClip _armorDamageSound;
    [SerializeField] private AudioClip _armorBreakSound;
    [SerializeField] private float _armorSoundVolumeMultiplier = 1f;

    private AudioSource _soundEffectsAudioSource;
    private IAudioPlayer _soundEffectsPlayer;
    private IAudioPlayer _musicPlayer;
    private IMusicPlaylist _musicPlaylist;
    private MusicManager _musicManager;
    private FootstepManager _footstepManager;

    public float MusicVolume => _musicVolume;
    public float SoundEffectsVolume => _soundEffectsVolume;
    public bool IsPlayingFootsteps => _footstepManager.IsPlaying;

    private void Awake()
    {
        InitializeAudioSources();
        InitializeMusicSystem();
        InitializeFootstepManager();
        StartBackgroundMusic();
    }

    private void Update()
    {
        _musicManager?.Update();
    }

    private void InitializeAudioSources()
    {
        _soundEffectsAudioSource = GetComponent<AudioSource>();

        if (_soundEffectsAudioSource == null)
        {
            _soundEffectsAudioSource = gameObject.AddComponent<AudioSource>();
        }

        _soundEffectsPlayer = new AudioPlayer(_soundEffectsAudioSource);

        AudioSource musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = false;
        musicSource.playOnAwake = false;

        _musicPlayer = new AudioPlayer(musicSource);
    }

    private void InitializeMusicSystem()
    {
        _musicPlaylist = new MusicPlaylist();
        _musicPlaylist.Initialize(_backgroundMusicTracks);
        _musicManager = new MusicManager(_musicPlayer, _musicPlaylist);

        SetMusicVolume(_musicVolume);
    }

    private void InitializeFootstepManager()
    {
        _footstepManager = new FootstepManager(_footstepSound, _loopFootstepSound);

        UpdateFootstepVolume();
    }

    private void UpdateFootstepVolume()
    {
        _footstepManager.SetVolume(_soundEffectsVolume * _footstepVolumeMultiplier);
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp(volume, MinimumVolume, MaximumVolume);
        _musicPlayer.SetVolume(_musicVolume);
    }

    public void SetSoundEffectsVolume(float volume)
    {
        _soundEffectsVolume = Mathf.Clamp(volume, MinimumVolume, MaximumVolume);
        _soundEffectsPlayer.SetVolume(_soundEffectsVolume);

        UpdateFootstepVolume();
    }

    public void StartFootsteps()
    {
        _footstepManager.Start();
    }

    public void StopFootsteps()
    {
        _footstepManager.Stop();
    }

    public void PauseFootsteps()
    {
        _footstepManager.Pause();
    }

    public void ResumeFootsteps()
    {
        _footstepManager.Resume();
    }

    public void SetFootstepSound(AudioClip newClip, bool playImmediately = false)
    {
        _footstepManager.SetSound(newClip, playImmediately);
    }

    public void SetFootstepVolumeMultiplier(float multiplier)
    {
        _footstepVolumeMultiplier = Mathf.Clamp01(multiplier);

        UpdateFootstepVolume();
    }


    public void PlayAttack1HitSound()
    {
        PlaySoundWithDelay(_soundConfiguration.Attack1HitSound, _soundConfiguration.Attack1SoundDelay);
    }

    public void PlayAttack2HitSound()
    {
        PlaySoundWithDelay(_soundConfiguration.Attack2HitSound, _soundConfiguration.Attack2SoundDelay);
    }

    public void PlayAttack3HitSound()
    {
        PlaySoundWithDelay(_soundConfiguration.Attack3HitSound, _soundConfiguration.Attack3SoundDelay);
    }

    public void PlayAirAttackHitSound()
    {
        PlaySoundWithDelay(_soundConfiguration.AirAttackHitSound, _soundConfiguration.AirAttackSoundDelay);
    }

    public void PlayAttack1MissSound()
    {
        PlaySoundWithDelay(_soundConfiguration.Attack1MissSound, _soundConfiguration.Attack1SoundDelay);
    }

    public void PlayAttack2MissSound()
    {
        PlaySoundWithDelay(_soundConfiguration.Attack2MissSound, _soundConfiguration.Attack2SoundDelay);
    }

    public void PlayAttack3MissSound()
    {
        PlaySoundWithDelay(_soundConfiguration.Attack3MissSound, _soundConfiguration.Attack3SoundDelay);
    }

    public void PlayAirAttackMissSound()
    {
        PlaySoundWithDelay(_soundConfiguration.AirAttackMissSound, _soundConfiguration.AirAttackSoundDelay);
    }

    public void PlayTakeDamageSound()
    {
        PlaySound(_takeDamageSound);
    }

    public void PlayDeathSound()
    {
        PlaySound(_deathSound);
    }

    public void PlayHealSound()
    {
        PlaySound(_healSound);
    }

    public void PlayVictorySound()
    {
        PlaySound(_victorySound);
    }

    public void PlayButtonClickSound()
    {
        PlaySound(_buttonClickSound);
    }

    public void PlayEnemyDetectedSound()
    {
        PlaySound(_enemyDetectedSound);
    }
    public void PlayBossDoorOpenSound()
    {
        PlaySound(_soundConfiguration.BossDoorOpenSound);
    }
    public void PlayLightningTrapActivationSound()
    {
        PlaySound(_lightningTrapActivationSound);
    }
    public void PlayDefaultDoorOpenSound()
    {
        PlaySound(_defaultDoorOpenSound);
    }
    public void PlayDefaultDoorCloseSound()
    {
        PlaySound(_defaultDoorCloseSound);
    }
    public void PlayArmorDamageSound()
    {
        PlayOneShotWithVolume(_armorDamageSound, _armorSoundVolumeMultiplier);
    }

    public void PlayArmorBreakSound()
    {
        PlayOneShotWithVolume(_armorBreakSound, _armorSoundVolumeMultiplier);
    }

    public AudioClip GetLightningTrapActivationSound() => _lightningTrapActivationSound;

    public void PlayOneShot(AudioClip clip)
    {
        if (clip != null)
        {
            _soundEffectsPlayer.PlayOneShot(clip, _soundEffectsVolume);
        }
    }

    public void PlayOneShotWithVolume(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip != null)
        {
            float volume = Mathf.Clamp(_soundEffectsVolume * volumeMultiplier, MinimumVolume, MaximumVolume);
            _soundEffectsPlayer.PlayOneShot(clip, volume);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            _soundEffectsPlayer.PlayOneShot(clip, _soundEffectsVolume);
        }
    }

    private void PlaySoundWithDelay(AudioClip clip, float delay)
    {
        if (clip == null)
        {
            return;
        }

        if (delay <= Mathf.Epsilon)
        {
            PlaySound(clip);
        }
        else
        {
            StartCoroutine(PlayDelayedSoundCoroutine(clip, delay));
        }
    }

    private IEnumerator PlayDelayedSoundCoroutine(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);

        PlaySound(clip);
    }

    public void PlayBossMusic(AudioClip bossMusic)
    {
        _musicManager?.PlayBossMusic(bossMusic);
    }

    public void StopBossMusic()
    {
        _musicManager?.StopBossMusic();
    }

    public void PauseBackgroundMusic()
    {
        _musicManager?.PauseBackgroundMusic();
    }

    public void ResumeBackgroundMusic()
    {
        _musicManager?.ResumeBackgroundMusic();
    }

    public void SkipCurrentTrack()
    {
        _musicManager?.PlayNextTrack();
    }

    private void StartBackgroundMusic()
    {
        _musicManager?.StartBackgroundMusic();
    }
}
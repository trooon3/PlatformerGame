using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class VolumeSettings : MonoBehaviour
{
    private const string MusicPrefsKey = "MusicVolume";
    private const string SFXPrefsKey = "SFXVolume";
    private const float DefaultVolume = 0.8f;

    public static event Action<float> OnMusicVolumeChanged;
    public static event Action<float> OnSFXVolumeChanged;

    [Header("References")]
    [SerializeField] private MusicPlayer _musicPlayer;
    [SerializeField] private SfxPlayer _sfxPlayer;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    private void Start()
    {
        InitializeSliders();
        LoadSavedVolumes();
    }

    private void OnDestroy()
    {
        if (_musicSlider != null)
        {
            _musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
        }
    }

    private void InitializeSliders()
    {
        if (_musicSlider != null)
        {
            _musicSlider.minValue = 0f;
            _musicSlider.maxValue = 1f;
            _musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.minValue = 0f;
            _sfxSlider.maxValue = 1f;
            _sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    private void LoadSavedVolumes()
    {
        float musicVol = PlayerPrefs.GetFloat(MusicPrefsKey, DefaultVolume);
        float sfxVol = PlayerPrefs.GetFloat(SFXPrefsKey, DefaultVolume);

        if (_musicSlider != null) _musicSlider.value = musicVol;
        if (_sfxSlider != null) _sfxSlider.value = sfxVol;

        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
    }

    private void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(MusicPrefsKey, volume);
        PlayerPrefs.Save();

        _musicPlayer?.SetVolume(volume);
        OnMusicVolumeChanged?.Invoke(volume);
    }

    private void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(SFXPrefsKey, volume);

        _sfxPlayer?.SetVolume(volume);
        OnSFXVolumeChanged?.Invoke(volume);
    }
}
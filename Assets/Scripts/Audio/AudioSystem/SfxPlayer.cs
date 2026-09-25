using UnityEngine;
using UnityEngine.Audio;

public sealed class SfxPlayer : MonoBehaviour, ISfxPlayer
{
    [SerializeField] private AudioMixerGroup _sfxMixerGroup;

    private AudioSource _audioSource;
    private float _volume = 1f;

    public float Volume => _volume;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.outputAudioMixerGroup = _sfxMixerGroup;
    }

    public void SetVolume(float volume) => _volume = Mathf.Clamp01(volume);

    public void Play(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        _audioSource.PlayOneShot(clip, Mathf.Clamp01(_volume * volumeMultiplier));
    }
}

public interface ISfxPlayer
{
    float Volume { get; }
    void SetVolume(float volume);
    void Play(AudioClip clip, float volumeMultiplier = 1f);
}

using UnityEngine;
using AudioSystem;

public sealed class EnemyAudioController : MonoBehaviour
{
    [Header("Basic Attack Sounds")]
    [SerializeField] private AudioClip _attackHitSound;
    [SerializeField] private AudioClip _attackMissSound;

    [Header("Special Attack Sounds")]
    [SerializeField] private AudioClip _specialAttackHitSound;
    [SerializeField] private AudioClip _specialAttackMissSound;

    [Header("Damage Sounds")]
    [SerializeField] private AudioClip _hurtSound;
    [SerializeField] private AudioClip _deathSound;

    private IAudioPlayer _audioPlayer;

    private void Awake()
    {
        InitializeAudioPlayer();
    }

    private void InitializeAudioPlayer()
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        _audioPlayer = new AudioPlayer(audioSource);
    }

    public void PlayAttackHitSound()
    {
        PlaySound(_attackHitSound);
    }

    public void PlayAttackMissSound()
    {
        PlaySound(_attackMissSound);
    }

    public void PlaySpecialAttackHitSound()
    {
        PlaySound(_specialAttackHitSound);
    }

    public void PlaySpecialAttackMissSound()
    {
        PlaySound(_specialAttackMissSound);
    }

    public void PlayHurtSound()
    {
        PlaySound(_hurtSound);
    }

    public void PlayDeathSound()
    {
        PlaySound(_deathSound);
    }

    private void PlaySound(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            return;
        }

        _audioPlayer.PlayOneShot(audioClip);
    }

    public void SetAttackHitSound(AudioClip audioClip)
    {
        _attackHitSound = audioClip;
    }

    public void SetAttackMissSound(AudioClip audioClip)
    {
        _attackMissSound = audioClip;
    }

    public void SetSpecialAttackHitSound(AudioClip audioClip)
    {
        _specialAttackHitSound = audioClip;
    }

    public void SetSpecialAttackMissSound(AudioClip audioClip)
    {
        _specialAttackMissSound = audioClip;
    }

    public void SetHurtSound(AudioClip audioClip)
    {
        _hurtSound = audioClip;
    }

    public void SetDeathSound(AudioClip audioClip)
    {
        _deathSound = audioClip;
    }
}
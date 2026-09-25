using UnityEngine;

[CreateAssetMenu(fileName = "SoundConfiguration", menuName = "Audio/Sound Configuration")]
public sealed class SoundConfiguration : ScriptableObject
{
    [Header("Health")]
    public AudioClip HealSound;
    public AudioClip TakeDamageSound;
    public AudioClip DeathSound;

    [Header("Detection")]
    public AudioClip EnemyDetectedSound;

    [Header("Victory")]
    public AudioClip VictorySound;

    [Header("UI")]
    public AudioClip ButtonClickSound;

    [Header("Doors")]
    public AudioClip DefaultDoorOpenSound;
    public AudioClip DefaultDoorCloseSound;
    public AudioClip BossDoorOpenSound;

    [Header("Traps")]
    public AudioClip LightningTrapActivationSound;
    public AudioClip LightningTrapDamageSound;
    public AudioClip LightningTrapDeactivationSound;

    [Header("Armor")]
    public AudioClip ArmorDamageSound;
    public AudioClip ArmorBreakSound;
    [Range(0f, 1f)] public float ArmorSoundVolume = 1f;

    [Header("Attacks")]
    public AudioClip Attack1HitSound;
    public AudioClip Attack2HitSound;
    public AudioClip Attack3HitSound;
    public AudioClip AirAttackHitSound;
    public AudioClip Attack1MissSound;
    public AudioClip Attack2MissSound;
    public AudioClip Attack3MissSound;
    public AudioClip AirAttackMissSound;

    [Header("Attack Delays")]
    public float Attack1SoundDelay;
    public float Attack2SoundDelay;
    public float Attack3SoundDelay;
    public float AirAttackSoundDelay;
}
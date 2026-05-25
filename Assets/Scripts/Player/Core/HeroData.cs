using UnityEngine;

namespace Player
{
    [CreateAssetMenu(menuName = "Player/Hero Data")]
    public sealed class HeroData : ScriptableObject
    {
        private const float DefaultSlideDuration = 0.2f;

        [Header("Health Settings")]
        [SerializeField] private int _maxLives = 6;

        [Header("Movement Settings")]
        [SerializeField] private float _movementSpeed = 3.0f;
        [SerializeField] private float _jumpForce = 8.0f;
        [SerializeField] private float _slideSpeed = 5.0f;
        [SerializeField] private float _slideDuration = DefaultSlideDuration;

        [Header("Combat Settings")]
        [SerializeField] private int _attack1Damage = 1;
        [SerializeField] private int _attack2Damage = 1;
        [SerializeField] private int _superAttackDamage = 4;
        [SerializeField] private int _airAttackDamage = 1;

        [Header("Collider Settings")]
        [SerializeField] private Vector2 _slideColliderOffset;
        [SerializeField] private Vector2 _slideColliderSize;
        [SerializeField] private Vector2 _standColliderOffset;
        [SerializeField] private Vector2 _standColliderSize;

        public int MaxLives => _maxLives;
        public float MovementSpeed => _movementSpeed;
        public float JumpForce => _jumpForce;
        public float SlideSpeed => _slideSpeed;
        public float SlideDuration => _slideDuration;
        public int Attack1Damage => _attack1Damage;
        public int Attack2Damage => _attack2Damage;
        public int SuperAttackDamage => _superAttackDamage;
        public int AirAttackDamage => _airAttackDamage;
        public Vector2 SlideColliderOffset => _slideColliderOffset;
        public Vector2 SlideColliderSize => _slideColliderSize;
        public Vector2 StandColliderOffset => _standColliderOffset;
        public Vector2 StandColliderSize => _standColliderSize;
    }
}
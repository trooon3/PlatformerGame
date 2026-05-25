using GeneralLogicEnemies;
using System.Linq;
using UnityEngine;

public sealed class EnemyDeathHandler : MonoBehaviour, IEnemyDeathHandler
{
    [Header("Death Settings")]
    [SerializeField] private bool _disableColliderOnDeath = true;
    [SerializeField] private bool _disablePhysicsOnDeath = true;
    [SerializeField] private bool _disableAIOnDeath = true;

    private Entity _entity;
    private EnemyRegistration _registration;
    private Collider2D[] _colliders;
    private Rigidbody2D _rigidbody;
    private MonoBehaviour[] _aiComponents;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        _entity = GetComponent<Entity>();
        _registration = GetComponent<EnemyRegistration>();

        _colliders = GetComponents<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();

        _aiComponents = GetComponents<MonoBehaviour>()
            .Where(colliders => colliders != this && colliders != _entity && colliders != _registration &&
                       !(colliders is Transform) && !(colliders is Animator) && !(colliders is SpriteRenderer))
            .ToArray();
    }

    private void OnEnable()
    {
        if (_entity != null)
        {
            _entity.OnEntityDeath += OnEntityDeath;
        }
    }

    private void OnDisable()
    {
        if (_entity != null)
        {
            _entity.OnEntityDeath -= OnEntityDeath;
        }
    }

    private void OnEntityDeath(Entity entity)
    {
        DisableComponents();
    }

    private void DisableComponents()
    {
        if (_disableColliderOnDeath)
        {
            foreach (var collider in _colliders)
            {
                if (collider != null) collider.enabled = false;
            }
        }

        if (_disablePhysicsOnDeath && _rigidbody != null)
        {
            _rigidbody.velocity = Vector2.zero;

            _rigidbody.simulated = false;
        }

        if (_disableAIOnDeath)
        {
            foreach (var aiComponent in _aiComponents)
            {
                if (aiComponent != null)
                {
                    aiComponent.enabled = false;
                }
            }
        }
    }

    public void DisableColliderOnDeath(bool disable)
    {
        _disableColliderOnDeath = disable;
    }

    public void DisablePhysicsOnDeath(bool disable)
    {
        _disablePhysicsOnDeath = disable;
    }

    public void DisableAIOnDeath(bool disable)
    {
        _disableAIOnDeath = disable;
    }
}
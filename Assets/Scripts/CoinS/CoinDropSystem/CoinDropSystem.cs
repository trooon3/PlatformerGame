using UnityEngine;

namespace GeneralLogicEnemies
{
    [RequireComponent(typeof(Entity))]
    public sealed class CoinDropSystem : MonoBehaviour, ICoinDropSystem
    {
        private const float DefaultDropRadius = 1f;
        private const float DefaultVerticalForce = 4f;
        private const float DefaultHorizontalForce = 2f;
        private const float DefaultTorqueForce = 100f;
        private const float DefaultBounciness = 0.3f;

        [Header("Coin Drop Settings")]
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private int _minCoins = 1;
        [SerializeField] private int _maxCoins = 3;

        [Header("Physics Settings")]
        [SerializeField] private float _dropRadius = DefaultDropRadius;
        [SerializeField] private float _verticalForce = DefaultVerticalForce;
        [SerializeField] private float _horizontalForce = DefaultHorizontalForce;
        [SerializeField] private float _torqueForce = DefaultTorqueForce;
        [SerializeField] private LayerMask _groundLayer = 1 << 6;
        [SerializeField] private float _bounciness = DefaultBounciness;

        private Entity _entity;
        private bool _hasDroppedCoins;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _entity = GetComponent<Entity>();

            if (_entity == null)
            {
                _entity = gameObject.AddComponent<Entity>();
            }

            _entity.OnEntityDeath += HandleEntityDeath;
        }

        private void Start()
        {
            if (_entity == null)
            {
                Initialize();
            }
        }

        public void InitializeDropSettings(GameObject coinPrefab, int minCoins, int maxCoins)
        {
            _coinPrefab = coinPrefab;
            _minCoins = minCoins;
            _maxCoins = maxCoins;
        }

        public void DropCoins()
        {
            int incrementForMaxRange = 1;

            if (_hasDroppedCoins || _coinPrefab == null)
            {
                return;
            }

            int coinCount = Random.Range(_minCoins, _maxCoins + incrementForMaxRange);

            for (int i = 0; i < coinCount; i++)
            {
                SpawnCoin(i);
            }

            _hasDroppedCoins = true;
        }

        private void HandleEntityDeath(Entity entity)
        {
            DropCoins();
        }

        private void SpawnCoin(int index)
        {
             float verticalOffset = 0.5f;
             float rotation = 0f;

            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-_dropRadius, _dropRadius), verticalOffset, rotation);

            GameObject coin = Instantiate(_coinPrefab, spawnPosition, Quaternion.identity);

            if (coin == null)
            {
                return;
            }

            SetupCoinPhysics(coin);
        }

        private void SetupCoinPhysics(GameObject coin)
        {
            Rigidbody2D rigidbody = coin.GetComponent<Rigidbody2D>();

            if (rigidbody == null)
            {
                rigidbody = coin.AddComponent<Rigidbody2D>();

                SetupDefaultRigidbody(rigidbody);
            }

            rigidbody.sharedMaterial = CreatePhysicsMaterial();

            ApplyRandomForces(rigidbody);

            var groundHandler = coin.AddComponent<CoinPhysicsHandler>();

            groundHandler.Initialize(_groundLayer);
        }

        private PhysicsMaterial2D CreatePhysicsMaterial()
        {
            return new PhysicsMaterial2D("BouncyCoin")
            {
                bounciness = _bounciness,
                friction = 0.1f
            };
        }

        private void SetupDefaultRigidbody(Rigidbody2D rigidbody)
        {
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            rigidbody.gravityScale = 3f;
            rigidbody.mass = 0.1f;
            rigidbody.drag = 0.5f;
            rigidbody.angularDrag = 0.05f;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void ApplyRandomForces(Rigidbody2D rigidbody)
        {
            float minHorizontalDirection = -1f;
            float maxHorizontalDirection = 1f;

            float horizontalDir = Random.Range(-minHorizontalDirection, maxHorizontalDirection);

            Vector2 force = new Vector2(horizontalDir * _horizontalForce, _verticalForce);

            rigidbody.AddForce(force, ForceMode2D.Impulse);

            float torque = Random.Range(-_torqueForce, _torqueForce);

            rigidbody.AddTorque(torque, ForceMode2D.Impulse);
        }
    }
}
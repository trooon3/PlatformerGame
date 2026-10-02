using UnityEngine;

namespace GeneralLogicEnemies
{
    [RequireComponent(typeof(Entity))]
    public sealed class CoinDropSystem : MonoBehaviour, ICoinDropSystem
    {
        private const float SpawnVerticalOffset = 0.5f;
        private const int RandomRangeMaxOffset = 1;

        [Header("Coin Drop Settings")]
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private int _minCoins = 1;
        [SerializeField] private int _maxCoins = 3;

        [Header("Physics Settings")]
        [SerializeField] private float _dropRadius = 1f;
        [SerializeField] private float _verticalForce = 4f;
        [SerializeField] private float _horizontalForce = 2f;
        [SerializeField] private float _torqueForce = 100f;
        [SerializeField] private float _bounciness = 0.3f;
        [SerializeField] private LayerMask _groundLayer = 1 << 6;

        private CoinFactory _factory;
        private Entity _entity;
        private bool _hasDroppedCoins;

        private void Awake()
        {
            _entity = GetComponent<Entity>();

            _factory = new CoinFactory(
                _coinPrefab,
                _groundLayer,
                _bounciness,
                _verticalForce,
                _horizontalForce,
                _torqueForce);

            if (_entity != null)
                _entity.OnEntityDeath += HandleEntityDeath;
        }

        private void OnDestroy()
        {
            if (_entity != null)
                _entity.OnEntityDeath -= HandleEntityDeath;
        }

        public void DropCoins()
        {
            if (_hasDroppedCoins || _coinPrefab == null)
                return;

            int coinCount = Random.Range(_minCoins, _maxCoins + RandomRangeMaxOffset);

            for (int i = 0; i < coinCount; i++)
            {
                SpawnCoin();
            }

            _hasDroppedCoins = true;
        }

        public void InitializeDropSettings(GameObject coinPrefab, int minCoins, int maxCoins)
        {
            _coinPrefab = coinPrefab;
            _minCoins = Mathf.Max(0, minCoins);
            _maxCoins = Mathf.Max(_minCoins, maxCoins);

            _factory = new CoinFactory(
                _coinPrefab,
                _groundLayer,
                _bounciness,
                _verticalForce,
                _horizontalForce,
                _torqueForce);

            _hasDroppedCoins = false;
        }

        private void HandleEntityDeath(Entity entity)
        {
            DropCoins();
        }

        private void SpawnCoin()
        {
            Vector3 spawnPosition = transform.position + new Vector3(
                Random.Range(-_dropRadius, _dropRadius),
                SpawnVerticalOffset,
                0f);

            _factory.Create(spawnPosition);
        }
    }
}
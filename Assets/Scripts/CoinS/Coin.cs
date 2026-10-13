using UnityEngine;

namespace GameLogic
{
    public sealed class Coin : MonoBehaviour, ICoin
    {
        private const string PlayerTag = "Player";

        [Header("Coin Settings")]
        [SerializeField] private WalletManager.CoinType _coinType = WalletManager.CoinType.Bronze;
        [SerializeField] private int _coinValue = 1;
        [SerializeField] private float _collectableDelay = 1f;
        [SerializeField] private AudioClip _collectSound;

        [Header("Components")]
        [SerializeField] private CoinMagnet _magnet;
        [SerializeField] private CoinFloating _floating;

        private SfxPlayer _sfxPlayer;
        private bool _isCollectable;

        public WalletManager.CoinType CoinType => _coinType;
        public int CoinValue => _coinValue;
        public bool IsCollectable => _isCollectable;

        private void Awake()
        {
            _magnet.OnReachedTarget += Collect;

            if (Hero.Instance != null)
                _magnet.SetTarget(Hero.Instance.transform);
        }

        private void OnDestroy()
        {
            _magnet.OnReachedTarget -= Collect;
        }

        private void Start()
        {
            Invoke(nameof(EnableCollection), _collectableDelay);
        }

        private void Update()
        {
            if (!_isCollectable) return;

            if (_magnet.IsActive)
            {
                _magnet.Tick();
                return; 
            }

            if (_magnet.TryStartMagnet())
            {
                _floating.StopFloating();
                return;
            }

            _floating.Tick();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isCollectable || !other.CompareTag(PlayerTag)) return;
            Collect();
        }

        public void EnableCollection()
        {
            if (_isCollectable) return;
            _isCollectable = true;
            _floating.StartFloating();
        }

        public void Collect()
        {
            if (!_isCollectable) return;
            _isCollectable = false;

            WalletManager.Instance?.AddCoins(_coinType, _coinValue);

            if (_collectSound != null && _sfxPlayer != null)
                _sfxPlayer.Play(_collectSound);

            Destroy(gameObject);
        }

        public void SetSfxPlayer(SfxPlayer player)
        {
            _sfxPlayer = player;
        }
    }
}
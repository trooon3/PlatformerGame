using Player.Input;
using UnityEngine;
using YG;

namespace NPC
{
    public sealed class Merchant : MonoBehaviour, IMerchant
    {
        private const int StateIdle = 0;
        private const int StateIdle2 = 1;
        private const int StateTalk = 2;

        [Header("Input")]
        [SerializeField] private IInputProvider _inputProvider;

        [Header("Animations")]
        [SerializeField] private Animator _animator;

        [Header("Shop Settings")]
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private ShopManager _shopManager;
        [SerializeField] private bool _closeShopOnExit = true;

        [Header("Interaction")]
        [SerializeField] private GameObject _interactionHint;

        [SerializeField] private RectTransform _shopMarker;

        private readonly int _stateHash = Animator.StringToHash("state");

        private bool _isPlayerInRange;
        private bool _isShopOpen;

        public bool IsShopOpen => _isShopOpen;

        private void Start()
        {
            InitializeReferences();
            FindInputProvider();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerInRange = true;
                if (_interactionHint != null) _interactionHint.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerInRange = false;
                if (_interactionHint != null) _interactionHint.SetActive(false);

                if (_closeShopOnExit && _isShopOpen)
                    CloseShop();

                if (_isShopOpen == false)
                    SetAnimation(StateIdle);
            }
        }

        private void Update()
        {
            if (_inputProvider == null)
                FindInputProvider();

            if (_isPlayerInRange && _inputProvider != null && _inputProvider.IsOpenShopOrChestPressed)
            {
                ToggleShop();
            }
        }

        public void OpenShop()
        {
            _isShopOpen = true;
            SetAnimation(StateTalk);
            if (_shopPanel != null) _shopPanel.SetActive(true);
            _shopManager?.OpenShop();
        }

        public void CloseShop()
        {
            _shopManager?.CloseShop();
            if (_shopPanel != null) _shopPanel.SetActive(false);
            _isShopOpen = false;
            SetAnimation(StateIdle);
        }

        public void CloseShopExternal() => CloseShop();

        private void InitializeReferences()
        {
            if (_interactionHint != null)
                _interactionHint.SetActive(false);

            if (_shopPanel != null)
                _shopPanel.SetActive(false);

            if (_shopManager == null && _shopPanel != null)
                _shopManager = _shopPanel.GetComponent<ShopManager>();

            SetAnimation(StateIdle2);
        }

        public void SetMarkerVisible(bool isVisible)
        {
            if (_shopMarker != null)
            {
                _shopMarker.gameObject.SetActive(isVisible);
            }
        }

        public void UpdateMarkerPosition(Vector2 uiPosition)
        {
            if (_shopMarker != null)
            {
                _shopMarker.anchoredPosition = uiPosition;
            }
        }

        private void FindInputProvider()
        {
            if (_inputProvider != null)
                return;

            _inputProvider = FindFirstObjectByType<AggregatedInputProvider>();

            if (_inputProvider == null)
            {
                _inputProvider = FindFirstObjectByType<OldInputProvider>();
            }
        }

        private void ToggleShop()
        {
            if (_isShopOpen)
                CloseShop();
            else
                OpenShop();
        }

        private void SetAnimation(int state)
        {
            if (_animator != null)
                _animator.SetInteger(_stateHash, state);
        }
    }
}
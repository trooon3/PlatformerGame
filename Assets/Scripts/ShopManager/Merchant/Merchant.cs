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
        private const float BoxRotationAngle = 0f;

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

        [Header("Interaction Area")]
        [SerializeField] private Vector2 _triggerSize = new Vector2(1.5f, 1.5f);
        [SerializeField] private LayerMask _playerLayer;

        private readonly int _stateHash = Animator.StringToHash("state");

        private bool _isPlayerInRange;
        private bool _wasPlayerInside;
        private bool _isShopOpen;

        public bool IsShopOpen => _isShopOpen;

        private void Start()
        {
            InitializeReferences();
            FindInputProvider();
        }

        private void Update()
        {
            if (_inputProvider == null)
            {
                FindInputProvider();
            }

            _isPlayerInRange = IsPlayerInsideInteractionArea();

            if (_isPlayerInRange != _wasPlayerInside)
            {
                _wasPlayerInside = _isPlayerInRange;
                UpdateInteractionHint(_isPlayerInRange);

                if (!_isPlayerInRange && _closeShopOnExit && _isShopOpen)
                {
                    CloseShop();
                }
            }

            if (!_isPlayerInRange && !_isShopOpen)
            {
                SetAnimation(StateIdle);
            }

            if (_isPlayerInRange && _inputProvider != null && _inputProvider.IsOpenShopOrChestPressed)
            {
                ToggleShop();
            }
        }

        public void OpenShop()
        {
            _isShopOpen = true;
            SetAnimation(StateTalk);

            if (_interactionHint != null)
                _interactionHint.SetActive(false);

            if (_shopPanel != null)
                _shopPanel.SetActive(true);

            _shopManager?.OpenShop();
        }

        public void CloseShop()
        {
            _shopManager?.CloseShop();

            if (_shopPanel != null)
                _shopPanel.SetActive(false);

            _isShopOpen = false;
            SetAnimation(StateIdle);

            if (_isPlayerInRange && _interactionHint != null)
            {
                _interactionHint.SetActive(true);
            }
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

        private void UpdateInteractionHint(bool isInside)
        {
            if (_isShopOpen) return;

            if (_interactionHint != null)
            {
                _interactionHint.SetActive(isInside);
            }
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

            if (_inputProvider == null && YG2.envir.isDesktop)
                _inputProvider = FindFirstObjectByType<OldInputProvider>();

            if (_inputProvider == null && YG2.envir.isMobile)
                _inputProvider = FindFirstObjectByType<JoystickInput>();

            if (_inputProvider == null)
                Debug.LogWarning("IInputProvider не найден для торговца");
        }

        private bool IsPlayerInsideInteractionArea()
        {
            return Physics2D.OverlapBox(transform.position, _triggerSize, BoxRotationAngle, _playerLayer) != null;
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, _triggerSize);
        }
    }
}
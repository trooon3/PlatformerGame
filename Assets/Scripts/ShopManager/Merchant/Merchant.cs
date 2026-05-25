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
        private const float DefaultInteractionRadius = 2f;

        [Header("Input")]
        [SerializeField] private IInputProvider _inputProvider;

        [Header("Animations")]
        [SerializeField] private Animator _animator;

        [Header("Shop Settings")]
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private ShopManager _shopManager;
        [SerializeField] private bool _closeShopOnExit = true;

        [Header("Interaction Points")]
        [SerializeField] private float _interactionRadius = DefaultInteractionRadius;
        [SerializeField] private Transform _interactionPoint;
        [SerializeField] private GameObject _interactionHint;

        private bool _hasInteracted = false;
        private bool _isPlayerInRange = false;
        private bool _isShopOpen = false;
        private readonly int _stateHash = Animator.StringToHash("state");

        public bool IsShopOpen => _isShopOpen;

        private void Start()
        {
            InitializeReferences();
            FindInputProvider();
        }

        private void InitializeReferences()
        {
            _interactionPoint ??= transform;

            if (_interactionHint != null)
            {
                _interactionHint.SetActive(false);
            }

            if (_shopPanel != null)
            {
                _shopPanel.SetActive(false);
            }

            _shopManager ??= _shopPanel?.GetComponent<ShopManager>();

            SetAnimation(StateIdle2);
        }

        private void Update()
        {
            CheckForPlayer();
            HandlePlayerInput();
        }

        private void CheckForPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                if (_isPlayerInRange)
                {
                    OnPlayerExitRange();
                }

                return;
            }

            float distance = Vector2.Distance(_interactionPoint.position, player.transform.position);

            bool playerInRange = distance <= _interactionRadius;

            if (playerInRange && !_isPlayerInRange)
            {
                OnPlayerEnterRange();
            }
            else if (!playerInRange && _isPlayerInRange)
            {
                OnPlayerExitRange();
            }
        }

        private void FindInputProvider()
        {
            if (_inputProvider == null)
            {
                if (YG2.envir.isDesktop)
                {
                    _inputProvider = FindObjectOfType<OldInputProvider>();
                }
                else if (YG2.envir.isMobile)
                {
                    _inputProvider = FindObjectOfType<JoystickInput>();
                }
            }
            
            if (_inputProvider == null)
                Debug.LogWarning("IInputProvider not found! Map toggle won't work with key.");
        }

        private void HandlePlayerInput()
        {
            if (_isPlayerInRange && _inputProvider.IsOpenShopOrChestPressed)
            {
                if (!_isShopOpen)
                {
                    OpenShop();
                }
                else
                {
                    CloseShop();
                }
            }

            if (_isShopOpen && _inputProvider.IsMenuPressed)
            {
                CloseShop();
            }
        }

        private void OnPlayerEnterRange()
        {
            _isPlayerInRange = true;

            if (_interactionHint != null)
            {
                _interactionHint.SetActive(true);
            }
        }

        private void OnPlayerExitRange()
        {
            _isPlayerInRange = false;

            if (_interactionHint != null)
            {
                _interactionHint.SetActive(false);
            }

            if (_closeShopOnExit && _isShopOpen)
            {
                CloseShop();
            }

            if (_hasInteracted && !_isShopOpen)
            {
                SetAnimation(StateIdle);
            }
        }

        public void OpenShop()
        {
            if (!_hasInteracted)
            {
                _hasInteracted = true;
            }

            SetAnimation(StateTalk);

            if (_shopPanel != null)
            {
                _shopPanel.SetActive(true);
                _isShopOpen = true;

                _shopManager?.OpenShop();
            }
        }

        public void CloseShop()
        {
            _shopManager?.CloseShop();

            if (_shopPanel != null)
            {
                _shopPanel.SetActive(false);
                _isShopOpen = false;
            }

            if (_isPlayerInRange)
            {
                SetAnimation(StateIdle);
            }
        }

        public void CloseShopExternal()
        {
            CloseShop();
        }

        private void SetAnimation(int state)
        {
            if (_animator == null)
            {
                return;
            }

            _animator.SetInteger(_stateHash, state);
        }
    }
}
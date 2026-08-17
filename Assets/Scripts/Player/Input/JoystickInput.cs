using PinePie.SimpleJoystick;
using Player.Input;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed class JoystickInput : MonoBehaviour, IInputProvider
{
    private class ButtonHoldTracker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool IsHeld { get; private set; }

        public void OnPointerDown(PointerEventData eventData) => IsHeld = true;
        public void OnPointerUp(PointerEventData eventData) => IsHeld = false;

        private void OnDisable() => IsHeld = false;
    }

    [SerializeField] private JoystickController _joystick;

    [Header("Gameplay Buttons")]
    [SerializeField] private Button _jumpButton;
    [SerializeField] private Button _attackButton;
    [SerializeField] private Button _secondaryAttackButton;
    [SerializeField] private Button _slideButton;
    [SerializeField] private Button _liftButton;
    [SerializeField] private Button _dropButton;
    [SerializeField] private Button _sprintButton; // Новая кнопка

    [Header("UI Buttons")]
    [SerializeField] private Button _mapButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _shopOrChestButton;

    private ButtonHoldTracker _jumpButtonTracker;

    private bool _isJumpPressed;
    private bool _isAttackPressed;
    private bool _isSecondaryAttackPressed;
    private bool _isSlidePressed;
    private bool _isLiftPressed;
    private bool _isDropPressed;
    private bool _isMapPressed;
    private bool _isMenuPressed;
    private bool _isOpenShopOrChestPressed;
    private bool _isSprintPressed;

    private bool _isInputBlocked;
    private bool _isShopOpen;

    public float HorizontalAxis
    {
        get
        {
            if (IsGameplayInputBlocked())
            {
                return 0f;
            }

            if (_joystick == null)
            {
                return 0f;
            }

            return _joystick.InputDirection.x;
        }
    }

    public bool IsJumpPressed => IsGameplayInputBlocked() == false && _isJumpPressed;
    public bool IsJumpHeld => IsGameplayInputBlocked() == false && (_jumpButtonTracker != null && _jumpButtonTracker.IsHeld);
    public bool IsAttackPressed => IsGameplayInputBlocked() == false && _isAttackPressed;
    public bool IsSecondaryAttackPressed => IsGameplayInputBlocked() == false && _isSecondaryAttackPressed;
    public bool IsSlidePressed => IsGameplayInputBlocked() == false && _isSlidePressed;
    public bool IsLiftPressed => IsGameplayInputBlocked() == false && _isLiftPressed;
    public bool IsDropHeroPressed => IsGameplayInputBlocked() == false && _isDropPressed;
    public bool IsOpenMapPressed => IsGameplayInputBlocked() == false && _isMapPressed;
    public bool IsMenuPressed => _isInputBlocked == false && _isMenuPressed;
    public bool IsOpenShopOrChestPressed => _isInputBlocked == false && _isOpenShopOrChestPressed;
    public bool IsSprintPressed => IsGameplayInputBlocked() == false && _isSprintPressed;

    private void Start()
    {
#if UNITY_WEBGL
        if (YG.YG2.envir.isMobile || YG.YG2.envir.isTablet)
        {
            gameObject.SetActive(true);
        }
#endif
    }

    private void OnEnable()
    {
        SubscribeButtons();
    }

    private void LateUpdate()
    {
        ResetFrameInput();
    }

    private void OnDisable()
    {
        UnsubscribeButtons();
    }

    private void SubscribeButtons()
    {
        if (_jumpButton != null)
        {
            _jumpButton.onClick.AddListener(OnJumpButtonClicked);

            _jumpButtonTracker = _jumpButton.gameObject.GetComponent<ButtonHoldTracker>();
            if (_jumpButtonTracker == null)
            {
                _jumpButtonTracker = _jumpButton.gameObject.AddComponent<ButtonHoldTracker>();
            }
        }

        if (_attackButton != null) _attackButton.onClick.AddListener(OnAttackButtonClicked);
        if (_secondaryAttackButton != null) _secondaryAttackButton.onClick.AddListener(OnSecondaryAttackButtonClicked);
        if (_slideButton != null) _slideButton.onClick.AddListener(OnSlideButtonClicked);
        if (_liftButton != null) _liftButton.onClick.AddListener(OnLiftButtonClicked);
        if (_dropButton != null) _dropButton.onClick.AddListener(OnDropButtonClicked);
        if (_mapButton != null) _mapButton.onClick.AddListener(OnMapButtonClicked);
        if (_menuButton != null) _menuButton.onClick.AddListener(OnMenuButtonClicked);
        if (_shopOrChestButton != null) _shopOrChestButton.onClick.AddListener(OnShopOrChestButtonClicked);
        if (_sprintButton != null) _sprintButton.onClick.AddListener(OnSprintButtonClicked);
    }

    private void UnsubscribeButtons()
    {
        if (_jumpButton != null) _jumpButton.onClick.RemoveListener(OnJumpButtonClicked);
        if (_attackButton != null) _attackButton.onClick.RemoveListener(OnAttackButtonClicked);
        if (_secondaryAttackButton != null) _secondaryAttackButton.onClick.RemoveListener(OnSecondaryAttackButtonClicked);
        if (_slideButton != null) _slideButton.onClick.RemoveListener(OnSlideButtonClicked);
        if (_liftButton != null) _liftButton.onClick.RemoveListener(OnLiftButtonClicked);
        if (_dropButton != null) _dropButton.onClick.RemoveListener(OnDropButtonClicked);
        if (_mapButton != null) _mapButton.onClick.RemoveListener(OnMapButtonClicked);
        if (_menuButton != null) _menuButton.onClick.RemoveListener(OnMenuButtonClicked);
        if (_shopOrChestButton != null) _shopOrChestButton.onClick.RemoveListener(OnShopOrChestButtonClicked);
        if (_sprintButton != null) _sprintButton.onClick.RemoveListener(OnSprintButtonClicked);
    }

    private void ResetFrameInput()
    {
        _isJumpPressed = false;
        _isAttackPressed = false;
        _isSecondaryAttackPressed = false;
        _isSlidePressed = false;
        _isLiftPressed = false;
        _isDropPressed = false;
        _isMapPressed = false;
        _isMenuPressed = false;
        _isOpenShopOrChestPressed = false;
        //_isSprintPressed = false;
    }

    private bool IsGameplayInputBlocked() => _isInputBlocked || _isShopOpen;

    private void OnJumpButtonClicked() => _isJumpPressed = true;
    private void OnAttackButtonClicked() => _isAttackPressed = true;
    private void OnSecondaryAttackButtonClicked() => _isSecondaryAttackPressed = true;
    private void OnSlideButtonClicked() => _isSlidePressed = true;
    private void OnLiftButtonClicked() => _isLiftPressed = true;
    private void OnDropButtonClicked() => _isDropPressed = true;
    private void OnMapButtonClicked() => _isMapPressed = true;
    private void OnMenuButtonClicked() => _isMenuPressed = true;
    private void OnShopOrChestButtonClicked() => _isOpenShopOrChestPressed = true;
    private void OnSprintButtonClicked() => _isSprintPressed = true;

    public void BlockInput(bool isBlocked) => _isInputBlocked = isBlocked;
    public void SetShopMode(bool isShopOpen) => _isShopOpen = isShopOpen;
}
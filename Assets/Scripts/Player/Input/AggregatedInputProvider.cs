using Player.Input;
using UnityEngine;

public sealed class AggregatedInputProvider : MonoBehaviour, IInputProvider
{
    private const float DefaultDeadZone = 0.1f;
    private const float ZeroAxisValue = 0f;

    [Header("Input Sources")]
    [SerializeField] private JoystickInput _joystickInput;
    [SerializeField] private OldInputProvider _keyboardInput;

    [Header("Settings")]
    [SerializeField] private float _deadZone = DefaultDeadZone;

    private bool _isInputBlocked;
    private bool _isShopOpen;

    public float HorizontalAxis
    {
        get
        {
            if (IsGameplayInputBlocked()) return ZeroAxisValue;
            return GetDominantAxis(GetJoystickAxis(), GetKeyboardAxis());
        }
    }

    public bool IsJumpPressed => IsGameplayInputBlocked() == false && IsAnyButtonPressed(_joystickInput?.IsJumpPressed, _keyboardInput?.IsJumpPressed);
    public bool IsJumpHeld => IsGameplayInputBlocked() == false && IsAnyButtonPressed(_joystickInput?.IsJumpHeld, _keyboardInput?.IsJumpHeld);
    public bool IsAttackPressed => IsGameplayInputBlocked() == false && IsAnyButtonPressed(_joystickInput?.IsAttackPressed, _keyboardInput?.IsAttackPressed);
    public bool IsSecondaryAttackPressed => IsGameplayInputBlocked() == false && IsAnyButtonPressed(_joystickInput?.IsSecondaryAttackPressed, _keyboardInput?.IsSecondaryAttackPressed);
    public bool IsSlidePressed => IsGameplayInputBlocked() == false && IsAnyButtonPressed(_joystickInput?.IsSlidePressed, _keyboardInput?.IsSlidePressed);
    public bool IsLiftPressed => IsGameplayInputBlocked() == false && IsAnyButtonPressed(_joystickInput?.IsLiftPressed, _keyboardInput?.IsLiftPressed);
    public bool IsDropHeroPressed => IsGameplayInputBlocked() == false && IsAnyButtonPressed(_joystickInput?.IsDropHeroPressed, _keyboardInput?.IsDropHeroPressed);
    public bool IsOpenMapPressed => IsGameplayInputBlocked() == false && IsAnyButtonPressed(_joystickInput?.IsOpenMapPressed, _keyboardInput?.IsOpenMapPressed);
    public bool IsMenuPressed => _isInputBlocked == false && _isShopOpen == false && IsAnyButtonPressed(_joystickInput?.IsMenuPressed, _keyboardInput?.IsMenuPressed);
    public bool IsOpenShopOrChestPressed => _isInputBlocked == false && IsAnyButtonPressed(_joystickInput?.IsOpenShopOrChestPressed, _keyboardInput?.IsOpenShopOrChestPressed);
    public bool IsSprintPressed => IsGameplayInputBlocked() == false && IsAnyButtonPressed(_joystickInput?.IsSprintPressed, _keyboardInput?.IsSprintPressed);

    private void Awake()
    {
        InitializeInputSources();
#if UNITY_WEBGL
        CheckAndEnableMobileInput();
#endif
    }

    private void CheckAndEnableMobileInput()
    {
        bool isMobile = YG.YG2.envir.isMobile || YG.YG2.envir.isTablet || Application.isMobilePlatform;
        if (isMobile && _joystickInput != null) _joystickInput.gameObject.SetActive(true);
    }

    private void InitializeInputSources()
    {
        if (_joystickInput == null)
        {
            _joystickInput = GetComponent<JoystickInput>();
            if (_joystickInput == null) _joystickInput = FindFirstObjectByType<JoystickInput>();
        }

        if (_keyboardInput == null)
        {
            _keyboardInput = GetComponent<OldInputProvider>();
            if (_keyboardInput == null) _keyboardInput = FindFirstObjectByType<OldInputProvider>();
        }
#if UNITY_WEBGL
        if (_joystickInput != null) _joystickInput.gameObject.SetActive(true);
#endif
    }

    private float GetJoystickAxis()
    {
        if (_joystickInput == null || _joystickInput.enabled == false) return ZeroAxisValue;
        float axis = _joystickInput.HorizontalAxis;
        return Mathf.Abs(axis) > _deadZone ? axis : ZeroAxisValue;
    }

    private float GetKeyboardAxis()
    {
        if (_keyboardInput == null || _keyboardInput.enabled == false) return ZeroAxisValue;
        float axis = _keyboardInput.HorizontalAxis;
        return Mathf.Abs(axis) > _deadZone ? axis : ZeroAxisValue;
    }

    private float GetDominantAxis(float joystickAxis, float keyboardAxis)
    {
        return Mathf.Abs(joystickAxis) > Mathf.Abs(keyboardAxis) ? joystickAxis : keyboardAxis;
    }

    private bool IsAnyButtonPressed(bool? joystickButtonPressed, bool? keyboardButtonPressed)
    {
        bool isJoystickPressed = _joystickInput != null && _joystickInput.enabled && joystickButtonPressed.GetValueOrDefault();
        bool isKeyboardPressed = _keyboardInput != null && _keyboardInput.enabled && keyboardButtonPressed.GetValueOrDefault();
        return isJoystickPressed || isKeyboardPressed;
    }

    private bool IsGameplayInputBlocked() => _isInputBlocked || _isShopOpen;

    public void BlockInput(bool isBlocked)
    {
        _isInputBlocked = isBlocked;
        if (_joystickInput != null) _joystickInput.BlockInput(isBlocked);
        if (_keyboardInput != null) _keyboardInput.BlockInput(isBlocked);
    }

    public void SetShopMode(bool isShopOpen)
    {
        _isShopOpen = isShopOpen;
        if (_joystickInput != null) _joystickInput.SetShopMode(isShopOpen);
        if (_keyboardInput != null) _keyboardInput.SetShopMode(isShopOpen);
    }
}
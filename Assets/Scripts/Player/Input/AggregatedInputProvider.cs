using Player.Input;
using UnityEngine;

public class AggregatedInputProvider : MonoBehaviour, IInputProvider
{
    [Header("Input Sources")]
    [SerializeField] private JoystickInput _joystickInput;
    [SerializeField] private OldInputProvider _keyboardInput;

    [Header("Settings")]
    [SerializeField] private float _deadZone = 0.1f;

    private bool _isInputBlocked = false;
    private bool _isShopOpen = false;

    private void Start()
    {
        LogInputStatus();
    }

    public float HorizontalAxis
    { 
        get
        {
            if (_isInputBlocked || _isShopOpen) return 0f;

            float joystick = GetJoystickAxis(() => _joystickInput?.HorizontalAxis ?? 0f);
            float keyboard = GetKeyboardAxis(() => _keyboardInput?.HorizontalAxis ?? 0f);

            return GetDominantAxis(joystick, keyboard);
        }
    }

    public bool IsJumpPressed
    {
        get
        {
            if (_isInputBlocked || _isShopOpen) return false;
            return GetButtonPress(() => _joystickInput?.IsJumpPressed ?? false,
                                  () => _keyboardInput?.IsJumpPressed ?? false);
        }
    }

    public bool IsOpenShopOrChestPressed
    {
        get
        {
            if (_isInputBlocked || _isShopOpen) return false;
            return GetButtonPress(() => _joystickInput?.IsOpenShopOrChestPressed ?? false,
                                  () => _keyboardInput?.IsOpenShopOrChestPressed ?? false);
        }
    }

    public bool IsAttackPressed
    {
        get
        {
            if (_isInputBlocked || _isShopOpen) return false;
            return GetButtonPress(() => _joystickInput?.IsAttackPressed ?? false,
                                  () => _keyboardInput?.IsAttackPressed ?? false);
        }
    }

    public bool IsSecondaryAttackPressed
    {
        get
        {
            if (_isInputBlocked || _isShopOpen) return false;
            return GetButtonPress(() => _joystickInput?.IsSecondaryAttackPressed ?? false,
                                  () => _keyboardInput?.IsSecondaryAttackPressed ?? false);
        }
    }

    public bool IsSlidePressed
    {
        get
        {
            if (_isInputBlocked || _isShopOpen) return false;
            return GetButtonPress(() => _joystickInput?.IsSlidePressed ?? false,
                                  () => _keyboardInput?.IsSlidePressed ?? false);
        }
    }

    public bool IsLiftPressed
    {
        get
        {
            if (_isInputBlocked || _isShopOpen) return false;
            return GetButtonPress(() => _joystickInput?.IsLiftPressed ?? false,
                                  () => _keyboardInput?.IsLiftPressed ?? false);
        }
    }

    public bool IsDropHeroPressed
    {
        get
        {
            if (_isInputBlocked || _isShopOpen) return false;
            return GetButtonPress(() => _joystickInput?.IsDropHeroPressed ?? false,
                                  () => _keyboardInput?.IsDropHeroPressed ?? false);
        }
    }

    public bool IsOpenMapPressed
    {
        get
        {
            if (_isInputBlocked || _isShopOpen) return false;
            return GetButtonPress(() => _joystickInput?.IsOpenMapPressed ?? false,
                                  () => _keyboardInput?.IsOpenMapPressed ?? false);
        }
    }

    public bool IsMenuPressed
    {
        get
        {
            if (_isInputBlocked) return false;
            return GetButtonPress(() => _joystickInput?.IsMenuPressed ?? false,
                                  () => _keyboardInput?.IsMenuPressed ?? false);
        }
    }

    public void SetShopMode(bool isShopOpen)
    {
        _isShopOpen = isShopOpen;
    }

    private float GetJoystickAxis(System.Func<float> getAxis)
    {
        if (_joystickInput == null || !_joystickInput.enabled) return 0f;
        return getAxis();
    }

    private float GetKeyboardAxis(System.Func<float> getAxis)
    {
        if (_keyboardInput == null || !_keyboardInput.enabled) return 0f;
        return getAxis();
    }

    private float GetDominantAxis(float joystick, float keyboard)
    {
        if (Mathf.Abs(joystick) > Mathf.Abs(keyboard))
            return joystick;
        return keyboard;
    }

    private bool GetButtonPress(System.Func<bool> getJoystick, System.Func<bool> getKeyboard)
    {
        bool joystickPressed = _joystickInput != null && _joystickInput.enabled && getJoystick();
        bool keyboardPressed = _keyboardInput != null && _keyboardInput.enabled && getKeyboard();

        return joystickPressed || keyboardPressed;
    }


    public void BlockInput(bool block)
    {
        _isInputBlocked = block;

        if (_joystickInput != null)
            _joystickInput.BlockInput(block);
        if (_keyboardInput != null)
            _keyboardInput.BlockInput(block);
    }

    private void LogInputStatus()
    {
        Debug.Log($"=== AggregatedInputProvider Status ===");
        Debug.Log($"Joystick active: {_joystickInput != null && _joystickInput.enabled}");
        Debug.Log($"Keyboard active: {_keyboardInput != null && _keyboardInput.enabled}");
        Debug.Log($"Input blocked: {_isInputBlocked}");
        Debug.Log($"Shop mode: {_isShopOpen}");
        Debug.Log($"Jump: {IsJumpPressed}, Attack: {IsAttackPressed}");
    }
}

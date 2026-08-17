using UnityEngine;

namespace Player.Input
{
    public sealed class OldInputProvider : MonoBehaviour, IInputProvider
    {
        private const string HorizontalAxisName = "Horizontal";
        private const string JumpButtonName = "Jump";
        private const string PrimaryAttackButtonName = "Fire1";
        private const string SecondaryAttackButtonName = "Fire2";

        private const KeyCode SlideKey = KeyCode.LeftShift;
        private const KeyCode SprintKey = KeyCode.LeftControl; 
        private const KeyCode LiftKey = KeyCode.E;
        private const KeyCode DropHeroKey = KeyCode.S;
        private const KeyCode MapKey = KeyCode.M;
        private const KeyCode MenuKey = KeyCode.Escape;
        private const KeyCode InteractKey = KeyCode.F;

        private bool _isInputBlocked;
        private bool _isShopOpen;
        private Hero _hero;

        public float HorizontalAxis => IsGameplayInputBlocked() ? 0f : UnityEngine.Input.GetAxisRaw(HorizontalAxisName);

        public bool IsJumpPressed => IsGameplayInputBlocked() == false && UnityEngine.Input.GetButtonDown(JumpButtonName);
        public bool IsJumpHeld => IsGameplayInputBlocked() == false && UnityEngine.Input.GetButton(JumpButtonName);
        public bool IsAttackPressed => IsGameplayInputBlocked() == false && UnityEngine.Input.GetButtonDown(PrimaryAttackButtonName);
        public bool IsSecondaryAttackPressed => IsGameplayInputBlocked() == false && UnityEngine.Input.GetButtonDown(SecondaryAttackButtonName);
        public bool IsLiftPressed => IsGameplayInputBlocked() == false && UnityEngine.Input.GetKeyDown(LiftKey);
        public bool IsDropHeroPressed => IsGameplayInputBlocked() == false && UnityEngine.Input.GetKeyDown(DropHeroKey);

        public bool IsOpenMapPressed => IsGameplayInputBlocked() == false && UnityEngine.Input.GetKeyDown(MapKey);
        public bool IsOpenShopOrChestPressed => IsGameplayInputBlocked() == false && (UnityEngine.Input.GetKeyDown(InteractKey));

        public bool IsMenuPressed => _isInputBlocked == false && _isShopOpen == false && UnityEngine.Input.GetKeyDown(MenuKey);

        public bool IsSprintPressed => IsGameplayInputBlocked() == false && UnityEngine.Input.GetKeyDown(SprintKey);

        public bool IsSlidePressed => IsGameplayInputBlocked() == false &&
                              UnityEngine.Input.GetKeyDown(SlideKey) &&
                              (_hero?.AbilityManager?.HasDash ?? true);

        private void Awake()
        {
            InitializeHeroReference();
        }

        private void InitializeHeroReference()
        {
            _hero = GetComponent<Hero>();

            if (_hero == null)
            {
                _hero = FindFirstObjectByType<Hero>();
            }
        }

        private bool IsGameplayInputBlocked()
        {
            return _isInputBlocked || _isShopOpen || PauseMenuController.IsGamePaused;
        }

        public void BlockInput(bool isBlocked)
        {
            _isInputBlocked = isBlocked;
        }

        public void SetShopMode(bool isShopOpen)
        {
            _isShopOpen = isShopOpen;
        }
    }
}
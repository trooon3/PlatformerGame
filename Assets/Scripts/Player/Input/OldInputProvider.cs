using System.Runtime.InteropServices;
using PlayerDropOnPlatform;
using UnityEngine;

namespace Player.Input
{
    public sealed class OldInputProvider : MonoBehaviour, IInputProvider
    {
        private const string HorizontalAxisName = "Horizontal";
        private const string JumpButtonName = "Jump";
        private const string PrimaryAttackButtonName = "Fire1";
        private const string SecondaryAttackButtonName = "Fire2";
        private const KeyCode SlideKeyCode = KeyCode.LeftShift;
        private const KeyCode LiftKeyCode = KeyCode.E;
        private const KeyCode IsDropHeroKeyCode = KeyCode.S;
        private const KeyCode MapKey = KeyCode.M;
        private const KeyCode MenuKey = KeyCode.Escape;
        private const KeyCode interactKey = KeyCode.F;

        private bool _isInputBlocked = false;
        private bool _isShopOpen = false; 
        private Hero _hero;

        private void Start()
        {
            _hero = GetComponent<Hero>();

            if (_hero == null)
            {
                _hero = FindObjectOfType<Hero>();
            }
        }

        public bool IsSlidePressed
        {
            get
            {
                if (_isInputBlocked)
                {
                    return false;

                }

                if (_hero != null && _hero.AbilityManager != null)
                {
                    return _hero.AbilityManager.HasDash && UnityEngine.Input.GetKeyDown(SlideKeyCode);
                }

                return UnityEngine.Input.GetKeyDown(SlideKeyCode);
            }
        }

        public void BlockInput(bool block)
        {
            _isInputBlocked = block;
        }

        public void SetShopMode(bool isShopOpen)
        {
            _isShopOpen = isShopOpen;
        }

        public float HorizontalAxis =>
            (_isInputBlocked || _isShopOpen) ? 0f : UnityEngine.Input.GetAxisRaw(HorizontalAxisName);

        public bool IsJumpPressed =>
            (_isInputBlocked || _isShopOpen) ? false : UnityEngine.Input.GetButtonDown(JumpButtonName);

        public bool IsAttackPressed =>
            (_isInputBlocked || _isShopOpen) ? false : UnityEngine.Input.GetButtonDown(PrimaryAttackButtonName);

        public bool IsSecondaryAttackPressed =>
            (_isInputBlocked || _isShopOpen) ? false : UnityEngine.Input.GetButtonDown(SecondaryAttackButtonName);

        public bool IsLiftPressed =>
            (_isInputBlocked || _isShopOpen) ? false : UnityEngine.Input.GetKeyDown(LiftKeyCode);
        public bool IsDropHeroPressed =>
            (_isInputBlocked || _isShopOpen) ? false : UnityEngine.Input.GetKeyDown(IsDropHeroKeyCode);
        public bool IsOpenMapPressed => (!_isInputBlocked) && UnityEngine.Input.GetKeyDown(MapKey);
        public bool IsMenuPressed => (!_isInputBlocked) && UnityEngine.Input.GetKeyDown(MenuKey);
        public bool IsOpenShopOrChestPressed => (!_isInputBlocked) && UnityEngine.Input.GetKeyDown(interactKey);
    }
}
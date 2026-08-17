namespace Player.Input
{
    public interface IInputProvider
    {
        float HorizontalAxis { get; }

        bool IsJumpPressed { get; }
        bool IsJumpHeld { get; }
        bool IsAttackPressed { get; }
        bool IsSecondaryAttackPressed { get; }
        bool IsSlidePressed { get; }
        bool IsLiftPressed { get; }
        bool IsDropHeroPressed { get; }
        bool IsOpenMapPressed { get; }
        bool IsMenuPressed { get; }
        bool IsOpenShopOrChestPressed { get; }
        bool IsSprintPressed { get; } 
    }
}
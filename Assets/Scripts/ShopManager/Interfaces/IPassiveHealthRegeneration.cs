public interface IPassiveHealthRegeneration
{
    float RegenerationInterval { get; }
    int HealAmount { get; }
    bool IsActive { get; }

    void EnableRegeneration();
    void DisableRegeneration();
}
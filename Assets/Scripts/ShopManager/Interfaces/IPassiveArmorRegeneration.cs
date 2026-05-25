public interface IPassiveArmorRegeneration
{
    float RegenInterval { get; }
    int RegenAmount { get; }
    bool IsActive { get; }

    void Activate();
    void Deactivate();
}
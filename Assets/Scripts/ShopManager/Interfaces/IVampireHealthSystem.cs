public interface IVampireHealthSystem
{
    int HealthPerKill { get; }
    bool IsActive { get; }

    void Activate();
    void Deactivate();
}
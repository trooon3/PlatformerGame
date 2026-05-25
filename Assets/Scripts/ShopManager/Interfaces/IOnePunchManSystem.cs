using GeneralLogicEnemies;

public interface IOnePunchManSystem
{
    float InstakillChance { get; }
    bool IsActive { get; }

    void Activate();
    void Deactivate();
    bool CheckForInstakill(Entity enemy);
}
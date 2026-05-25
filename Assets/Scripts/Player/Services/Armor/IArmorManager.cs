public interface IArmorManager
{
    int CurrentArmor { get; }
    int MaxArmor { get; }
    bool HasArmor { get; }

    bool IsArmorUnlocked();

    void SetArmor(int amount);
    void AddArmor(int amount);
    int TakeArmorDamage(int damageAmount);
    void FillArmor();
    void ResetArmor();
    void UnlockArmor(); 
    void LoadArmorFromSave(int armorFromSave);
    void UpdateArmorUI();
}
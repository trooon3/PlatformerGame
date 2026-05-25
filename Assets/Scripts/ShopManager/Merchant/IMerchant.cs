namespace NPC
{
    public interface IMerchant
    {
        bool IsShopOpen { get; }

        void OpenShop();
        void CloseShop();
        void CloseShopExternal();
    }
}
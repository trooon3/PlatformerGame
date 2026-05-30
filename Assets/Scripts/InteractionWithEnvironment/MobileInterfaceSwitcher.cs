using UnityEngine;
using YG;

namespace Assets.Scripts.InteractionWithEnvironment
{
    public class MobileInterfaceSwitcher : MonoBehaviour
    {
        [SerializeField] private GameObject _mobilePanel;

        private void Awake()
        {
            CheckMobileDevice();
        }

        private void CheckMobileDevice()
        {
            //if (YG2.envir.isDesktop)
            //{
            //    _mobilePanel.SetActive(false);
            //}

            if(YG2.envir.isMobile)
            {
                _mobilePanel.SetActive(true);
            }
        }
    }
}
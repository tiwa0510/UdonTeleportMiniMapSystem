using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace WaitingFox.Udon
{
    public class ShowStateButton : UdonSharpBehaviour
    {
        public bool DefaultState;
        public GameObject EnableObj;
        public GameObject DisableObj;

        private bool isEnable;

        void Start()
        {
            isEnable = DefaultState;
            if (DefaultState)
            {
                EnableObj.SetActive(true);
                DisableObj.SetActive(false);
            }
            else
            {
                EnableObj.SetActive(false);
                DisableObj.SetActive(true);
            }
        }

        public void ChangeState()
        {
            isEnable = !isEnable;
            EnableObj.SetActive(isEnable);
            DisableObj.SetActive(!isEnable);
        }

        public void SetEnableState()
        {
            isEnable = true;
            EnableObj.SetActive(isEnable);
            DisableObj.SetActive(!isEnable);
        }

        public void SetDisableState()
        {
            isEnable = false;
            EnableObj.SetActive(isEnable);
            DisableObj.SetActive(!isEnable);
        }
    }
}
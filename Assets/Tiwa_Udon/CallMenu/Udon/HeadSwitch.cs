
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace WaitingFox.Udon.CallMenu
{
    // This Udon original version written by hoke
    public class HeadSwitch : UdonSharpBehaviour
    {
        [SerializeField] private GameObject target;

        void Update()
        {
            var localPlayer = Networking.LocalPlayer;
            if (localPlayer == null)
            {
                return;
            }

            if (localPlayer.GetBonePosition(HumanBodyBones.Head) == null)
            {
                transform.position = localPlayer.GetPosition() + new Vector3(0, 0.5f, 0);
            }
            else
            {
                transform.position = localPlayer.GetBonePosition(HumanBodyBones.Head);
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                Interact();
            }   
        }

        public override void Interact()
        {
            target.SetActive(!target.activeSelf);

            if (target.activeSelf)
            {
                var localPlayer = Networking.LocalPlayer;
                if (localPlayer != null)
                {
                    target.transform.rotation = localPlayer.GetRotation();
                }
            }
        }
    }
}
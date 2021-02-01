
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace WaitingFox.Udon.CallMenu
{
    // This Udon original version written by hoke
    public class BodyChaser : UdonSharpBehaviour
    {
        [Tooltip(
            "type of movement\n 0:follow body direction\n 1:follow viewpoint direction\n 2:follow position only\n other:same as zero")]
        public int chaseType = 0;

        [Tooltip("also fix children position")]
        public bool fixChildrenPosition = false;

        GameObject[] children;
        Vector3[] childrenPosition;

        void Start()
        {
            SetChildren();
            if (chaseType < 0 || chaseType > 2)
            {
                chaseType = 0;
            }
        }

        public void SetChildren()
        {
            children = new GameObject[transform.childCount];
            childrenPosition = new Vector3[transform.childCount];
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = transform.GetChild(i).gameObject;
                childrenPosition[i] = children[i].transform.localPosition;
            }
        }

        void Update()
        {
            var localPlayer = Networking.LocalPlayer;
            if (localPlayer != null)
            {
                if (Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Head) == null)
                {
                    transform.position = localPlayer.GetPosition() + new Vector3(0, 0.5f, 0);
                    switch (chaseType)
                    {
                        case 0:
                        case 1:
                            transform.rotation = localPlayer.GetRotation();
                            break;
                    }
                }
                else
                {
                    transform.position = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                    switch (chaseType)
                    {
                        case 0:
                            transform.rotation = localPlayer.GetRotation();
                            break;
                        case 1:
                            transform.rotation =
                                localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                            break;
                    }
                }
            }

            if (fixChildrenPosition)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].transform.localPosition = childrenPosition[i];
                }
            }
        }
    }
}

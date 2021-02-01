using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace WaitingFox.Udon.TeleportMinimap
{
    public class TeleportToSelectPlayer : UdonSharpBehaviour
    {
        public TScrollView scrollView;
        
        public GameObject teleportDialog;
        public Text selectPlayerText;

        public DelayTimer timer;

        private void Start()
        {
            timer.SetTimerCapacity(1);
            teleportDialog.gameObject.SetActive(false);
            selectPlayerText.text = "";
        }

        public void OnClickTeleportButton()
        {
            if (Networking.LocalPlayer == null) return;
            
            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].displayName == selectPlayerText.text)
                {
                    Networking.LocalPlayer.TeleportTo(players[i].GetPosition() + Vector3.back * 2, players[i].GetRotation());
                }
            }
        }

        public void OnClickShowDialog()
        {
            teleportDialog.gameObject.SetActive(true);
            selectPlayerText.text = scrollView.SelectButtonText;
        }

        public void OnClickCancelDialog()
        {
            teleportDialog.gameObject.SetActive(false);
        }

        public void UpdatePlayerList()
        {
            if (Networking.LocalPlayer == null) return;
            
            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);
            
            string[] playersDisplayName = new string[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                playersDisplayName[i] = players[i].displayName;
            }
            
            scrollView.SetContents(playersDisplayName);
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            UpdatePlayerList();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            timer.StartTimer(0, this, nameof(UpdatePlayerList), 1f);
        }
    }
}
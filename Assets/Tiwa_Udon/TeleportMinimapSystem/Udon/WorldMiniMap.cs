using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace WaitingFox.Udon.TeleportMinimap
{
    public class WorldMiniMap : UdonSharpBehaviour
    {
        [SerializeField] private DelayTimer Timer;
        [SerializeField] private GameObject MinimapObject;
        [SerializeField] private TScrollMapView MapView;
        [SerializeField] private Transform PlayerTrackerRoot;
        [SerializeField] private Transform LandmarkTrackerRoot;
        
        [SerializeField] private Vector2 TrackerPosMin;
        [SerializeField] private Vector2 TrackerPosMax;

        [SerializeField] private ShowStateButton EnableButton;
        [SerializeField] private ShowStateButton AutoButton;
        [SerializeField] private ShowStateButton ManualButton;
        
        private RectTransform[] playerTracker;
        private Button[] playerIconButton;
        private Text[] playerName;

        private RectTransform[] LandmarkTracker;
        private Button[] landmarkIconButton;
        private Text[] LandmarkName;

        private bool isActiveMap;
        private bool isFollowPlayer;
        
        private float MapWidth;
        private float MapHeight;
        private float MapSizeFactor;
        private float CamFactor;
        private Vector2 scrollMaxPosition;
        private Vector2 scrollMinPosition;

        private Vector2 MapOffset;
        private Vector2 NowMapLimitMin;
        private Vector2 NowMapLimitMax;

        private Vector2 NowPivot;
        private Vector2 NowMapSize;

        private string selectPlayerName;
        private string selectLandmarkName;

        [SerializeField] private GameObject TeleportDialog;
        [SerializeField] private Text TeleportDialogText;
        
        [SerializeField] private GameObject targetIconPlayer;
        [SerializeField] private GameObject targetIconLandmark;
        
        [Header("User Settings")]
        [SerializeField] private Transform[] LandmarkPoint;
        [SerializeField] private float zoomMaxRange;
        [SerializeField] private Camera SnapshotCamera;

        [Header("Advanced Settings")] 
        [SerializeField] private bool useUserMapImage;
        [SerializeField] private float userMapSize;
        [SerializeField] private Vector2 userMapOffset;

        void Start()
        {
            // UI Cam Initialize
            Timer.SetTimerCapacity(1);
            MinimapObject.gameObject.SetActive(false);
            AutoButton.gameObject.SetActive(false);
            ManualButton.gameObject.SetActive(false);
            PlayerTrackerRoot.gameObject.SetActive(false);
            LandmarkTrackerRoot.gameObject.SetActive(false);
            TeleportDialog.SetActive(false);
            targetIconLandmark.SetActive(false);
            targetIconPlayer.SetActive(false);
            OnClickChangeAutoMode();
            
            // MapView Initalize
            if (!useUserMapImage)
            {
                SnapshotCamera.gameObject.SetActive(false);
                MapView.Setup(SnapshotCamera.orthographicSize, zoomMaxRange);
                MapOffset = new Vector2(SnapshotCamera.transform.position.x, SnapshotCamera.transform.position.z);
            }
            else
            {
                MapView.Setup(userMapSize, zoomMaxRange);
                MapOffset = userMapOffset;
            }

            scrollMaxPosition = MapView.scrollMaxPosition;
            scrollMinPosition = MapView.scrollMinPosition;
            MapSizeFactor = MapView.mapsizeFactor;
            CamFactor = MapView.camFactor;

            if (!useUserMapImage)
            {
                MapWidth = SnapshotCamera.orthographicSize * 2 * MapSizeFactor;
                MapHeight = SnapshotCamera.orthographicSize * 2 * MapSizeFactor;
            }
            else
            {
                MapWidth = userMapSize * 2 * MapSizeFactor;
                MapHeight = userMapSize * 2 * MapSizeFactor;
            }

            // Tracker cache GetComponent
            playerTracker = new RectTransform[PlayerTrackerRoot.childCount];
            playerIconButton = new Button[PlayerTrackerRoot.childCount];
            playerName = new Text[PlayerTrackerRoot.childCount];
            for (int i = 0; i < playerTracker.Length; i++)
            {
                playerTracker[i] = PlayerTrackerRoot.GetChild(i).GetComponent<RectTransform>();
                playerIconButton[i] = PlayerTrackerRoot.GetChild(i).GetComponent<Button>();
                playerName[i] = playerTracker[i].GetComponentInChildren<Text>();
                playerName[i].text = "";
                playerTracker[i].gameObject.SetActive(false);
            }
            
            LandmarkTracker = new RectTransform[LandmarkTrackerRoot.childCount];
            landmarkIconButton = new Button[LandmarkTrackerRoot.childCount];
            LandmarkName = new Text[LandmarkTrackerRoot.childCount];
            for (int i = 0; i < LandmarkTracker.Length; i++)
            {
                LandmarkTracker[i] = LandmarkTrackerRoot.GetChild(i).GetComponent<RectTransform>();
                landmarkIconButton[i] = LandmarkTrackerRoot.GetChild(i).GetComponent<Button>();
                LandmarkName[i] = LandmarkTracker[i].GetComponentInChildren<Text>();
                if (i < LandmarkPoint.Length)
                {
                    if (LandmarkPoint[i] != null)
                    {
                        LandmarkName[i].text = LandmarkPoint[i].name;
                    }
                }
            }
        }

        private void Update()
        {
            if(!isActiveMap) return;
            
            UpdateMap();
            UpdatePlayerTracker();
            UpdateLandmarkTracker();

            if (isFollowPlayer)
            {
                FollowPlayer();
            }
        }

        private void UpdatePlayerTracker()
        {
            if (Networking.LocalPlayer == null) return;

            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);

            for (int i = 0; i < playerTracker.Length; i++)
            {
                if (i < players.Length)
                {
                    playerTracker[i].gameObject.SetActive(true);
                    playerName[i].text = players[i].displayName;
                    playerTracker[i].anchoredPosition = TransformWorldPositionToMiniMap(players[i].GetPosition());
                }
                else
                {
                    playerTracker[i].gameObject.SetActive(false);
                }
            }
        }

        private void UpdateLandmarkTracker()
        {
            for (int i = 0; i < LandmarkTracker.Length; i++)
            {
                if (i < LandmarkPoint.Length)
                {
                    LandmarkTracker[i].anchoredPosition = TransformWorldPositionToMiniMap(LandmarkPoint[i].position);
                }
            }
        }

        private void UpdateMap()
        {
            NowMapSize = (new Vector2(MapWidth, MapHeight)) / (MapView.zoomValue * MapSizeFactor);

            Vector2 pivot = new Vector2(Mathf.Lerp(scrollMinPosition.x * CamFactor, scrollMaxPosition.x * CamFactor, MapView.RectHandle.handleValueH), 
                                        Mathf.Lerp(scrollMinPosition.y * CamFactor, scrollMaxPosition.y * CamFactor, MapView.RectHandle.handleValueV));

            NowPivot = (pivot) / (-1 * MapView.zoomValue) * MapView.zoomFactor;

            NowMapLimitMin = new Vector2(NowPivot.x - NowMapSize.x / 2f, NowPivot.y - NowMapSize.y / 2f) + MapOffset;
            NowMapLimitMax = new Vector2(NowPivot.x + NowMapSize.x / 2f, NowPivot.y + NowMapSize.y / 2f) + MapOffset;
        }

        private Vector2 TransformWorldPositionToMiniMap(Vector3 targetPosition)
        {
            float x = Mathf.Clamp(targetPosition.x, NowMapLimitMin.x, NowMapLimitMax.x);
            float tx = LerpFactor(x, NowMapLimitMin.x, NowMapLimitMax.x);
            x = Mathf.Lerp(TrackerPosMin.x, TrackerPosMax.x, tx);
            
            float y = Mathf.Clamp(targetPosition.z, NowMapLimitMin.y, NowMapLimitMax.y);
            float ty = LerpFactor(y, NowMapLimitMin.y, NowMapLimitMax.y);
            y = Mathf.Lerp(TrackerPosMin.y, TrackerPosMax.y, ty);

            return new Vector2(x, y);
        }

        private void FollowPlayer()
        {
            if(Networking.LocalPlayer == null) return;

            Vector3 playerPos = Networking.LocalPlayer.GetPosition();
            
            playerPos = (playerPos - new Vector3(MapOffset.x, 0, MapOffset.y)) * (-1 * MapView.zoomValue) / MapView.zoomFactor;
            float nextHandleValueH = LerpFactor(playerPos.x, scrollMinPosition.x * CamFactor, scrollMaxPosition.x * CamFactor);
            float nextHandleValueV = LerpFactor(playerPos.z, scrollMinPosition.y * CamFactor, scrollMaxPosition.y * CamFactor);
            
            MapView.RectHandle.SetHandlePosition(nextHandleValueH, nextHandleValueV);
        }

        private float LerpFactor(float y, float a, float b)
        {
            return y / (b - a) - a / (b - a);
        }

        public void OnClickChangeActiveMap()
        {
            isActiveMap = !isActiveMap;
            EnableButton.ChangeState();
            if (isActiveMap)
            {
                if (!useUserMapImage)
                {
                    SnapshotCamera.gameObject.SetActive(true);
                    Timer.StartTimer(0, this, nameof(DisableSnapshotCamera), 0.1f);
                }

                MinimapObject.SetActive(true);
                AutoButton.gameObject.SetActive(true);
                ManualButton.gameObject.SetActive(true);
                PlayerTrackerRoot.gameObject.SetActive(true);
                LandmarkTrackerRoot.gameObject.SetActive(true);
            }
            else
            {
                MinimapObject.SetActive(false);
                AutoButton.gameObject.SetActive(false);
                ManualButton.gameObject.SetActive(false);
                PlayerTrackerRoot.gameObject.SetActive(false);
                LandmarkTrackerRoot.gameObject.SetActive(false);
            }
        }

        public void DisableSnapshotCamera()
        {
            SnapshotCamera.gameObject.SetActive(false);
        }
        
        public void OnClickChangeAutoMode()
        {
            isFollowPlayer = true;
            AutoButton.SetEnableState();
            ManualButton.SetDisableState();
            MapView.RectHandle.SetHandleActive(false);
        }

        public void OnClickChangeManualMode()
        {
            isFollowPlayer = false;
            AutoButton.SetDisableState();
            ManualButton.SetEnableState();
            MapView.RectHandle.SetHandleActive(true);
        }

        public void OnClickPlayerIcon()
        {
            for (int i = 0; i < playerIconButton.Length; i++)
            {
                if (playerIconButton[i].interactable) continue;

                playerIconButton[i].interactable = true;
                ShowTeleportDialog(playerName[i].text);
                targetIconPlayer.SetActive(true);
                targetIconLandmark.SetActive(false);
            }
        }

        public void OnClickLandmarkIcon()
        {
            for (int i = 0; i < landmarkIconButton.Length; i++)
            {
                if (landmarkIconButton[i].interactable) continue;

                landmarkIconButton[i].interactable = true;
                ShowTeleportDialog(LandmarkName[i].text);
                targetIconPlayer.SetActive(false);
                targetIconLandmark.SetActive(true);
            }
        }

        public void ShowTeleportDialog(string selectText)
        {
            TeleportDialog.SetActive(true);
            TeleportDialogText.text = selectText;
        }

        public void OnClickTeleportButton()
        {
            if(TeleportDialogText.text == "") return;
            
            if (Networking.LocalPlayer == null) return;

            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);

            for (int i = 0; i < players.Length; i++)
            {
                if (TeleportDialogText.text == players[i].displayName)
                {
                    Networking.LocalPlayer.TeleportTo(players[i].GetPosition() + Vector3.back * 2, players[i].GetRotation());
                    return;
                }
            }

            for (int i = 0; i < LandmarkPoint.Length; i++)
            {
                if (TeleportDialogText.text == LandmarkPoint[i].gameObject.name)
                {
                    Networking.LocalPlayer.TeleportTo(LandmarkPoint[i].position, LandmarkPoint[i].rotation);
                    return;
                }
            }
        }

        public void OnClickDisableTeleportDialog()
        {
            TeleportDialog.SetActive(false);
            TeleportDialogText.text = "";
        }
    }
}
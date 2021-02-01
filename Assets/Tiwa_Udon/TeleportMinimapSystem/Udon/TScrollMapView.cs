using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace WaitingFox.Udon.TeleportMinimap
{
    public class TScrollMapView : UdonSharpBehaviour
    {
        [SerializeField] private Transform Content;
        [SerializeField] public TScrollViewHandle ZoomHandle;
        [SerializeField] public TScrollViewRectHandle RectHandle;
        
        [HideInInspector] public Vector2 scrollMinPosition;
        [HideInInspector] public Vector2 scrollMaxPosition;

        [HideInInspector] public float zoomValue;
        [HideInInspector] public float zoomFactor;
        [HideInInspector] public float mapsizeFactor;
        [HideInInspector] public float camFactor;

        private float zoomRangeMax;
        private float zoomMax;
        
        private RectTransform contentRectTransform;
        
        private bool initalize;

        public void Setup(float orthographicSize, float _zoomRangeMax)
        {
            ZoomHandle.SetHandlePosition(0.5f);

            zoomRangeMax = _zoomRangeMax;
            contentRectTransform = Content.GetComponent<RectTransform>();

            Vector2 movableRange = (contentRectTransform.sizeDelta * zoomRangeMax - contentRectTransform.sizeDelta) / 2;
            mapsizeFactor = movableRange.x * 2 / contentRectTransform.sizeDelta.x;
            scrollMinPosition = new Vector2(movableRange.x, -movableRange.y);
            scrollMaxPosition = new Vector2(-movableRange.x, movableRange.y);

            camFactor = orthographicSize / (contentRectTransform.sizeDelta.x / 2);
            zoomMax = zoomRangeMax * camFactor;
            
            initalize = true;
        }

        private void Update()
        {
            if(!initalize) return;
            
            zoomValue = Mathf.Lerp(zoomMax, 1, ZoomHandle.handleValue);
            zoomFactor = 1 - ((zoomMax - zoomValue) / (zoomMax - 1));

            contentRectTransform.localScale = new Vector3( zoomValue, zoomValue, 1);
            
            contentRectTransform.anchoredPosition = new Vector2(Mathf.Lerp(scrollMinPosition.x, scrollMaxPosition.x, RectHandle.handleValueH), Mathf.Lerp(scrollMinPosition.y, scrollMaxPosition.y, RectHandle.handleValueV)) * zoomFactor;
        }
    }
}
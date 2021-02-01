using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace WaitingFox.Udon.TeleportMinimap
{
    public class TScrollViewHandle : UdonSharpBehaviour
    {
        [SerializeField] private bool isHorizontal;
        [SerializeField] private RectTransform StartPoint;
        [SerializeField] private RectTransform EndPoint;

        public bool hasHandle;
        private bool isStartUpper;
        private float scrollBarDistance;
        private bool initalized;
        private RectTransform thisRectTransform;

        public float handleValue;

        private void Start()
        {
            Initalize();
        }

        private void Initalize()
        {
            if(initalized) return;
            
            isStartUpper = isHorizontal
                ? StartPoint.localPosition.x > EndPoint.localPosition.x
                : StartPoint.localPosition.y > EndPoint.localPosition.y;
            thisRectTransform = GetComponent<RectTransform>();
            scrollBarDistance = isHorizontal
                ? Mathf.Abs(EndPoint.anchoredPosition.x - StartPoint.anchoredPosition.x)
                : Mathf.Abs(EndPoint.anchoredPosition.y - StartPoint.anchoredPosition.y);
            initalized = true;
        }

        private void Update()
        {
            if (!hasHandle) return;

            if (isHorizontal)
            {
                HandleUpdate(transform.localPosition.x, StartPoint.localPosition.x, EndPoint.localPosition.x, 
                    thisRectTransform.anchoredPosition.x, StartPoint.anchoredPosition.x, EndPoint.anchoredPosition.x);
            }
            else
            {
                HandleUpdate(transform.localPosition.y, StartPoint.localPosition.y, EndPoint.localPosition.y, 
                    thisRectTransform.anchoredPosition.y, StartPoint.anchoredPosition.y, EndPoint.anchoredPosition.y);
            }
        }

        private void HandleUpdate(float nowPoint, float startPoint, float endPoint, float nowAnchoredPoint, float startAnchoredPoint, float endAnchoredPoint)
        {
            float handleAnchoredPos = isStartUpper
                ? Mathf.Clamp(nowAnchoredPoint, endAnchoredPoint,startAnchoredPoint)
                : Mathf.Clamp(nowAnchoredPoint, startAnchoredPoint,endAnchoredPoint);

            float handleLocalPos = isStartUpper
                ? Mathf.Clamp(nowPoint, endPoint, startPoint)
                : Mathf.Clamp(nowPoint, startPoint, endPoint);

            if (isHorizontal)
            {
                transform.localPosition = new Vector3(handleLocalPos, StartPoint.localPosition.y, StartPoint.localPosition.z);
            }
            else
            {
                transform.localPosition = new Vector3(StartPoint.localPosition.x, handleLocalPos, StartPoint.localPosition.z);
            }

            transform.localRotation = Quaternion.identity;
            handleValue = Mathf.Clamp01(Mathf.Abs(handleAnchoredPos / scrollBarDistance));
        }

        public void SetHandlePosition(float value)
        {
            Initalize();
            
            value = Mathf.Clamp01(value);
            handleValue = value;
            float newPoint = scrollBarDistance * value;

            if (isHorizontal)
            {
                transform.localPosition = new Vector2(Mathf.Sign(EndPoint.localPosition.x) * newPoint, StartPoint.localPosition.y);
            }
            else
            {
                transform.localPosition = new Vector2(StartPoint.localPosition.x, Mathf.Sign(EndPoint.localPosition.y) * newPoint);
            }
        }

        public override void OnPickup()
        {
            hasHandle = true;
        }

        public override void OnDrop()
        {
            hasHandle = false;
        }
    }
}

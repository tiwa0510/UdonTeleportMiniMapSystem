using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace WaitingFox.Udon.TeleportMinimap
{
    public class TScrollViewRectHandle : UdonSharpBehaviour
    {
        [SerializeField] private GameObject HandleVisual;
        [SerializeField] private BoxCollider HandleCollider;
        [SerializeField] private RectTransform HandleRectTransform;
        
        [SerializeField] private RectTransform StartPointH;
        [SerializeField] private RectTransform EndPointH;
        
        private bool isStartUpperH;
        private float scrollBarDistanceH;
        
        [SerializeField] private RectTransform StartPointV;
        [SerializeField] private RectTransform EndPointV;

        private bool isStartUpperV;
        private float scrollBarDistanceV;
        
        [HideInInspector] public float handleValueH;
        [HideInInspector] public float handleValueV;

        public bool hasHandle;
        private bool initalized;
        
        private void Start()
        {
            Initalize();
        }

        private void Initalize()
        {
            if(initalized) return;
            
            isStartUpperH = StartPointH.localPosition.x > EndPointH.localPosition.x; 
            isStartUpperV = StartPointV.localPosition.y > EndPointV.localPosition.y;

            scrollBarDistanceH = Mathf.Abs(EndPointH.anchoredPosition.x - StartPointH.anchoredPosition.x);
            scrollBarDistanceV = Mathf.Abs(EndPointV.anchoredPosition.y - StartPointV.anchoredPosition.y);
            
            initalized = true;
        }

        private void Update()
        {
            if (!hasHandle) return;
            
            HandleUpdate(true, transform.localPosition.x, StartPointH.localPosition.x, EndPointH.localPosition.x, 
                HandleRectTransform.anchoredPosition.x, StartPointH.anchoredPosition.x, EndPointH.anchoredPosition.x);
            HandleUpdate(false, transform.localPosition.y, StartPointV.localPosition.y, EndPointV.localPosition.y, 
                HandleRectTransform.anchoredPosition.y, StartPointV.anchoredPosition.y, EndPointV.anchoredPosition.y);
        }

        private void HandleUpdate(bool isHorizontal, float nowPoint, float startPoint, float endPoint, float nowAnchoredPoint, float startAnchoredPoint, float endAnchoredPoint)
        {
            if (isHorizontal)
            {
                float handleAnchoredPos = isStartUpperH
                    ? Mathf.Clamp(nowAnchoredPoint, endAnchoredPoint,startAnchoredPoint)
                    : Mathf.Clamp(nowAnchoredPoint, startAnchoredPoint,endAnchoredPoint);

                float handleLocalPos = isStartUpperH
                    ? Mathf.Clamp(nowPoint, endPoint, startPoint)
                    : Mathf.Clamp(nowPoint, startPoint, endPoint);
                
                transform.localPosition = new Vector3(handleLocalPos, transform.localPosition.y, StartPointH.localPosition.z);
                handleValueH = Mathf.Clamp01(Mathf.Abs(handleAnchoredPos / scrollBarDistanceH));
            }
            else
            {
                float handleAnchoredPos = isStartUpperV
                    ? Mathf.Clamp(nowAnchoredPoint, endAnchoredPoint,startAnchoredPoint)
                    : Mathf.Clamp(nowAnchoredPoint, startAnchoredPoint,endAnchoredPoint);

                float handleLocalPos = isStartUpperV
                    ? Mathf.Clamp(nowPoint, endPoint, startPoint)
                    : Mathf.Clamp(nowPoint, startPoint, endPoint);
                
                transform.localPosition = new Vector3(transform.localPosition.x, handleLocalPos, StartPointH.localPosition.z);
                handleValueV = Mathf.Clamp01(Mathf.Abs(handleAnchoredPos / scrollBarDistanceV));
            }

            transform.localRotation = Quaternion.identity;
        }

        public void SetHandlePosition(float valueH, float valueV)
        {
            Initalize();
            
            valueH = Mathf.Clamp01(valueH);
            handleValueH = valueH;
            float newPointH = scrollBarDistanceH * valueH;
            
            valueV = Mathf.Clamp01(valueV);
            handleValueV = valueV;
            float newPointV = scrollBarDistanceV * valueV;

            transform.localPosition = new Vector2(Mathf.Sign(EndPointH.localPosition.x) * newPointH, Mathf.Sign(EndPointV.localPosition.y) * newPointV);
        }

        public override void OnPickup()
        {
            hasHandle = true;
        }

        public override void OnDrop()
        {
            hasHandle = false;
        }

        public void SetHandleActive(bool isShow)
        {
            HandleVisual.SetActive(isShow);
            HandleCollider.enabled = isShow;
        }
    }
}

using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace WaitingFox.Udon.TeleportMinimap
{
    public class TScrollView : UdonSharpBehaviour
    {
        [SerializeField] private int VisiableButtonMaxCount;
        [SerializeField] private float ButtonHeight;
        [SerializeField] private Transform Content;

        public string SelectButtonText;

        private Button[] buttons;
        private Text[] texts;

        public TScrollViewHandle handle;

        private int activeCount;
        private RectTransform contentRectTransform;

        void Start()
        {
            contentRectTransform = Content.GetComponent<RectTransform>();
            SelectButtonText = "";
            buttons = new Button[Content.childCount];
            texts = new Text[Content.childCount];
            for (int i = 0; i < Content.childCount; i++)
            {
                buttons[i] = Content.GetChild(i).GetComponent<Button>();
                texts[i] = buttons[i].GetComponentInChildren<Text>();
                texts[i].text = "";
                buttons[i].gameObject.SetActive(true);
                activeCount = buttons.Length - VisiableButtonMaxCount;
            }
        }

        private void Update()
        {
            if (!handle.hasHandle) return;

            float positionY = activeCount * ButtonHeight * handle.handleValue;
            contentRectTransform.anchoredPosition = new Vector2(contentRectTransform.anchoredPosition.x, positionY);
        }

        public void OnClickPlayerListButton()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].interactable) continue;

                buttons[i].interactable = true;
                SelectButtonText = texts[i].text;
            }
        }

        public void SetContents(string[] contents)
        {
            activeCount = -VisiableButtonMaxCount;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i < contents.Length)
                {
                    buttons[i].gameObject.SetActive(true);
                    texts[i].text = contents[i];
                    activeCount++;
                }
                else
                {
                    buttons[i].gameObject.SetActive(false);
                }
            }

            activeCount = Mathf.Clamp(activeCount, 0, buttons.Length);
        }
    }
}
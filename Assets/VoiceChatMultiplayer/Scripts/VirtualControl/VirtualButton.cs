using UnityEngine;
using UnityEngine.EventSystems;
namespace VoiceChatMultiplayer.VirtualControl
{
    public class VirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool IsPress { get; private set; }
        RectTransform rect = default;
        private void Start()
        {
            rect = GetComponent<RectTransform>();
        }
        public void OnPointerDown(PointerEventData data)
        {
            IsPress = true;
            rect.localScale = Vector3.one * 1.1f;
        }

        public void OnPointerUp(PointerEventData data)
        {
            IsPress = false;
            rect.localScale = Vector3.one;
        }
    }
}
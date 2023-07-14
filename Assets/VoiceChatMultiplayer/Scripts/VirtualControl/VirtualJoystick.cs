using UnityEngine;
using UnityEngine.EventSystems;
namespace VoiceChatMultiplayer.VirtualControl
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] RectTransform thumb = default;
        [SerializeField] float radius = 150;
        public bool IsEnter { get; private set; }
        public Vector2 Direction { get; private set; }
        private Vector2 centerPoint;
        RectTransform rect = default;
        private void Start()
        {
            rect = GetComponent<RectTransform>();
        }
        public void OnPointerDown(PointerEventData data)
        {
            IsEnter = true;
            rect.localScale = Vector3.one * 1.1f;
            centerPoint = RectTransformUtility.WorldToScreenPoint(data.pressEventCamera, rect.position);
        }

        public void OnPointerUp(PointerEventData data)
        {
            IsEnter = false;
            rect.localScale = Vector3.one;
            thumb.anchoredPosition = Vector2.zero;
            Direction = Vector2.zero;
        }

        public void OnDrag(PointerEventData data)
        {
            if (!IsEnter) return;
            Vector2 pointerPos = data.position;
            Vector2 dir = pointerPos - centerPoint;
            Direction = dir.normalized * Mathf.Clamp(dir.magnitude, 0, radius) / radius;
            thumb.anchoredPosition = Direction * radius;
        }
    }
}
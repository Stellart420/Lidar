using UnityEngine;
using UnityEngine.EventSystems;

namespace VoiceChatMultiplayer.VirtualControl
{
    public class VirtualMoveField : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public float speed = 1f;
        public bool IsEnter { get; private set; }
        public Vector2 Direction { get; private set; }
        private Vector2 centerPoint;
        public void OnPointerDown(PointerEventData data)
        {
            IsEnter = true;
            centerPoint = data.position;
        }

        public void OnPointerUp(PointerEventData data)
        {
            IsEnter = false;
            Direction = Vector2.zero;
        }

        public void OnDrag(PointerEventData data)
        {
            if (!IsEnter) return;
            Vector2 pointerPos = data.position;
            Direction = (pointerPos - centerPoint) * 0.01f * speed;
            centerPoint = pointerPos;
        }
    }
}

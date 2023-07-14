using UnityEngine;
namespace VoiceChatMultiplayer
{
    public class ObjectLookCamera : MonoBehaviour
    {
        private void FixedUpdate()
        {
            transform.LookAt(Camera.main.transform);
        }
    }
}
using UnityEngine;
namespace VoiceChatMultiplayer.VirtualControl
{
    public class VirtualControl : MonoBehaviour
    {
        public static VirtualControl instance { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        public VirtualJoystick joystick = default;
        public VirtualMoveField moveView = default;
        public VirtualButton buttonShoot = default;
        public VirtualButton buttonJumb = default;
    }
}
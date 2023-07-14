using UnityEngine;
namespace VoiceChatMultiplayer
{
    public class PlayerControl : MonoBehaviour
    {
        [System.Serializable]
        public class OnShootEvent : UnityEngine.Events.UnityEvent { }
        public OnShootEvent onShoot = new OnShootEvent();
        //[System.Serializable]
        //public class OnVoiceEvent : UnityEngine.Events.UnityEvent<bool> { }
        //public OnVoiceEvent onVoiceEnable = new OnVoiceEvent();
        public bool touchControl = true;
        //[SerializeField] bool cursorLocked = default;
        [SerializeField] float sensitivity = 5f;
        [SerializeField] float speed = 10f;
        [SerializeField] float shootDelay = 1f;
        Rigidbody rb;
        Camera cam;
        bool isGround = false;
        float shootTime = default;
        //bool voiceActive = false;
        VirtualControl.VirtualControl virtualControl;
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            cam = Camera.main;
            //Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            virtualControl = VirtualControl.VirtualControl.instance;
        }

        private void Update()
        {
            float moveHorizontal = touchControl ? virtualControl.joystick.Direction.x : Input.GetAxis("Horizontal");
            float moveVertical = touchControl ? virtualControl.joystick.Direction.y : Input.GetAxis("Vertical");
            float rotateHorizontal = touchControl ? virtualControl.moveView.Direction.x : Input.GetAxis("Mouse X");
            float rotateVertical = touchControl ? virtualControl.moveView.Direction.y : Input.GetAxis("Mouse Y");
            bool onJumb = touchControl ? virtualControl.buttonJumb.IsPress : Input.GetAxis("Jump") == 1;
            bool isShoot = touchControl ? virtualControl.buttonShoot.IsPress : Input.GetAxis("Fire1") == 1;
            //bool isVoice = Input.GetKey(KeyCode.N);

            if (isGround)
            {
                //transform.RotateAround(transform.position, Vector3.up, moveHorizontal);
                //rb.velocity += Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up) * Vector3.forward * moveVertical * Time.deltaTime * speed;
                //Other move
                Vector3 dir = cam.transform.rotation * new Vector3(moveHorizontal, 0, moveVertical);
                dir.y = 0;
                rb.velocity += dir * Time.deltaTime * speed;
                if (dir.magnitude > 0)
                    transform.rotation = Quaternion.LookRotation(dir);
                if (onJumb)
                {
                    rb.AddForce(Vector3.up * rb.mass * 50);
                }
            }

            cam.transform.RotateAround(transform.position, Vector3.up, rotateHorizontal * sensitivity);
            cam.transform.RotateAround(transform.position, cam.transform.right, -rotateVertical * sensitivity);
            cam.transform.parent.position = transform.position;

            if (isShoot)
            {
                if (Time.time - shootTime > shootDelay)
                {
                    shootTime = Time.time;
                    onShoot.Invoke();
                }
            }

            //Active voice chat via press N key
            //if (isVoice != voiceActive)
            //{
            //    voiceActive = isVoice;
            //    onVoiceEnable.Invoke(isVoice);
            //}
        }

        private void OnCollisionStay(Collision collision)
        {
            isGround = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            isGround = false;
        }
    }
}
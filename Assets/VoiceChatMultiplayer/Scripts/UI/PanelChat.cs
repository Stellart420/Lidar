using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace VoiceChatMultiplayer.UI
{
    public class PanelChat : MonoBehaviour
    {
        public static PanelChat instance { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        [SerializeField] TextMeshProUGUI clientNameText = default;
        [SerializeField] TextMeshProUGUI chatContentText = default;
        [SerializeField] GameObject[] chatObjects = default;
        [SerializeField] CanvasGroup[] chatCanvas = default;

        public void SetClientName(string text)
        {
            clientNameText.text = text;
        }

        public void OnToggleChatChange(bool active)
        {
            foreach (var o in chatObjects)
            {
                o.SetActive(active);
            }
            foreach (var c in chatCanvas)
            {
                c.alpha = active ? 1 : 0;
            }
        }

        private void Start()
        {
            OnToggleChatChange(false);
        }

        public void AddChat(string playerNameText, string text)
        {
            chatContentText.text += string.Format("<br><b>{0}:</b> {1}", playerNameText, text);
        }

        public void SetupPlayerChat(PlayerChatNetwork clientPlayer)
        {
            clientPlayerChat = clientPlayer;
            SetupChat();
            SetupVoice();
        }

        PlayerChatNetwork clientPlayerChat = default;
        [SerializeField] TMP_InputField inputChat = default;
        [SerializeField] Button btnSendChat = default;
        void SetupChat()
        {
            btnSendChat.onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(inputChat.text))
                {
                    clientPlayerChat.SendChat(inputChat.text);
                    inputChat.text = string.Empty;
                }
            });
        }

        [SerializeField] Toggle toggleVoice = default;
        void SetupVoice()
        {
            toggleVoice.onValueChanged.AddListener((value) =>
            {
                toggleVoice.isOn = clientPlayerChat.StartCall(value);
            });
        }
    }
}
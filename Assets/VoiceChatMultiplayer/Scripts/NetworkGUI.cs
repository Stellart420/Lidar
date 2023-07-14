using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace VoiceChatMultiplayer
{
    public class NetworkGUI : MonoBehaviour
    {
        private void OnGUI()
        {
            if (!onGUI) return;
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));

            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (!networkManager.IsClient && !networkManager.IsServer)
            {

            }
            else
            {
                GUILayout.Label($"Mode: {(networkManager.IsHost ? "Host" : networkManager.IsServer ? "Server" : "Client")}");
            }

            GUILayout.EndArea();
        }

        [SerializeField] bool onGUI = true;
        [SerializeField] GameObject panelMenu = default;
        [SerializeField] GameObject panelControl = default;
        [SerializeField] Button btnServer = default;
        [SerializeField] TMP_InputField inputPlayerName = default;
        [SerializeField] GameObject textError = default;
        [SerializeField] Button btnHost = default;
        [SerializeField] Button btnClient = default;
        public string playerName { get; private set; }

        private void Start()
        {
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            btnServer.onClick.AddListener(() =>
            {
                networkManager.StartServer();
                panelControl.SetActive(false);
                StartGame();
            });
            btnHost.onClick.AddListener(() =>
            {
                if (!string.IsNullOrWhiteSpace(inputPlayerName.text))
                {
                    playerName = inputPlayerName.text;
                    networkManager.StartHost();
                    StartGame();
                }
                else
                {
                    textError.SetActive(true);
                }
            });
            btnClient.onClick.AddListener(() =>
            {
                if (!string.IsNullOrWhiteSpace(inputPlayerName.text))
                {
                    playerName = inputPlayerName.text;
                    networkManager.StartClient();
                    StartGame();
                }
                else
                {
                    textError.SetActive(true);
                }
            });
            panelMenu.SetActive(true);
        }

        void MainMenu()
        {
            panelMenu.SetActive(true);
        }

        void StartGame()
        {
            panelMenu.SetActive(false);
        }
    }
}
using UnityEngine;
using Unity.Netcode;
using TMPro;

namespace VoiceChatMultiplayer
{
    public class PlayerNetwork : NetworkBehaviour
    {
        [SerializeField] TextMeshPro playerNameTextMesh = default;
        [SerializeField] MeshRenderer[] playerMR = default;
        [SerializeField] Transform barrelTransform = default;
        [SerializeField] Rigidbody bulletPrefab = default;
        //Player Data
        public string PlayerName { get; private set; }
        public string ColorHex { get; private set; }
        public string PlayerNameTextString { get => string.Format("<color={0}>{1}.</color>{2}", ColorHex, OwnerClientId, PlayerName); }

        //Set Player data on server
        [ServerRpc]
        public void SetPlayerDataServerRpc(string _PlayerName, string _ColorHex)
        {
            PlayerName = _PlayerName;
            ColorHex = _ColorHex;

            UpdateAllPlayersData();
        }

        void UpdateAllPlayersData()
        {
            var players = FindObjectsOfType<PlayerNetwork>();
            foreach(var p in players)
            {
                p.UpdatePlayerDataClientRpc(p.PlayerName, p.ColorHex);
            }
        }

        //Recive Player data on client
        [ClientRpc]
        public void UpdatePlayerDataClientRpc(string _PlayerName, string _ColorHex)
        {
            PlayerName = _PlayerName;
            ColorHex = _ColorHex;

            Debug.Log(OwnerClientId + " Set data");
            var mat = playerMR[0].material;
            Color newCol;
            if (ColorUtility.TryParseHtmlString(ColorHex, out newCol))
            {
                mat.color = newCol;
                foreach (MeshRenderer mr in playerMR)
                {
                    mr.sharedMaterial = mat;
                }
            }

            playerNameTextMesh.text = PlayerNameTextString;

            //update player name of panel chat
            if (IsOwner)
            {
                var playerChatNetwork = gameObject.GetComponent<PlayerChatNetwork>();
                if (playerChatNetwork != null)
                {
                    VoiceChatMultiplayer.UI.PanelChat.instance.SetClientName(PlayerNameTextString);
                }
            }
        }

        [ServerRpc]
        void ShootBulletServerRpc()
        {
            Rigidbody bullet = Instantiate(bulletPrefab, barrelTransform.position, barrelTransform.rotation);
            bullet.isKinematic = false;
            bullet.AddForce(barrelTransform.rotation * Vector3.forward * 1000);
            Destroy(bullet.gameObject, 10);
            bullet.GetComponent<NetworkObject>().Spawn();
        }

        private void Start()
        {
            var playerControl = GetComponent<PlayerControl>();
            if (playerControl != null)
            {
                playerControl.enabled = IsOwner;
            }

            if (IsOwner)
            {
                playerControl.onShoot.AddListener(() =>
                {
                    ShootBulletServerRpc();
                });

                // Set player data
                PlayerName = FindObjectOfType<NetworkGUI>().playerName;
                ColorHex = "#" + ColorUtility.ToHtmlStringRGB(Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
                SetPlayerDataServerRpc(PlayerName, ColorHex);
            }
        }

        //When the server is missing player automatic be destroy
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (!IsOwner) return;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
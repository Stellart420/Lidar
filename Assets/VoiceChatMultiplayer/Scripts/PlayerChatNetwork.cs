using Unity.Netcode;
using Unity.WebRTC;
using UnityEngine;
using VoiceChatMultiplayer.UI;

namespace VoiceChatMultiplayer
{
    public class PlayerChatNetwork : NetworkBehaviour
    {
        [SerializeField] VoiceWebRTC voiceWebRTC = default;
        public string playerId { get => OwnerClientId.ToString(); }

        private void Start()
        {
            if (IsOwner)
            {
                if (PanelChat.instance != null)
                    PanelChat.instance.SetupPlayerChat(this);

                if(ScanController.Instance != null)
                    ScanController.Instance.SetupPlayerChat(this);
            }
            if (voiceWebRTC == null) voiceWebRTC.GetComponentInChildren<VoiceWebRTC>(); 
        }

        string FindClientPlayerId()
        {
            var playersChat = FindObjectsOfType<PlayerChatNetwork>();
            foreach (var p in playersChat)
            {
                if (p.IsOwner)
                {
                    return p.playerId;
                }
            }
            return string.Empty;
        }

        public void SendChat(string text)
        {
            SendChatServerRpc("user", text);
        }

        public void Mute(bool active)
        {
            voiceWebRTC.gameObject.SetActive(!active);
        }

        [ServerRpc]
        void SendChatServerRpc(string playerName, string text)
        {
            SendChatClientRpc(playerName, text);
        }
        [ClientRpc]
        void SendChatClientRpc(string playerName, string text)
        {
            PanelChat.instance.AddChat(playerName, text);
        }

        public bool StartCall(bool active)
        {
            return voiceWebRTC.EnableMic(active);
        }

        public void CallStopCall()
        {
            SetStopCallServerRpc(FindClientPlayerId());
        }
        [ServerRpc(RequireOwnership = false)]
        void SetStopCallServerRpc(string sendPlayerId)
        {
            SetStopCallClientRpc(sendPlayerId);
        }
        [ClientRpc]
        void SetStopCallClientRpc(string sendPlayerId)
        {
            if (sendPlayerId == FindClientPlayerId()) return;
                voiceWebRTC.StopOutputSound();
        }


        //callIndex = pc_local index, answerIndex = pc_remote index
        public void CallOfferAnswerd(string callName, RTCSessionDescription desc)
        {
            //is mine call
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(desc);
            SetOfferAnswerdServerRpc(callName, json, FindClientPlayerId());
        }
        [ServerRpc(RequireOwnership = false)]
        void SetOfferAnswerdServerRpc(string callName, string json, string sendPlayerId)
        {
            SetOfferAnswerdClientRpc(callName, json, sendPlayerId);
        }
        [ClientRpc]
        void SetOfferAnswerdClientRpc(string callName, string json, string sendPlayerId)
        {
            if (sendPlayerId == FindClientPlayerId()) return;
            //Debug.Log("create remote..." + callName + "vs" + FindClientPlayerId());
            if (callName != FindClientPlayerId()) return;
            RTCSessionDescription desc = Newtonsoft.Json.JsonConvert.DeserializeObject<RTCSessionDescription>(json);
            voiceWebRTC.OfferAnswer(desc);
        }
        public void CallOfferCandiates(RTCIceCandidateInit info)
        {
            //is mine call
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(info);
            SetOfferCandiatesServerRpc(playerId, json, FindClientPlayerId());
        }
        [ServerRpc(RequireOwnership = false)]
        void SetOfferCandiatesServerRpc(string callName, string json, string sendPlayerId)
        {
            SetOfferCandiatesClientRpc(callName, json, sendPlayerId);
        }
        [ClientRpc]
        void SetOfferCandiatesClientRpc(string callName, string json, string sendPlayerId)
        {
            if (sendPlayerId == FindClientPlayerId()) return;
            //Debug.Log("set candiate..." + callName + "vs" + FindClientPlayerId());
            if (callName != playerId) return;
            RTCIceCandidateInit info = Newtonsoft.Json.JsonConvert.DeserializeObject<RTCIceCandidateInit>(json);
            voiceWebRTC.OfferCandiates(info);//pc_remote null ???
        }

        //client answerd
        public void CallRemoteAnswerd(RTCSessionDescription desc)
        {
            //other player call
            //client player set description to character call
            var answerName = FindClientPlayerId();// GameManager.instance.clientPlayerNetwork.GetPlayerName();
            if (answerName != string.Empty)
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(desc);
                    SetRemoteAnswerdServerRpc(answerName, json, FindClientPlayerId());
            }
        }
        [ServerRpc(RequireOwnership = false)]
        void SetRemoteAnswerdServerRpc(string answerName, string json, string sendPlayerId)
        {
            SetRemoteAnswerdClientRpc(answerName, json, sendPlayerId);
        }
        [ClientRpc]
        void SetRemoteAnswerdClientRpc(string answerName, string json, string sendPlayerId)
        {
            if (sendPlayerId == FindClientPlayerId()) return;
            //call index is mine
            if (!IsOwner) return;
            //answer the call index (answerIndex == callIndex)
            RTCSessionDescription desc = Newtonsoft.Json.JsonConvert.DeserializeObject<RTCSessionDescription>(json);
            voiceWebRTC.RemoteAnswered(answerName, desc);
        }

        public void CallAnswerCandiates(RTCIceCandidateInit info)
        {
            //other player call
            var answerName = FindClientPlayerId();// GameManager.instance.clientPlayerNetwork.GetPlayerName();
            if (answerName != string.Empty)
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(info);
                if (IsOwner)
                    SetAnswerCandiatesServerRpc(answerName, json, FindClientPlayerId());
            }
        }
        [ServerRpc(RequireOwnership = false)]
        void SetAnswerCandiatesServerRpc(string answerName, string json, string sendPlayerId)
        {
            SetAnswerCandiatesClientRpc(answerName, json, sendPlayerId);
        }
        [ClientRpc]
        void SetAnswerCandiatesClientRpc(string answerName, string json, string sendPlayerId)
        {
            if (sendPlayerId == FindClientPlayerId()) return;
            //callindex is mine
            if (!IsOwner) return;
            //set answer candiate the call index (answerIndex == callIndex)
            RTCIceCandidateInit info = Newtonsoft.Json.JsonConvert.DeserializeObject<RTCIceCandidateInit>(json);
            voiceWebRTC.AnswerCandiates(answerName, info);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using System.Linq;

namespace VoiceChatMultiplayer
{
    public class VoiceWebRTC : MonoBehaviour
    {
        [SerializeField] PlayerChatNetwork player = default;
        //count players add muti pear local
        RTCPeerConnection[] pc_local;
        RTCPeerConnection pc_remote;
        private MediaStream localStream;
        private MediaStream remoteStream;

        private AudioStreamTrack m_audioTrack;
        private string m_deviceName = null;
        private AudioClip m_clipInput;
        int m_samplingFrequency = 48000;
        int m_lengthSeconds = 1;
        public bool startMic { get; private set; }
        [SerializeField] private AudioSource inputAudioSource = default;
        [SerializeField] private AudioSource outputAudioSource = default;
        [System.Serializable]
        public class OnSpeakEvent : UnityEngine.Events.UnityEvent<bool> { }
        public OnSpeakEvent onSpeakEvent = new OnSpeakEvent();
        bool isSpeak = false;

        float[] spectrum = new float[128];
        [SerializeField] bool isOutputSound = default;
#if UNITY_STANDALONE
        [Header("TEST")]
        [SerializeField] bool enableTestClip = false;
        [SerializeField] AudioClip testClip = default;
#endif

        private static RTCConfiguration GetSelectedSdpSemantics()
        {
            RTCConfiguration config = default;

            //SET SERVER CONFIG
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            //config.iceServers = new[] { 
            //    new RTCIceServer { 
            //        urls = new[] { "stun:stun.l.google.com:19302", "turn:url" },
            //        username = "username",
            //        credential = "password"
            //    } 
            //};
            return config;
        }

        private void Start()
        {
            onSpeakEvent.Invoke(false);
            if (player == null) player = GetComponentInParent<PlayerChatNetwork>();
        }

        private void FixedUpdate()
        {
            if (isOutputSound)
            {
                outputAudioSource.GetSpectrumData(spectrum, 0, FFTWindow.Blackman);
                bool speak = spectrum[0] * 100 > 0.1f;
                if (speak != isSpeak)
                {
                    isSpeak = speak;
                    onSpeakEvent.Invoke(speak);
                }
            }
        }

        void OnAddTrack(MediaStreamTrackEvent e)
        {
            var track = e.Track as AudioStreamTrack;
            outputAudioSource.SetTrack(track);
            outputAudioSource.loop = true;
            outputAudioSource.Play();
        }

        public void OnEnableMic(bool active)
        {
            EnableMic(active);
        }

        public bool EnableMic(bool active)
        {
            if (active)
            {
                //if (!startMic)
                //{
                if (MicButton())
                {
                    CallButton();
                    return true;
                }
                //}
                //else
                //{
                //    OnResume();
                //}
            }
            else
            {
                //if (startMic)
                //{
                MicStopButton();
                //}
                //OnPause();
            }
            return false;
        }

        public bool MicButton()
        {
#if UNITY_STANDALONE
            if (!enableTestClip)
            {
#endif
                foreach (var device in Microphone.devices)
                {
                    m_deviceName = device;
                    break;
                }
                m_clipInput = Microphone.Start(m_deviceName, true, m_lengthSeconds, m_samplingFrequency);
                // set the latency to �0� samples before the audio starts to play.
                while (!(Microphone.GetPosition(m_deviceName) > 0)) { }
                startMic = true;
                //if (Microphone.GetPosition(m_deviceName) > 0)
                //    {
                //        startMic = true;
                //        Debug.Log("start " + m_deviceName);
                //    }
#if UNITY_STANDALONE
            }
            else
            {
                m_clipInput = testClip;
                startMic = true;
            }
#endif

            inputAudioSource.gameObject.SetActive(true);
            inputAudioSource.loop = true;
            inputAudioSource.clip = m_clipInput;
            inputAudioSource.Play();

            return startMic;
        }

        void OnPause()
        {
            foreach (var pc in pc_local)
            {
                var transceiver1 = pc.GetTransceivers().First();
                var track = transceiver1.Sender.Track;
                track.Enabled = false;
            }
        }

        void OnResume()
        {
            foreach (var pc in pc_local)
            {
                var transceiver1 = pc.GetTransceivers().First();
                var track = transceiver1.Sender.Track;
                track.Enabled = true;
            }
        }

        public void MicStopButton()
        {
            startMic = false;
            Microphone.End(m_deviceName);
            m_clipInput = null;

            m_audioTrack?.Dispose();
            localStream?.Dispose();
            for (int i = 0; i > pc_local.Length; i++)
            {
                pc_local[i]?.Dispose();
                pc_local[i] = null;
            }

            inputAudioSource.Stop();
            inputAudioSource.gameObject.SetActive(false);
            //outputAudioSource.Stop();

            player.CallStopCall();
        }

        public void StopOutputSound()
        {
            if (pc_remote == null) return;
            remoteStream?.Dispose();
            pc_remote.Dispose();
            pc_remote = null;
            outputAudioSource.Stop();
            outputAudioSource.gameObject.SetActive(false);
            isOutputSound = false;
            onSpeakEvent.Invoke(false);
            notSetOfferCandiate.Clear();
        }

        PlayerChatNetwork[] players = default;
        public void CallButton()
        {
            players = FindObjectsOfType<PlayerChatNetwork>();
#if UNITY_EDITOR
            Debug.Log("Call all:" + players.Length);
#endif
            //when player left, sort is reduce but character index not change
            pc_local = new RTCPeerConnection[players.Length];
            var configuration = GetSelectedSdpSemantics();

            for (int i = 0; i < pc_local.Length; i++)
            {
                if (!players[i].IsOwner)// != clientPlayer)
                {
                    if (pc_local[i] == null)
                    {

                        pc_local[i] = new RTCPeerConnection(ref configuration);
                    }

                    var pc = pc_local[i];
                    pc.OnIceCandidate = candidate =>
                    {
                        //pcCC = candidate.Candidate;
                        //push offer candiate
                        Debug.Log(candidate.Candidate);
                        player.CallOfferCandiates(new RTCIceCandidateInit()
                        {
                            candidate = candidate.Candidate,
                            sdpMid = candidate.SdpMid,
                            sdpMLineIndex = candidate.SdpMLineIndex
                        });
                        //StartCoroutine(Call());
                    };
                    pc.OnNegotiationNeeded = () => StartCoroutine(Call(pc));
                }
            }

            // Push tracks from local stream to peer connection
            localStream = new MediaStream();
            m_audioTrack = new AudioStreamTrack(inputAudioSource);
            for (int i = 0; i < pc_local.Length; i++)
            {
                if (!players[i].IsOwner)// players[i] != clientPlayer)
                {
                    pc_local[i].AddTrack(m_audioTrack, localStream);
                }
            }
        }
        IEnumerator Call(RTCPeerConnection pc)
        {

            var offerDes = pc.CreateOffer();
            yield return offerDes;
            RTCSessionDescription desc = offerDes.Desc;
            var localDes = pc.SetLocalDescription(ref desc);
            yield return localDes;


            player.CallOfferAnswerd(players[GetPcLocalIndex(pc)].playerId, desc);
            //push offer to server calldoc

            // Listen for remote answer

            // When answered, add candidate to peer connection
        }

        int GetPcLocalIndex(RTCPeerConnection pc)
        {
            var index = 0;
            for (int i = 0; i < pc_local.Length; i++)
                if (pc_local[i] == pc)
                {
                    index = i;
                    break;
                }
            return index;
        }

        public void RemoteAnswered(string answerName, RTCSessionDescription desc)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].playerId == answerName)
                {
                    pc_local[i].SetRemoteDescription(ref desc);
                    break;
                }
            }

        }

        public void AnswerCandiates(string answerName, RTCIceCandidateInit info)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].playerId == answerName)
                {
                    var candidate = new RTCIceCandidate(info);
                    pc_local[i].AddIceCandidate(candidate);
                    Debug.Log(candidate.Candidate);
                    break;
                }
            }

        }

        public void OfferAnswer(RTCSessionDescription callDesc)
        {
            if (pc_remote == null)
            {
                var configuration = GetSelectedSdpSemantics();
                pc_remote = new RTCPeerConnection(ref configuration);
            }
            pc_remote.OnIceCandidate = candidate =>
            {
                //pcCC = candidate.Candidate;
                //push answer candiate
                player.CallAnswerCandiates(new RTCIceCandidateInit()
                {
                    candidate = candidate.Candidate,
                    sdpMid = candidate.SdpMid,
                    sdpMLineIndex = candidate.SdpMLineIndex
                });
            };
            StartCoroutine(Answer(callDesc));

            // Pull tracks from remote stream;
            outputAudioSource.gameObject.SetActive(true);
            remoteStream = new MediaStream();
            remoteStream.OnAddTrack += OnAddTrack;
            pc_remote.OnTrack = e => remoteStream.AddTrack(e.Track);
            var transceiver2 = pc_remote.AddTransceiver(TrackKind.Audio);
            transceiver2.Direction = RTCRtpTransceiverDirection.RecvOnly;

            isOutputSound = true;
        }
        IEnumerator Answer(RTCSessionDescription callDesc)
        {

            yield return pc_remote.SetRemoteDescription(ref callDesc);
            var answerDescription = pc_remote.CreateAnswer();
            yield return answerDescription;
            RTCSessionDescription desc = answerDescription.Desc;
            pc_remote.SetLocalDescription(ref desc);
 
            player.CallRemoteAnswerd(desc);
            //listen

            if (notSetOfferCandiate.Count > 0)
            {
                foreach(var c in notSetOfferCandiate)
                {
                    OfferCandiates(c);
                }
                notSetOfferCandiate.Clear();
            }
        }

        List<RTCIceCandidateInit> notSetOfferCandiate = new List<RTCIceCandidateInit>();
        public void OfferCandiates(RTCIceCandidateInit info)
        {
            //RTCIceCandidateInit info;
            if (pc_remote == null)
            {
                notSetOfferCandiate.Add(info);
                return;
            }
            var candidate = new RTCIceCandidate(info);
            pc_remote.AddIceCandidate(candidate);
            Debug.Log(candidate.Candidate);
        }
    }
}
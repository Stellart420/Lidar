using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSynchronizer : MonoBehaviour
{
    [SerializeField] private NetworkAudioPlayer _networkAudioPlayer;
    [SerializeField] private MicrophoneDataGetter _networkAudioReader;

    [ContextMenu("start test")]
    public void StartTest()
    {
        _networkAudioReader.OnSamplePartRecorded += (d, s) =>
        {
            _networkAudioPlayer.PlaySamplePart(s, d);
        };

        _networkAudioReader.StartRecord();
    }

}

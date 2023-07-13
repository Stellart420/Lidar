using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneDataGetter : MonoBehaviour
{
    public Action<byte[], int> OnSamplePartRecorded;


    [SerializeField] private bool _recordEnabled = false;
    [SerializeField] private int FREQUENCY = 44100;
    [SerializeField] private float _sampleLenghtSecond = 0.1f;
    private AudioClip _microphoneClip;
    private int _lastSamplePosition = 0;
    private float _timer;
    private float _clipTimer = 0;



    public void StartRecord()
    {
        _microphoneClip = Microphone.Start(null, true, 100, FREQUENCY);
        while (Microphone.GetPosition(null) < 0) { }

        _recordEnabled = true;
    }

    public void StopRecord()
    {
        _microphoneClip = null;
        _recordEnabled = false;

        _lastSamplePosition = 0;
        _clipTimer = 0;
        _timer = 0;
    }




    private void Update()
    {
        _timer += Time.deltaTime;
        _clipTimer += Time.deltaTime;
        if (_recordEnabled)
        {
            if(_clipTimer > 90)
            {
                StopRecord();
                StartRecord();
                return;
            }

            if (_timer > _sampleLenghtSecond)
            {

                _timer = 0;
                int pos = Microphone.GetPosition(null);
                int diff = pos - _lastSamplePosition;
                if (diff > 0)
                {
                    float[] samples = new float[diff * _microphoneClip.channels];
                    _microphoneClip.GetData(samples, _lastSamplePosition);
                    byte[] ba = ToByteArray(samples);
                    OnSamplePartRecorded?.Invoke(ba, _microphoneClip.channels);
                }

                _lastSamplePosition = pos;
            }
        }
    }





    private byte[] ToByteArray(float[] floatArray)
    {
        int len = floatArray.Length * 4;
        byte[] byteArray = new byte[len];
        int pos = 0;
        foreach (float f in floatArray)
        {
            byte[] data = System.BitConverter.GetBytes(f);
            System.Array.Copy(data, 0, byteArray, pos, 4);
            pos += 4;
        }
        return byteArray;
    }

   
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource _player;

    public void PlaySamplePart(int channels, byte[] audioData)
    {
        var array = ToFloatArray(audioData, out int samplesCount);

        var clip = AudioClip.Create("temp", samplesCount, channels, 44100, false);
        clip.SetData(array, 0);

        _player.clip = clip;
        _player.Play();
    }


    private float[] ToFloatArray(byte[] byteArray, out int samplesCount)
    {
        int len = byteArray.Length / 4;
        float[] floatArray = new float[len];
        for (int i = 0; i < byteArray.Length; i += 4)
        {
            floatArray[i / 4] = System.BitConverter.ToSingle(byteArray, i);
        }

        samplesCount = len;
        return floatArray;
    }
}

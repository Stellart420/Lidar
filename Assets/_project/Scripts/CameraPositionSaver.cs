using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class CameraPositionSaver : Singleton<CameraPositionSaver>
{
    private List<(Vector3, Quaternion)> _cameraPositions = new List<(Vector3, Quaternion)>();
    private Coroutine _savingProcesss;
    private Coroutine _getCameraTextureProcess;

    public List<(Vector3, Quaternion)> CameraPositions => _cameraPositions;


    public void StartSaving()
    {
        Debug.Log("StartSaving");
        _savingProcesss = StartCoroutine(SavePositionProcess());
        //_getCameraTextureProcess = StartCoroutine(SaveTextureProcess());
    }

    public void StopSaving()
    {
        if (_savingProcesss != null)
            StopCoroutine(_savingProcesss);

        if (_getCameraTextureProcess != null)
            StopCoroutine(_getCameraTextureProcess);
        Debug.Log("StopSaving");
    }


    private IEnumerator SavePositionProcess()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);

            _cameraPositions.Add((transform.localPosition, transform.localRotation));
            TextureGetter.Instance.GetImageAsync();
        }
    }


    private IEnumerator SaveTextureProcess()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);
            TextureGetter.Instance.GetImageAsync();

        }
    }
}

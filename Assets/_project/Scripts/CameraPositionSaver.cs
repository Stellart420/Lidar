using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class CameraPositionSaver : Singleton<CameraPositionSaver>
{
    private Coroutine _savingProcesss;
    private Coroutine _getCameraTextureProcess;

    public Dictionary<int, ScanData> SavedCameraData = new Dictionary<int, ScanData>();
    private int _currentId = 0;

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
            yield return new WaitForSeconds(1f);

            SavedCameraData.Add(_currentId, new ScanData() { Position = transform.localPosition, Rotation = transform.localRotation });
            TextureGetter.Instance.GetImageAsync(SavedCameraData, _currentId);
        }
    }

}

public class ScanData
{
    public int Id;
    public Vector3 Position;
    public Quaternion Rotation;
    public Texture2D Texture;
}
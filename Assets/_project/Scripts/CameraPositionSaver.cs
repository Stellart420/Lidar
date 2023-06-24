using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class CameraPositionSaver : MonoBehaviour
{
    public static CameraPositionSaver Instance;

    [SerializeField] private RawImage _image;
    [SerializeField] private ARCameraBackground _cameraBackground;

    private List<(Vector3, Quaternion)> _cameraPositions = new List<(Vector3, Quaternion)>();
    private Coroutine _savingProcesss;

    public List<(Vector3, Quaternion)> CameraPositions => _cameraPositions;

    private void Awake()
    {
        Instance = this;
    }

    public void StartSaving()
    {
        Debug.Log("StartSaving");
        _savingProcesss = StartCoroutine(SavePsitionProcess());
    }

    public void StopSaving()
    {
        if (_savingProcesss != null)
            StopCoroutine(_savingProcesss);

        Debug.Log("StopSaving");
    }


    private IEnumerator SavePsitionProcess()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);

            _image.material.mainTexture = _cameraBackground.material.mainTexture;

            _cameraPositions.Add((transform.localPosition, transform.localRotation));
        }
    }
}

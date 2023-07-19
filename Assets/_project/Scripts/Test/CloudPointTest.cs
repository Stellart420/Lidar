using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CloudPointTest : MonoBehaviour
{
    [SerializeField] private ARPointCloudManager _pointsCloud;


    private void Start()
    {
        _pointsCloud.pointCloudsChanged += _pointsCloud_pointCloudsChanged;
    }

    private void _pointsCloud_pointCloudsChanged(ARPointCloudChangedEventArgs obj)
    {
        Debug.Log($"CHANGES. ADD: {obj.added.Count} UPDATE: {obj.updated.Count} REMOVE: {obj.removed.Count}");
    }
}

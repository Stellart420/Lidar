using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CloudPointTest : MonoBehaviour
{
    [SerializeField] private ARPointCloudManager _pointsCloud;
    [SerializeField] private Transform _pointPrefab;

    private Dictionary<TrackableId, CloudPointBehaviour> _clouds = new Dictionary<TrackableId, CloudPointBehaviour>();

    private void Start()
    {
        _pointsCloud.pointCloudsChanged += _pointsCloud_pointCloudsChanged;
    }

    private void _pointsCloud_pointCloudsChanged(ARPointCloudChangedEventArgs obj)
    {
        foreach(var added in obj.added)
        {
            var behaviour = added.GetComponent<CloudPointBehaviour>();
            if (behaviour != null)
            {
                _clouds.Add(added.trackableId, behaviour);
                behaviour.Initialize(_pointPrefab);
                behaviour.UpdatePoints(added.positions);
            }
            else
                Debug.LogError("Can not find behaviour component");
        }

        foreach (var updated in obj.updated)
        {

            if(_clouds.TryGetValue(updated.trackableId, out CloudPointBehaviour behaviour))
            {
                behaviour.UpdatePoints(updated.positions);
            }
            else
                Debug.LogError("Can not find behaviour component");
        }

        if (obj.removed.Count > 0)
            Debug.Log($"REMOVED: {obj.removed.Count}");
    }
}

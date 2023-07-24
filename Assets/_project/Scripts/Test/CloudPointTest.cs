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

    public void StartScan()
    {
        _pointsCloud.pointCloudsChanged += _pointsCloud_pointCloudsChanged;
    }

    public void StoptScan()
    {
        _pointsCloud.pointCloudsChanged -= _pointsCloud_pointCloudsChanged;

        foreach(var cloud in _clouds)
        {
            cloud.Value.StartCreatePoints();
        }
    }

    private void _pointsCloud_pointCloudsChanged(ARPointCloudChangedEventArgs obj)
    {
        Debug.Log($"add: {obj.added.Count}   update: {obj.updated.Count}");

        foreach(var added in obj.added)
        {
            var behaviour = added.GetComponent<CloudPointBehaviour>();
            if (behaviour != null)
            {
                Debug.Log($"add: {added.trackableId.ToString()}");
                _clouds.Add(added.trackableId, behaviour);
                behaviour.Initialize(_pointPrefab);
                behaviour.UpdatePoints(added.positions);
            }
            else
                Debug.LogError("Can not find behaviour component");
        }

        foreach (var updated in obj.updated)
        {
            Debug.Log($"updated: {updated.trackableId.ToString()}");

            if (_clouds.TryGetValue(updated.trackableId, out CloudPointBehaviour behaviour))
            {
                behaviour.UpdatePoints(updated.positions);
            }
            else
            {
                var behaviour1 = updated.GetComponent<CloudPointBehaviour>();
                if (behaviour1 != null)
                {
                    Debug.Log($"add updated: {updated.trackableId.ToString()}");
                    _clouds.Add(updated.trackableId, behaviour1);
                    behaviour1.Initialize(_pointPrefab);
                    behaviour1.UpdatePoints(updated.positions);
                }
                else
                    Debug.LogError("Can not find behaviour component in update");
            }
        }

        if (obj.removed.Count > 0)
            Debug.Log($"REMOVED: {obj.removed.Count}");
    }
}

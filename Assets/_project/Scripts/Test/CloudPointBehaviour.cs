using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CloudPointBehaviour : MonoBehaviour
{
    private Transform _transform;
    private Transform _pointPrefab;

    private Dictionary<Vector3, GameObject> _pointsView = new Dictionary<Vector3, GameObject>();


    public void Initialize(Transform pointPrefab)
    {
        _transform = transform;
        _pointPrefab = pointPrefab;
    }

    public void UpdatePoints(NativeSlice<Vector3>? positions)
    {
        if (positions == null || !positions.HasValue)
            return;

        int created = 0;
        foreach(var position in positions)
        {
            if (_pointsView.ContainsKey(position))
                continue;

            if (created > 10)
                break;

            var newPoint = Instantiate(_pointPrefab, _transform);
            newPoint.localPosition = position;
            newPoint.localScale = Vector3.one * 0.01f;

            _pointsView.Add(position, newPoint.gameObject);

            ++created;
        }

        Debug.Log($"PointsCount: {created}");

    }
}

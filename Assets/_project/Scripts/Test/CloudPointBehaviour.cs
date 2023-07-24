using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CloudPointBehaviour : MonoBehaviour
{
    private Transform _transform;
    private Transform _pointPrefab;

    private List<Vector3> _positions = new List<Vector3>();
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

        _positions = new List<Vector3>();

        foreach (var position in positions)
        {
            if (_pointsView.ContainsKey(position))
                continue;

            _positions.Add(position);
        }



    }

    public void StartCreatePoints()
    {
        Debug.Log($"PointsCount: {_positions.Count}");

        StartCoroutine(CreatePositionsProcess());

    }

    private IEnumerator CreatePositionsProcess()
    {
        int allCreated = 0;

        while (allCreated < _positions.Count)
        {
            int offset = (allCreated + 10 >= _positions.Count) ? _positions.Count - allCreated : 10;
            for (int i = allCreated; i < allCreated + offset; i++)
            {
                var newPoint = Instantiate(_pointPrefab, _transform);
                newPoint.localPosition = _positions[i];
                newPoint.localScale = Vector3.one * 0.01f;

                _pointsView.Add(_positions[i], newPoint.gameObject);
            }

            allCreated += 10;

            yield return new WaitForSeconds(0.1f);
        }
    }
}

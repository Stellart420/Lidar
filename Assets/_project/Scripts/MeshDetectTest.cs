using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDetectTest : MonoBehaviour
{
    [SerializeField] private bool _canCheck = false;
    [SerializeField] private MeshFilter m_Filter;
    [SerializeField] private MeshRenderer m_renderer;

    [SerializeField] private Camera _checkMeshCamera;

    void Update()
    {
        if(_canCheck)
        {
            if(IsMeshInCamera(m_Filter))
            {
                m_renderer.material.color = Color.green;
            }
            else
            {
                m_renderer.material.color = Color.red;
            }
        }
    }


    private bool IsMeshInCamera(MeshFilter mFilter)
    {
        //_checkMeshCamera.transform.position = camPosition;
        //_checkMeshCamera.transform.rotation = camRotation;

        var camPlanes = GeometryUtility.CalculateFrustumPlanes(_checkMeshCamera);

        var bounds = mFilter.GetComponent<MeshRenderer>().localBounds;
        Vector3[] points = new Vector3[8];
        points[0] = bounds.center + new Vector3(-bounds.size.x / 2, -bounds.size.y / 2, -bounds.size.z / 2);
        points[1] = bounds.center + new Vector3(-bounds.size.x / 2, bounds.size.y / 2, -bounds.size.z / 2);
        points[2] = bounds.center + new Vector3(bounds.size.x / 2, bounds.size.y / 2, -bounds.size.z / 2);
        points[3] = bounds.center + new Vector3(bounds.size.x / 2, -bounds.size.y / 2, -bounds.size.z / 2);
        points[4] = bounds.center + new Vector3(-bounds.size.x / 2, -bounds.size.y / 2, bounds.size.z / 2);
        points[5] = bounds.center + new Vector3(-bounds.size.x / 2, bounds.size.y / 2, bounds.size.z / 2);
        points[6] = bounds.center + new Vector3(bounds.size.x / 2, bounds.size.y / 2, bounds.size.z / 2);
        points[7] = bounds.center + new Vector3(bounds.size.x / 2, -bounds.size.y / 2, bounds.size.z / 2);

        var listcolliders = new List<SphereCollider>();
        foreach (var point in points)
        {
            var go = new GameObject("point");

            var tr = go.transform;

            tr.parent = mFilter.transform;
            tr.localPosition = point;
            var col = go.AddComponent<SphereCollider>();
            listcolliders.Add(col);

            col.radius = 0.01f;
        }

        int countCollidersInFrustrum = 0;
        foreach (var col in listcolliders)
        {
            if (GeometryUtility.TestPlanesAABB(camPlanes, col.bounds))
                countCollidersInFrustrum++;

            Destroy(col.gameObject);
        }

        if (countCollidersInFrustrum == 8)
            return true;
        else
            return false;
    }
}

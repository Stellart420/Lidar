using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSlicer : MonoBehaviour
{
    [SerializeField] private Transform _generatedMeshesRoot;
    [SerializeField] private MeshFilter _baseMesh;
    [SerializeField] private MeshFilter _testMeshFilter;

    [SerializeField] private int _maxTrisCount = 1000;

    [ContextMenu("Slice")]
    public void SliceMesh()
    {
        var baseMesh = _baseMesh.sharedMesh;

        var baseMeshVerts = baseMesh.vertices;
        var baseMeshTris = baseMesh.triangles;
        var baseMeshNormals = baseMesh.normals;


        var copiedTriangles = new List<Vector3>();

        for (int trianglNum = 0; trianglNum < baseMeshTris.Length; trianglNum += 3)
        {
            copiedTriangles.Add(new Vector3(baseMeshTris[trianglNum], baseMeshTris[trianglNum + 1], baseMeshTris[trianglNum + 2]));
        }

        Dictionary<int, Vector3> copiedVerts = new Dictionary<int, Vector3>();

        foreach(var triangle in copiedTriangles)
        {
            var intX = (int)triangle.x;
            var intY = (int)triangle.y;
            var intZ = (int)triangle.z;


            if (!copiedVerts.ContainsKey(intX))
            {
                copiedVerts.Add(intX, baseMeshVerts[intX]);
            }

            if (!copiedVerts.ContainsKey(intY))
            {
                copiedVerts.Add(intY, baseMeshVerts[intY]);
            }

            if (!copiedVerts.ContainsKey(intZ))
            {
                copiedVerts.Add(intZ, baseMeshVerts[intZ]);
            }
        }

        List<Vector3> realCopiedVerts = new List<Vector3>(); // verts
        int realCopiedVertsIndex = 0;

        Dictionary<int, int> vertsIdRestrucs = new Dictionary<int, int>();

        foreach(var copVert in copiedVerts)
        {
            realCopiedVerts.Add(copVert.Value);
            vertsIdRestrucs.Add(copVert.Key, realCopiedVertsIndex);
            ++realCopiedVertsIndex;
        }

        var realCopiedTriangles = new List<Vector3>();

        foreach(var preTriangle in copiedTriangles)
        {
            var intX = (int)preTriangle.x;
            var intY = (int)preTriangle.y;
            var intZ = (int)preTriangle.z;


            realCopiedTriangles.Add(new Vector3(vertsIdRestrucs[intX], vertsIdRestrucs[intY], vertsIdRestrucs[intZ]));
        }

        Vector3[] verts = realCopiedVerts.ToArray();
        int[] tris = new int[realCopiedTriangles.Count * 3];


        for(var trNum = 0; trNum < realCopiedTriangles.Count; trNum += 3)
        {
            tris[trNum] = (int)realCopiedTriangles[trNum].x;
            tris[trNum+1] = (int)realCopiedTriangles[trNum].y;
            tris[trNum+2] = (int)realCopiedTriangles[trNum].z;
        }

        var newMesh = new Mesh();
        newMesh.vertices = verts;
        newMesh.triangles = tris;
        newMesh.RecalculateNormals();

        _testMeshFilter.sharedMesh = newMesh;
    }

    [ContextMenu("Counts")]
    public void ShowCounts()
    {
        var baseMesh = _baseMesh.sharedMesh;

        var baseMeshVerts = baseMesh.vertices;
        var baseMeshTris = baseMesh.triangles;
        var baseMeshNormals = baseMesh.normals;

        //Debug.Log($"{baseMeshVerts.Length}__{baseMeshTris.Length}");

        foreach(var v in baseMeshVerts)
        {
            Debug.Log($"{v}");
        }
    }
}

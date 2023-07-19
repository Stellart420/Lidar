using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudPointBehaviour : MonoBehaviour
{
    private static int _pointsCount = 0;

    void Start()
    {
        Debug.Log($"Add point: {_pointsCount}");
        ++_pointsCount;
    }

}

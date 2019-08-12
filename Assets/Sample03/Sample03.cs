using System.Collections;
using System.Collections.Generic;
using QHull;
using UnityEngine;

public class Sample03 : MonoBehaviour
{
    public MeshFilter ori ;
    public GameObject prefab;

    public void Awake()
    {
        Vector3[] v3s = ori.mesh.vertices;
        EightBlockTree eightTree = new EightBlockTree();
        var points = eightTree.Build(v3s);


        foreach (var point in points)
        {
            Instantiate(prefab, point, Quaternion.identity);
        }
    }
}

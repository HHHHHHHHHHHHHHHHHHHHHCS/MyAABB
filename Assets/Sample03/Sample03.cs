using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample03 : MonoBehaviour
{
    public MeshFilter mesh;
    public GameObject prefab;

    public void Awake()
    {
        Vector3[] v3s = mesh.mesh.vertices;
        AABB aabb = new AABB();
        var points = aabb.Build(v3s);
        foreach (var point in points)
        {
            Instantiate(prefab, point, Quaternion.identity);
        }
    }
}

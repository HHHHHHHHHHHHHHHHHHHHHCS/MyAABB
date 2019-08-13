using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using QHull;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Sample05 : MonoBehaviour
{
    public MeshFilter ori, col;
    public bool useEight;

    public void Awake()
    {
        Stopwatch sw = Stopwatch.StartNew();
        Vector3[] points = ori.mesh.vertices;
        if (useEight)
        {
            EightBlockTree eightTree = new EightBlockTree();
            points = eightTree.Build(points,5);
        }

        QuickHull3D hull = new QuickHull3D();
        hull.Build(points);

        Vector3[] vertices = hull.GetVertices();

        int[] faceIndices = hull.GetFaces();

        Mesh mesh = new Mesh { vertices = vertices, triangles = faceIndices };
        col.mesh = mesh;
        sw.Stop();
        Debug.Log(sw.Elapsed);
    }


}

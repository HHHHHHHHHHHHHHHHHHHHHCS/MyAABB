using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExMethod
{
    public static void AddPoint(this List<Vector3> list, double x, double y, double z)
    {
        list.Add(new Vector3((float) x, (float) y, (float) z));
    }

    public static void AddPoint(this List<Vector3> list, float x, float y, float z)
    {
        list.Add(new Vector3(x, y, z));
    }
}

public class Sample02 : MonoBehaviour
{
    private void Awake()
    {
        Vector3[] pointArray = AddPointData();
        QuickHull3D hull = new QuickHull3D();
        hull.Build((float[]) pointArray);

        Vector3[] vertices = hull.GetVertices();

        int[][] faceIndices = hull.GetFaces();
    }


    private Vector3[] AddPointData()
    {
        List<Vector3> points = new List<Vector3>();
        points.AddPoint(0.0, 0.0, 0.0);
        points.AddPoint(1.0, 0.5, 0.0);
        points.AddPoint(2.0, 0.0, 0.0);
        points.AddPoint(0.5, 0.5, 0.5);
        points.AddPoint(0.0, 0.0, 2.0);
        points.AddPoint(0.1, 0.2, 0.3);
        points.AddPoint(0.0, 2.0, 0.0);
        points.AddPoint(-1.0, 0, 0.0);
        points.AddPoint(0.0, 0.0, -1.0);
        points.AddPoint(-1.0, -1.0, 1.0);
        return points.ToArray();
    }
}
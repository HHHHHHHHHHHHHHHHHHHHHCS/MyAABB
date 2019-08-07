using System.Collections;
using System.Collections.Generic;
using System.Text;
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
        hull.Build(pointArray);

        StringBuilder sb = new StringBuilder();
        sb.Append("Vertices:");
        Vector3[] vertices = hull.GetVertices();
        foreach (var vert in vertices)
        {
            sb.Append(vert.ToString());
        }
        Debug.Log(sb.ToString());

        sb.Clear();
        sb.Append("Faces:");
        int[][] faceIndices = hull.GetFaces();
        foreach (var faceIndex in faceIndices)
        {
            foreach (var index in faceIndex)
            {
                sb.Append(index.ToString() + " ");
            }

            sb.Append('\n');
        }
        Debug.Log(sb.ToString());
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
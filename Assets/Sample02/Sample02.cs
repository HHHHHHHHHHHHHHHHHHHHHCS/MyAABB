using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public Mesh mesh;
    public GameObject prefab;

    private IEnumerator Start()
    {
        Vector3[] pointArray = AddPointData();
        pointArray = mesh.vertices;

        QuickHull3D hull = new QuickHull3D();
        yield return new WaitForSeconds(0.1f);
        hull.Build(pointArray);

        StringBuilder sb = new StringBuilder();
        sb.Append("Vertices:" + '\n');
        Vector3[] vertices = hull.GetVertices();
        foreach (var vert in vertices)
        {
            sb.Append(vert.ToString() + '\n');
        }

        Debug.Log(sb.ToString());

        foreach (var v3 in vertices)
        {
            Instantiate(prefab, v3, Quaternion.identity);
        }

        sb.Clear();
        sb.Append("Faces:" + '\n');
        int[][] faceIndices = hull.GetFaces();
        for (int i = 0; i < faceIndices.Length && i < vertices.Length; i++)
        {
            for (int j = 0; j < faceIndices[i].Length; j++)
            {
                sb.Append(faceIndices[i][j] + " ");
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

    private void ToString(Vector3[] v3s)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("        points = new Point3d[]{\n");
        foreach (var v3 in v3s)
        {
            sb.Append($"                        new Point3d({v3.x},{v3.y},{v3.z}),\n");
        }

        sb.Append("        };");
        var bbs = System.Text.Encoding.Default.GetBytes(sb.ToString());
        using (var fs = File.Open("tt.txt", FileMode.Create))
        {
            fs.Write(bbs,0, bbs.Length);
        }
    }
}
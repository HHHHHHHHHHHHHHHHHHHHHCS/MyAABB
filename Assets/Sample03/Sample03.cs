using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample03 : MonoBehaviour
{
    private struct FaceStruct
    {
        public int x, y, z;

        public FaceStruct(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
    }

    List<Vector3> points = new List<Vector3>();
    List<Vector3> results = new List<Vector3>();
    List<FaceStruct> faces = new List<FaceStruct>();

    private bool isDraw = false;

    private void Awake()
    {
        isDraw = true;
    }

    private void OnDrawGizmos()
    {
        if (!isDraw)
        {
            return;
        }

        if (points.Count == 0)
        {
            AddPoints(0.0, 0.0, 0.0);
            AddPoints(1.0, 0.5, 0.0);
            AddPoints(2.0, 0.0, 0.0);
            AddPoints(0.5, 0.5, 0.5);
            AddPoints(0.0, 0.0, 2.0);
            AddPoints(0.1, 0.2, 0.3);
            AddPoints(0.0, 2.0, 0.0);
            AddPoints(-1.0, 0, 0.0);
            AddPoints(0.0, 0.0, -1.0);
            AddPoints(-1.0, -1.0, 1.0);
        }

        if (results.Count == 0)
        {
            AddResult(2.0, 0.0, 0.0);
            AddResult(0.0, 0.0, 2.0);
            AddResult(0.0, 2.0, 0.0);
            AddResult(-1.0, 0.0, 0.0);
            AddResult(0.0, 0.0, -1.0);
            AddResult(-1.0, -1.0, 1.0);
        }

        if (faces.Count == 0)
        {
            DrawFace(2, 1, 0);
            DrawFace(4, 3, 2);
            DrawFace(4, 2, 0);
            DrawFace(5, 3, 4);
            DrawFace(5, 4, 0);
            DrawFace(5, 0, 1);
        }

        Gizmos.color = Color.white;
        foreach (var item in points)
        {
            Gizmos.DrawSphere(item, 0.05f);
        }


        Gizmos.color = Color.red;
        foreach (var item in results)
        {
            Gizmos.DrawSphere(item, 0.05f);
        }

        Gizmos.color = Color.green;
        foreach (var item in faces)
        {
            DrawFace(item.x, item.y, item.z);
        }
    }

    private void AddPoints(double x, double y, double z)
    {
        AddPoints((float) x, (float) y, (float) z);
    }

    private void AddPoints(float x, float y, float z)
    {
        points.Add(new Vector3(x, y, z));
    }

    private void AddResult(double x, double y, double z)
    {
        AddResult((float) x, (float) y, (float) z);
    }

    private void AddResult(float x, float y, float z)
    {
        results.Add(new Vector3(x, y, z));
    }

    private void DrawFace(int x, int y, int z)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(results[x], results[y]);
        Gizmos.DrawLine(results[y], results[z]);
        Gizmos.DrawLine(results[z], results[x]);
    }
}
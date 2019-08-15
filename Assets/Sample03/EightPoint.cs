using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EightPoint
{
    private static Vector3 point = Vector3.zero;

    public float xMin, xMax, yMin, yMax, zMin, zMax;

    public EightPoint()
    {
    }

    public void OnInit(Vector3 start, Vector3 offset)
    {
        xMax = start.x + offset.x;
        yMax = start.y + offset.y;
        zMax = start.z + offset.z;

        xMin = start.x;
        yMin = start.y;
        zMin = start.z;
    }

    /// <summary>
    /// 得到block的八个点
    /// </summary>
    public void AddEightPoints(Vector3[] arr = null, int index = 0)
    {
        arr[index + 0] = ResetPoint(xMin, yMin, zMin);
        arr[index + 1] = ResetPoint(xMax, yMin, zMin);
        arr[index + 2] = ResetPoint(xMin, yMin, zMax);
        arr[index + 3] = ResetPoint(xMax, yMin, zMax);
        arr[index + 4] = ResetPoint(xMin, yMax, zMin);
        arr[index + 5] = ResetPoint(xMax, yMax, zMin);
        arr[index + 6] = ResetPoint(xMin, yMax, zMax);
        arr[index + 7] = ResetPoint(xMax, yMax, zMax);
    }

    public Vector3 ResetPoint(float x, float y, float z)
    {
        point.x = x;
        point.y = y;
        point.z = z;
        return point;
    }
}
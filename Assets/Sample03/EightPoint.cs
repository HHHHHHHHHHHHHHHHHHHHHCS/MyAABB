using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EightPoint
{
    public float xMin, xMax, yMin, yMax, zMin, zMax;


    private Vector3 center = Vector3.zero;


    public Vector3 Center
    {
        get
        {
            center.x = (xMax + xMin) / 2;
            center.y = (yMax + yMin) / 2;
            center.z = (zMax + zMin) / 2;
            return center;
        }
    }

    public EightPoint()
    {
    }

    public EightPoint(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        this.xMin = xMin;
        this.xMax = xMax;
        this.yMin = yMin;
        this.yMax = yMax;
        this.zMin = zMin;
        this.zMax = zMax;
    }

    public void Reset(Vector3 start, Vector3 offset)
    {
        this.xMax = start.x + offset.x;
        this.yMax = start.y + offset.y;
        this.zMax = start.z + offset.z;

        this.xMin = start.x;
        this.yMin = start.y;
        this.zMin = start.z;
    }

    /// <summary>
    /// 得到block的八个点
    /// </summary>
    public List<Vector3> AddEightPoints(List<Vector3> list = null)
    {
        if (list == null)
        {
            list = new List<Vector3>(8);
        }

        list.Add(new Vector3(xMin, yMin, zMin));
        list.Add(new Vector3(xMax, yMin, zMin));
        list.Add(new Vector3(xMin, yMin, zMax));
        list.Add(new Vector3(xMax, yMin, zMax));
        list.Add(new Vector3(xMin, yMax, zMin));
        list.Add(new Vector3(xMax, yMax, zMin));
        list.Add(new Vector3(xMin, yMax, zMax));
        list.Add(new Vector3(xMax, yMax, zMax));

        return list;
    }

    public bool CheckPointInBlock(Vector3 point, bool isBorder = false)
    {
        if (isBorder)
        {
            return xMin <= point.x && point.x <= xMax
                                   && yMin <= point.y && point.y <= yMax
                                   && zMin <= point.z && point.z <= zMax;
        }

        return xMin <= point.x && point.x < xMax
                               && yMin <= point.y && point.y < yMax
                               && zMin <= point.z && point.z < zMax;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class EightBlockTree
{
    public const int c_cutCount = 3;
    public const float c_sameDistance = 0.0001f; //相近的距离 QuaickHull是0.00000001f

    public Vector3[] Build(Vector3[] _points, int _cutCount = c_cutCount)
    {
        int count = (int) Mathf.Pow(2, _cutCount);
        int border = count - 1;

        EightPoint ep = FindFirstMaxMin(_points);
        Vector3 min = new Vector3(ep.xMin, ep.yMin, ep.zMin);
        Vector3 max = new Vector3(ep.xMax, ep.yMax, ep.zMax);
        Vector3 step = (max - min) / count;
        List<Vector3Int> blocks = new List<Vector3Int>(count);
        Vector3Int v3int = Vector3Int.zero;

        foreach (var point in _points)
        {
            v3int.x = (int) Mathf.Clamp((point.x - min.x) / step.x, 0, border);
            v3int.y = (int) Mathf.Clamp((point.y - min.y) / step.y, 0, border);
            v3int.z = (int) Mathf.Clamp((point.z - min.z) / step.z, 0, border);

            bool canAdd = true;
            foreach (var block in blocks)
            {
                if (block.x == v3int.x && block.y == v3int.y && block.z == v3int.z)
                {
                    canAdd = false;
                    break;
                }
            }

            if (canAdd)
            {
                blocks.Add(v3int);
            }
        }

        Vector3[] result = new Vector3[blocks.Count * 8];
        Vector3 start = Vector3.zero;

        for (int i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            start.x = min.x + block.x * step.x;
            start.y = min.y + block.y * step.y;
            start.z = min.z + block.z * step.z;
            ep.OnInit(start, step);
            ep.AddEightPoints(result, i * 8);
        }

        RemoveRepeatedPoint(result);
        return result;
    }


    /// <summary>
    /// 找到第一次最大小的点
    /// 如果是面片会自己加框
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public EightPoint FindFirstMaxMin(Vector3[] points)
    {
        EightPoint ep = new EightPoint();
        if (points.Length > 0)
        {
            ep.xMin = ep.xMax = points[0].x;
            ep.yMin = ep.yMax = points[0].y;
            ep.zMin = ep.zMax = points[0].z;
        }
        else
        {
            throw new Exception("Points Length is zero");
        }

        foreach (var point in points)
        {
            if (point.x < ep.xMin)
            {
                ep.xMin = point.x;
            }
            else if (point.x > ep.xMax)
            {
                ep.xMax = point.x;
            }

            if (point.y < ep.yMin)
            {
                ep.yMin = point.y;
            }
            else if (point.y > ep.yMax)
            {
                ep.yMax = point.y;
            }

            if (point.z < ep.zMin)
            {
                ep.zMin = point.z;
            }
            else if (point.z > ep.zMax)
            {
                ep.zMax = point.z;
            }
        }

        if (ep.xMax - ep.xMin < c_sameDistance)
        {
            ep.xMin -= c_sameDistance;
            ep.xMax += c_sameDistance;
        }

        if (ep.yMax - ep.yMin < c_sameDistance)
        {
            ep.yMin -= c_sameDistance;
            ep.yMax += c_sameDistance;
        }

        if (ep.zMax - ep.zMin < c_sameDistance)
        {
            ep.zMin -= c_sameDistance;
            ep.zMax += c_sameDistance;
        }

        return ep;
    }


    /// <summary>
    /// 删除重复的点 虽然在QHull中也会判断 提前剪枝
    /// </summary>
    /// <param name="eps"></param>
    public void RemoveRepeatedPoint(Vector3[] eps)
    {
        Vector3 v3 = Vector3.zero;
        int end = eps.Length - 1;
        for (int i = eps.Length - 1; i >= 0; i--)
        {
            var oriPoint = eps[i];
            for (int j = i - 1; j >= 0; j--)
            {
                v3.x = oriPoint.x - eps[j].x;
                v3.y = oriPoint.y - eps[j].y;
                v3.z = oriPoint.z - eps[j].z;
                if (v3.x * v3.x + v3.y * v3.y + v3.z * v3.z <= c_sameDistance)
                {
                    eps[i] = eps[end];
                    end--;
                    break;
                }
            }
        }

        Array.Resize(ref eps, end + 1);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EightBlockTree : MonoBehaviour
{
    public const int c_cutCount = 5;
    public const float c_sameDistance = 0.001f;//相近的距离

    public Vector3[] Build(Vector3[] _points, int _cutCount = c_cutCount)
    {
        List<Vector3> pointList = new List<Vector3>(_points);
        int count = (int) Mathf.Pow(2, _cutCount);
        int border = count - 1;

        EightPoint ep = FindFirstMaxMin(pointList);
        List<Vector3> result = new List<Vector3>(count);
        Vector3 min = new Vector3(ep.xMin, ep.yMin, ep.zMin);
        Vector3 max = new Vector3(ep.xMax, ep.yMax, ep.zMax);
        Vector3 step = (max - min) / count;
        Vector3 start;
        //TODO:pos-min)/step
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                for (int k = 0; k < count; k++)
                {
                    if (pointList.Count <= 0)
                    {
                        //强行跳出
                        i = j = k = count;
                        break;
                    }

                    start.x = min.x + i * step.x;
                    start.y = min.y + j * step.y;
                    start.z = min.z + k * step.z;

                    ep.Reset(start, step);
                    if (CheckRemovePoint(ep, pointList, i == border || j == border || k == border))
                    {
                        ep.AddEightPoints(result);
                    }
                }
            }
        }

        RemoveRepeatedPoint(result);
        return result.ToArray();
    }


    /// <summary>
    /// 找到第一次最大小的点
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public EightPoint FindFirstMaxMin(List<Vector3> points)
    {
        EightPoint ep = new EightPoint();
        if (points.Count > 0)
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

        return ep;
    }

    /// <summary>
    /// 删除空的盒子
    /// </summary>
    /// <param name="_ep"></param>
    /// <param name="_points"></param>
    /// <param name="isBorder"></param>
    /// <returns></returns>
    public bool CheckRemovePoint(EightPoint _ep, List<Vector3> _points, bool isBorder = false)
    {
        bool result = false;

        for (int i = _points.Count - 1; i >= 0; i--)
        {
            if (_ep.CheckPointInBlock(_points[i], isBorder))
            {
                result = true;
                _points.RemoveAt(i);
            }
        }

        return result;
    }


    /// <summary>
    /// 删除重复的点 虽然在QHull中也会判断 提前剪枝
    /// </summary>
    /// <param name="eps"></param>
    public void RemoveRepeatedPoint(List<Vector3> eps)
    {
        List<int> removeIndexs = new List<int>();
        Vector3 v3 = Vector3.zero;
        for (int i = eps.Count - 1; i >= 0; i--)
        {
            if (removeIndexs.Contains(i))
            {
                continue;
            }

            var oriPoint = eps[i];
            for (int j = i - 1; j >= 0; j--)
            {
                v3.x = Mathf.Abs(oriPoint.x - eps[j].x);
                v3.y = Mathf.Abs(oriPoint.y - eps[j].y);
                v3.z = Mathf.Abs(oriPoint.z - eps[j].z);
                if (v3.x + v3.y + v3.z <= c_sameDistance)
                {
                    removeIndexs.Add(j);
                }
            }
        }

        //倒序删除
        removeIndexs.Sort((x, y) => x < y ? 1 : -1);
        foreach (var index in removeIndexs)
        {
            eps.RemoveAt(index);
        }
    }
}
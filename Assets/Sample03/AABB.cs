using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AABB
{
    public const int cutCount = 5;


    public List<Vector3> Build(Vector3[] _points)
    {
        AABBBlock aabbBlock = new AABBBlock();
        List<List<Vector3>> lists = new List<List<Vector3>>(cutCount);
        List<EightPoint> minMaxBoxs = new List<EightPoint>();
        lists.Add(_points.ToList());
        minMaxBoxs.Add(AABBBlock.FindFirstMaxMin(lists[0]));

        for (int i = 0; i < cutCount; i++)
        {
            for (int j = lists.Count - 1; j >= 0; j--)
            {
                aabbBlock.Build(lists[j], minMaxBoxs[j], out var blocks, out var box);
                lists.AddRange(blocks);
                minMaxBoxs.AddRange(box);
                lists.RemoveAt(j);
                minMaxBoxs.RemoveAt(j);
            }
        }
        List<Vector3> endPoints = new List<Vector3>(minMaxBoxs.Count * 8);
        foreach (var box in minMaxBoxs)
        {
            endPoints.AddRange(box.GetEightPoints());
        }

        RemoveRepeatedPoint(endPoints);
        return endPoints;
    }

    /// <summary>
    /// 删除重复的点 虽然在QHull中也会判断 提前剪枝
    /// </summary>
    /// <param name="eps"></param>
    public void RemoveRepeatedPoint(List<Vector3> eps)
    {
        List<int> removIndexs = new List<int>();
        Vector3 v3 = Vector3.zero;
        for (int i = eps.Count -1; i >= 0; i--)
        {
            if (removIndexs.Contains(i))
            {
                continue;
            }

            var oriPoint = eps[i];
            for (int j = i - 1; j >= 0; j--)
            {
                v3.x = Mathf.Abs(oriPoint.x - eps[j].x);
                v3.y = Mathf.Abs(oriPoint.y - eps[j].y);
                v3.z = Mathf.Abs(oriPoint.z - eps[j].z);
                if ((v3.x + v3.y + v3.z) <= 0.0001f)
                {
                    removIndexs.Add(j);
                }
            }
        }
    }
}
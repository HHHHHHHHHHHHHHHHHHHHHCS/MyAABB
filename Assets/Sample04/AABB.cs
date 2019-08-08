using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AABB
{
    public const int cutCount = 3;


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
        //TODO:判断重复的点
        return endPoints;
    }
}
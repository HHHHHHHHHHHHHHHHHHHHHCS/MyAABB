using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickHull3D
{
    /// <summary>
    /// 按照顺时针输出顶点
    /// </summary>
    public const int c_Clockwise = 0x1;

    /// <summary>
    /// 在输出的时候面的顶点是从1开始的
    /// </summary>
    public const int c_IndexFromOne = 0x2;

    /// <summary>
    /// 在输出的时候面的顶点是从0开始的
    /// </summary>
    public const int c_IndexFromZero = 0x4;

    /// <summary>
    /// 在输出的时候面的顶点是相对于输入顶点的编号
    /// </summary>
    public const int c_PointRelative = 0x8;

    /// <summary>
    /// 用公差去计算距离
    /// </summary>
    public const double c_AutomaticTolerance = -1;

    protected int findIndex = -1;

    protected double charLength;

    protected bool debug = false;


    public void Build(Vector3[] points)
    {
        Build(points,points.Length);
    }

    public void Build(Vector3[] points,int length)
    {

    }

    public Vector3[] GetVertices()
    {
        return null;
    }

    public int[][] GetFaces()
    {
        return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 三角面
/// </summary>
public class Face
{
    /// <summary>
    /// 状态:可见的
    /// </summary>
    private const int c_visible = 1;

    /// <summary>
    /// 状态:不是凸出的形状
    /// </summary>
    private const int c_noneConvex = 2;

    /// <summary>
    /// 状态:删除的
    /// </summary>
    private const int c_deleted = 3;

    /// <summary>
    /// 第0条边
    /// </summary>
    private HalfEdge he0;

    /// <summary>
    /// 法线
    /// </summary>
    private Vector3 normal;

    /// <summary>
    /// 面积
    /// </summary>
    private float area;

    /// <summary>
    /// 中心点
    /// </summary>
    private Vector3 centerOid;

    /// <summary>
    /// 面板偏移
    /// </summary>
    private float planeOffset;

    /// <summary>
    /// face的Index
    /// </summary>
    private int index;

    /// <summary>
    /// 顶点数量
    /// </summary>
    private int numVerts;

    /// <summary>
    /// 下一个三角面
    /// </summary>
    private Face next;

    /// <summary>
    /// 当前的状态
    /// </summary>
    private int mark = c_visible;

    /// <summary>
    /// 外侧的点
    /// </summary>
    private Vertex outSide;

    public void ComputeCentroid()
    {
        centerOid = Vector3.zero;
        HalfEdge he = he0;
        do
        {
            centerOid += he.Head;
            he = he.Next;
        } while (he != he0);

        centerOid /= numVerts;
    }


    public void ComputeNormal(float minArea)
    {
        ComputeNormal();

        if (area < minArea)
        {
            //用来处理四边形以上的时候,不再一个平面的法线
            //通过删除最长的边 来让法线更准确

            HalfEdge hedgeMax = null;
            float lenSqrMax = 0;
            HalfEdge hedge = he0;
            do
            {//找出最长的边
                float lenSqr = hedge.LengthSquared();
                if (lenSqr > lenSqrMax)
                {
                    lenSqrMax = lenSqr;
                }

                hedge = hedge.Next;
            } while (hedge != he0);

            Vector3 p2 = hedgeMax.Head.pnt;
            Vector3 p1 = hedgeMax.Tail.pnt;

            float lenMax = Mathf.Sqrt(lenSqrMax);

            Vector3 p21 = (p2 - p1) / lenMax;

            float dot = Vector3.Dot(normal, p21);

            normal -= dot * p21;

            normal.Normalize();
        }
    }


    public void ComputeNormal()
    {
        HalfEdge he1 = he0.Next;
        HalfEdge he2 = he1.Next;

        Vector3 p0 = he0.Head.pnt;
        Vector3 d2 = he1.Head.pnt - p0;

        normal = Vector3.zero;
        numVerts = 2;

        while (he2 != he0)
        {
            Vector3 oD2 = d2;

            d2 = he2.Head.pnt - p0;

            normal += Vector3.Cross(oD2, d2);

            he2 = he2.Next;
            numVerts++;
        }

        area = normal.magnitude; //叉积的绝对值的一半 是面积
        normal.Normalize();
    }
}
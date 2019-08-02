using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 三角面的高的边
/// </summary>
public class HalfEdge
{
    /// <summary>
    /// 跟这个边有关的 头的顶点
    /// </summary>
    private Vertex vertex;

    /// <summary>
    /// 跟这个边有关的三角面
    /// </summary>
    private Face face;

    /// <summary>
    /// 三角面的下面一条边
    /// </summary>
    public HalfEdge Next { get; set; }

    /// <summary>
    /// 三角面的上面一条边
    /// </summary>
    public HalfEdge Prev { get; set; }

    /// <summary>
    /// 与此边缘相对的半边
    /// </summary>
    private HalfEdge opposite;

    /// <summary>
    /// 与此边缘相对的半边
    /// </summary>
    public HalfEdge Opposite
    {
        get => opposite;
        set
        {
            opposite = value;
            value.opposite = this;
        }
    }

    /// <summary>
    /// 半边的头点
    /// </summary>
    public Vertex Head => vertex;

    /// <summary>
    /// 半边的尾点
    /// </summary>
    public Vertex Tail => Prev?.vertex;

    public HalfEdge()
    {
    }

    /// <summary>
    /// 使用头部的点和左侧的三角面 构建
    /// </summary>
    /// <param name="v"></param>
    /// <param name="f"></param>
    public HalfEdge(Vertex v, Face f)
    {
        vertex = v;
        face = f;
    }

    /// <summary>
    /// 得到头尾 尾巴的Index 的string
    /// </summary>
    /// <returns></returns>
    public string GetVertexString()
    {
        return $"{(Tail != null ? Tail.index.ToString() : "?")}-{Head.index}";
    }

    /// <summary>
    /// 长度
    /// </summary>
    /// <returns></returns>
    public float Length()
    {
        return Tail != null ? Vector3.Distance(Head, Tail) : -1f;
    }

    /// <summary>
    /// 平方长度
    /// </summary>
    /// <returns></returns>
    public float LengthSquared()
    {
        return Tail != null ? Head.pnt.DistanceSquared(Tail) : -1f;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 简单的顶点链表
/// </summary>
public class VertexList 
{
    private Vertex head;
    private Vertex tail;

    /// <summary>
    /// 清除列表
    /// </summary>
    public void Clear()
    {
        head = tail = null;
    }

    /// <summary>
    /// 添加顶点到顶点链表
    /// </summary>
    /// <param name="vtx"></param>
    public void add(Vertex vtx)
    {
        if (head == null)
        {
            head = vtx;
        }
        else
        {
            tail.next = vtx;
        }
        vtx.prev = tail;
        vtx.next = null;
        tail = vtx;
    }

    /// <summary>
    /// 添加顶点到顶点链表 并且重新遍历一下
    /// </summary>
    /// <param name="vtx"></param>
    public void AddAll(Vertex vtx)
    {
        if (head == null)
        {
            head = vtx;
        }
        else
        {
            tail.next = vtx;
        }
        vtx.prev = tail;
        while (vtx.next != null)
        {
            vtx = vtx.next;
        }
        tail = vtx;
    }

    /// <summary>
    /// 从链表中删除一个点
    /// </summary>
    /// <param name="vtx"></param>
    public void Delete(Vertex vtx)
    {
        if (vtx.prev == null)
        {
            head = vtx.next;
        }
        else
        {
            vtx.prev.next = vtx.next;
        }
        if (vtx.next == null)
        {
            tail = vtx.prev;
        }
        else
        {
            vtx.next.prev = vtx.prev;
        }
    }

    /// <summary>
    /// 在链表中删除两个连续的点
    /// </summary>
    /// <param name="vtx1"></param>
    /// <param name="vtx2"></param>
    public void Delete(Vertex vtx1, Vertex vtx2)
    {
        if (vtx1.prev == null)
        {
            head = vtx2.next;
        }
        else
        {
            vtx1.prev.next = vtx2.next;
        }
        if (vtx2.next == null)
        {
            tail = vtx1.prev;
        }
        else
        {
            vtx2.next.prev = vtx1.prev;
        }
    }

    /// <summary>
    /// 在一个顶点前面插入一个点
    /// </summary>
    /// <param name="vtx"></param>
    /// <param name="next"></param>
    public void InsertBefore(Vertex vtx, Vertex next)
    {
        vtx.prev = next.prev;
        if (next.prev == null)
        {
            head = vtx;
        }
        else
        {
            next.prev.next = vtx;
        }
        vtx.next = next;
        next.prev = vtx;
    }

    /// <summary>
    /// 返回头节点
    /// </summary>
    public Vertex first => head;

    /// <summary>
    /// 是否是空的
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()=> head == null;
}

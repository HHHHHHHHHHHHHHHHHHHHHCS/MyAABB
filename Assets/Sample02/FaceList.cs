using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 简单的Face链表
/// </summary>
public class FaceList
{
    private Face head;
    private Face tail;

    
    /// <summary>
    /// 清除链表
    /// </summary>
    public void Clear()
    {
        head = tail = null;
    }

    /// <summary>
    /// 添加元素到链表
    /// </summary>
    /// <param name="vtx"></param>
    public void Add(Face face)
    {
        if (head == null)
        {
            head = face;
        }
        else
        {
            tail.next = face;
        }

        face.next = null;
        tail = face;
    }

    public Face First => head;

    /// <summary>
    /// 检查链表是否为空
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()=> head == null;
}

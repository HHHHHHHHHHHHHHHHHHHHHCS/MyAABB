namespace QHull
{
    using System;

    /// <summary>
    /// 顶点的链表
    /// </summary>
    public class VertexList
    {
        private Vertex head;
        private Vertex tail;

        /// <summary>
        /// 清空链表
        /// </summary>
        public void Clear()
        {
            head = tail = null;
        }

        /// <summary>
        /// 添加一个顶点到链表到最后
        /// </summary>
        /// <param name="vtx"></param>
        public void Add(Vertex vtx)
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
        /// 添加一个链表到当前链表的末尾
        /// </summary>
        /// <param name="vtx"></param>
        public void AddRange(Vertex vtx)
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
        /// 删除一个顶点
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
        /// 删除两个连续的顶点
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
        /// 在指定顶点前面插入一个顶点
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
        /// 返回头顶点
        /// </summary>
        public Vertex First => head;


        /// <summary>
        /// 是否是空的链表  
        /// </summary>
        public bool IsEmpty => head == null;
    }
}
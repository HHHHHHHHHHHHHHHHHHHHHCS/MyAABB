namespace QHull
{
    using System;

    /// <summary>
    /// ���������
    /// </summary>
    public class VertexList
    {
        private Vertex head;
        private Vertex tail;

        /// <summary>
        /// �������
        /// </summary>
        public void Clear()
        {
            head = tail = null;
        }

        /// <summary>
        /// ���һ�����㵽�������
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
        /// ���һ��������ǰ�����ĩβ
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
        /// ɾ��һ������
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
        /// ɾ�����������Ķ���
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
        /// ��ָ������ǰ�����һ������
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
        /// ����ͷ����
        /// </summary>
        public Vertex First => head;


        /// <summary>
        /// �Ƿ��ǿյ�����  
        /// </summary>
        public bool IsEmpty => head == null;
    }
}
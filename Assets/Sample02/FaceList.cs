namespace QHull
{
    using System;

    /// <summary>
    /// 面的链表
    /// </summary>
    public class FaceList
    {
        private Face head;
        private Face tail;

        /// <summary>
        /// 清空链表
        /// </summary>
        public void Clear()
        {
            head = tail = null;
        }


        /// <summary>
        /// 把面添加到链表里面
        /// </summary>
        /// <param name="vtx"></param>
        public void Add(Face vtx)
        {
            if (head == null)
            {
                head = vtx;
            }
            else
            {
                tail.next = vtx;
            }

            vtx.next = null;
            tail = vtx;
        }

        /// <summary>
        /// 返回第一个面
        /// </summary>
        /// <returns></returns>
        public Face First => head;


        /// <summary>
        /// 是否是空的链表
        /// </summary>
        public bool IsEmpty => head == null;
    }
}
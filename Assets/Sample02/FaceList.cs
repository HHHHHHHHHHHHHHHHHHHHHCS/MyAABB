namespace QHull
{
    using System;

    /// <summary>
    /// �������
    /// </summary>
    public class FaceList
    {
        private Face head;
        private Face tail;

        /// <summary>
        /// �������
        /// </summary>
        public void Clear()
        {
            head = tail = null;
        }


        /// <summary>
        /// ������ӵ���������
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
        /// ���ص�һ����
        /// </summary>
        /// <returns></returns>
        public Face First => head;


        /// <summary>
        /// �Ƿ��ǿյ�����
        /// </summary>
        public bool IsEmpty => head == null;
    }
}
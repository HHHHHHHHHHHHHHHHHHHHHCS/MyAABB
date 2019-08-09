namespace QHull
{
    using System;
    using UnityEngine;

    /// <summary>
    /// ��
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// ����ĵ��λ��
        /// </summary>
        public Vector3 pnt;

        /// <summary>
        /// ���������
        /// </summary>
        public int index;

        /// <summary>
        /// ����List ��һ����
        /// </summary>
        public Vertex prev;

        /// <summary>
        /// ����List ��һ����
        /// </summary>
        public Vertex next;

        /// <summary>
        /// �������ڵ���
        /// </summary>
        public Face face;

        /// <summary>
        /// ����һ���յĵ�
        /// </summary>
        public Vertex()
        {
            pnt = Vector3.zero;
        }

        /// <summary>
        /// ��
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="idx"></param>
        public Vertex(float x, float y, float z, int idx)
        {
            pnt = new Vector3(x, y, z);
            index = idx;
        }
    }
}
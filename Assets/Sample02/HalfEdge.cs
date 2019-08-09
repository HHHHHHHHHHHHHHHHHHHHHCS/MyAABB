using UnityEngine;

namespace QHull
{
    using System;


    /// <summary>
    /// ��
    /// </summary>
    public class HalfEdge
    {
        /// <summary>
        /// �� ��ͷ��
        /// </summary>
        public Vertex vertex;

        /// <summary>
        /// �� �����ڵ���
        /// </summary>
        public Face face;

        /// <summary>
        /// ��һ����
        /// </summary>
        public HalfEdge next;

        /// <summary>
        /// ��һ����
        /// </summary>
        public HalfEdge prev;

        /// <summary>
        /// ����ı�
        /// </summary>
        public HalfEdge opposite;

        /// <summary>
        /// ��ͷ����� ����һ����
        /// </summary>
        /// <param name="v"></param>
        /// <param name="f"></param>
        public HalfEdge(Vertex v, Face f)
        {
            vertex = v;
            face = f;
        }

        /// <summary>
        /// �յĹ��캯�� ����
        /// </summary>
        public HalfEdge()
        {
        }


        /// <summary>
        /// ��һ����
        /// </summary>
        /// <param name="edge"></param>
        public HalfEdge Next
        {
            get => next;
            set => next = value;
        }


        /// <summary>
        /// ��һ����
        /// </summary>
        public HalfEdge Prev
        {
            get => prev;
            set => prev = value;
        }


        /// <summary>
        /// ��ǰ�������ڵ�face
        /// </summary>
        /// <returns></returns>
        public Face Face => face;


        /// <summary>
        /// ����ı�
        /// </summary>
        public HalfEdge Opposite
        {
            get => opposite;
            set => opposite = value;
        }

        /// <summary>
        /// �ߵ�ͷ��
        /// </summary>
        public Vertex Head => vertex;


        /// <summary>
        /// �ߵ�β�͵�  ����һ���ߵ�β�͵�
        /// </summary>
        /// <returns></returns>
        public Vertex Tail => prev?.vertex;


        /// <summary>
        /// �Աߵ���
        /// </summary>
        /// <returns></returns>
        public Face OppositeFace => opposite?.face;


        /// <summary>
        /// �������Ϣ ����ͷ���β��
        /// </summary>
        /// <returns></returns>
        public string GetVertexString()
        {
            return ToString();
        }

        public override string ToString()
        {
            if (Tail != null)
            {
                return "" +
                       Tail.index + "-" +
                       Head.index;
            }
            else
            {
                return "?-" + Head.index;
            }
        }

        /// <summary>
        /// ͷ�㵽β��ľ���
        /// </summary>
        /// <returns></returns>
        public float length()
        {
            if (Tail != null)
            {
                return Vector3.Distance(Head.pnt, Tail.pnt);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// ͷ���β���ƽ������
        /// </summary>
        /// <returns></returns>
        public float lengthSquared()
        {
            if (Tail != null)
            {
                return Vector3.SqrMagnitude(Head.pnt - Tail.pnt);
            }
            else
            {
                return -1;
            }
        }
    }
}
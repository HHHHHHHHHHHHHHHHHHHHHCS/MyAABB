
namespace QHull
{
    using System;
    using UnityEngine;

    /// <summary>
    /// 边
    /// </summary>
    public class HalfEdge
    {
        /// <summary>
        /// 边 的头点
        /// </summary>
        public Vertex vertex;

        /// <summary>
        /// 边 所属于的面
        /// </summary>
        public Face face;

        /// <summary>
        /// 下一条边
        /// </summary>
        public HalfEdge next;

        /// <summary>
        /// 上一条边
        /// </summary>
        public HalfEdge prev;

        /// <summary>
        /// 对面的边
        /// </summary>
        public HalfEdge opposite;

        /// <summary>
        /// 用头点和面 构造一条边
        /// </summary>
        /// <param name="v"></param>
        /// <param name="f"></param>
        public HalfEdge(Vertex v, Face f)
        {
            vertex = v;
            face = f;
        }

        /// <summary>
        /// 空的构造函数 别用
        /// </summary>
        public HalfEdge()
        {
        }

        /// <summary>
        /// 下一条边
        /// </summary>
        /// <param name="edge"></param>
        public HalfEdge Next
        {
            get => next;
            set => next = value;
        }

        /// <summary>
        /// 上一条边
        /// </summary>
        public HalfEdge Prev
        {
            get => prev;
            set => prev = value;
        }

        /// <summary>
        /// 当前边所属于的face
        /// </summary>
        /// <returns></returns>
        public Face Face => face;

        /// <summary>
        /// 对面的边
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
        /// 边的头点
        /// </summary>
        public Vertex Head => vertex;

        /// <summary>
        /// 边的尾巴点  即上一条边的尾巴点
        /// </summary>
        /// <returns></returns>
        public Vertex Tail => prev?.vertex;


        /// <summary>
        /// 对边的面
        /// </summary>
        /// <returns></returns>
        public Face OppositeFace => opposite?.face;

        /// <summary>
        /// 输出边信息 包含头点和尾点
        /// </summary>
        /// <returns></returns>
        public string getVertexString()
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
        /// 头点到尾点的距离
        /// </summary>
        /// <returns></returns>
        public float Length()
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
        /// 头点的尾点的平方距离
        /// </summary>
        /// <returns></returns>
        public float LengthSquared()
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
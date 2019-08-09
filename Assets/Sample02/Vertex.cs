
namespace QHull
{
    using System;
    using UnityEngine;

    /// <summary>
    /// 点
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// 顶点的点的位置
        /// </summary>
        public Vector3 pnt;

        /// <summary>
        /// 顶点的索引
        /// </summary>
        public int index;

        /// <summary>
        /// 顶点List 上一个点
        /// </summary>
        public Vertex prev;

        /// <summary>
        /// 顶点List 下一个点
        /// </summary>
        public Vertex next;

        /// <summary>
        /// 顶点所在的面
        /// </summary>
        public Face face;

        /// <summary>
        /// 构造一个空的点
        /// </summary>
        public Vertex()
        {
            pnt = Vector3.zero;
        }

        /// <summary>
        /// 点
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
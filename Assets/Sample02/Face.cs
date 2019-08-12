namespace QHull
{
    using UnityEngine;
    using System;

    /// <summary>
    /// 面
    /// </summary>
    public class Face
    {
        /// <summary>
        /// 第0条边
        /// </summary>
        public HalfEdge he0;

        /// <summary>
        /// 法线
        /// </summary>
        public Vector3 normal;

        /// <summary>
        /// 面积
        /// </summary>
        public float area;

        /// <summary>
        /// 中心点
        /// </summary>
        public Vector3 centroid;

        /// <summary>
        /// 面板偏移
        /// </summary>
        public float planeOffset;

        /// <summary>
        /// face的Index
        /// </summary>
        public int index;

        /// <summary>
        /// 面上顶点的数量
        /// </summary>
        public int numVerts;

        /// <summary>
        /// 链表下一个面片
        /// </summary>
        public Face next;


        /// <summary>
        /// 状态:可见的
        /// </summary>
        public const int c_Visible = 1;

        /// <summary>
        /// 状态:不是凸出的形状
        /// </summary>
        public const int c_NoneConvex = 2;

        /// <summary>
        /// 状态:删除的
        /// </summary>
        public const int c_Deleted = 3;

        /// <summary>
        /// 当前的状态
        /// </summary>
        public int mark = c_Visible;

        /// <summary>
        /// 上一次最外面的点
        /// </summary>
        public Vertex outside;

        /// <summary>
        /// 计算中心点
        /// </summary>
        /// <param name="centroid"></param>
        public void ComputeCentroid()
        {
            centroid = Vector3.zero;
            HalfEdge he = he0;
            do
            {
                centroid += (he.Head.pnt);
                he = he.next;
            } while (he != he0);

            centroid /= numVerts;
        }

        /// <summary>
        /// 计算法线 根据最小的面积
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="minArea"></param>
        public void ComputeNormal(float minArea)
        {
            ComputeNormal();

            if (area < minArea)
            {
                //用来处理四边形以上的时候,不再一个平面的法线
                //通过删除最长的边 来让法线更准确

                HalfEdge hedgeMax = null;
                float lenSqrMax = 0;
                HalfEdge hedge = he0;
                do
                {
                    float lenSqr = hedge.LengthSquared();
                    if (lenSqr > lenSqrMax)
                    {
                        hedgeMax = hedge;
                        lenSqrMax = lenSqr;
                    }

                    hedge = hedge.next;
                } while (hedge != he0);

                Vector3 p2 = hedgeMax.Head.pnt;
                Vector3 p1 = hedgeMax.Tail.pnt;
                float lenMax = Mathf.Sqrt(lenSqrMax);
                Vector3 ud = (p2 - p1) / lenMax;
                float dot = Vector3.Dot(ud, normal);
                normal -= (dot * ud);
                normal.Normalize();
            }
        }

        /// <summary>
        /// 计算法线
        /// </summary>
        public void ComputeNormal()
        {
            HalfEdge he1 = he0.next;
            HalfEdge he2 = he1.next;

            Vector3 p0 = he0.Head.pnt;
            Vector3 p2 = he1.Head.pnt;

            Vector3 d2 = p2 - p0;

            normal = Vector3.zero;

            numVerts = 2;

            while (he2 != he0)
            {
                Vector3 d1 = d2;

                p2 = he2.Head.pnt;
                d2 = p2 - p0;

                normal += Vector3.Cross(d1, d2);

                he1 = he2;
                he2 = he2.next;
                numVerts++;
            }


            //叉积的绝对值的一半 是面积
            area = normal.magnitude;
            normal /= area;
        }

        /// <summary>
        /// 计算法线和中心点
        /// </summary>
        private void ComputeNormalAndCentroid()
        {
            ComputeNormal();
            ComputeCentroid();
            planeOffset = Vector3.Dot(normal, centroid);
            int numv = 0;
            HalfEdge he = he0;
            do
            {
                numv++;
                he = he.next;
            } while (he != he0);

            if (numv != numVerts)
            {
                throw new Exception(
                    "face " + ToString() + " numVerts=" + numVerts + " should be " + numv);
            }
        }

        /// <summary>
        /// 计算法线和中心点
        /// </summary>
        private void ComputeNormalAndCentroid(float minArea)
        {
            ComputeNormal(minArea);
            ComputeCentroid();
            planeOffset = Vector3.Dot(normal, centroid);
        }

        /// <summary>
        /// 创建三角面
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Face CreateTriangle(Vertex v0, Vertex v1, Vertex v2)
        {
            return CreateTriangle(v0, v1, v2, 0);
        }

        /// <summary>
        /// 创建三角面
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="minArea"></param>
        /// <returns></returns>
        public static Face CreateTriangle(Vertex v0, Vertex v1, Vertex v2, float minArea)
        {
            Face face = new Face();
            HalfEdge he0 = new HalfEdge(v0, face);
            HalfEdge he1 = new HalfEdge(v1, face);
            HalfEdge he2 = new HalfEdge(v2, face);

            he0.prev = he2;
            he0.next = he1;
            he1.prev = he0;
            he1.next = he2;
            he2.prev = he1;
            he2.next = he0;

            face.he0 = he0;

            //计算法线 中心点  和 偏移
            face.ComputeNormalAndCentroid(minArea);
            return face;
        }

        /// <summary>
        /// 根据定的顶点和索引 创建面片
        /// </summary>
        /// <param name="vtxArray"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static Face Create(Vertex[] vtxArray, int[] indices)
        {
            Face face = new Face();
            HalfEdge hePrev = null;
            for (int i = 0; i < indices.Length; i++)
            {
                HalfEdge he = new HalfEdge(vtxArray[indices[i]], face);
                if (hePrev != null)
                {
                    he.Prev = (hePrev);
                    hePrev.Next = (he);
                }
                else
                {
                    face.he0 = he;
                }

                hePrev = he;
            }

            face.he0.Prev = (hePrev);
            hePrev.Next = (face.he0);

            //计算法线 中心点  和 偏移

            face.ComputeNormalAndCentroid();
            return face;
        }

        public Face()
        {
            normal = new Vector3();
            centroid = new Vector3();
            mark = c_Visible;
        }

        /// <summary>
        /// 得到指定索引的边
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public HalfEdge GetEdge(int i)
        {
            HalfEdge he = he0;
            while (i > 0)
            {
                he = he.next;
                i--;
            }

            while (i < 0)
            {
                he = he.prev;
                i++;
            }

            return he;
        }

        /// <summary>
        /// 得到第一条边
        /// </summary>
        /// <returns></returns>
        public HalfEdge FirstEdge => he0;


        /// <summary>
        /// 根据头尾两个点,得到边
        /// </summary>
        /// <param name="vt"></param>
        /// <param name="vh"></param>
        /// <returns></returns>
        public HalfEdge GetFindEdge(Vertex vt, Vertex vh)
        {
            HalfEdge he = he0;
            do
            {
                if (he.Head == vh && he.Tail == vt)
                {
                    return he;
                }

                he = he.next;
            } while (he != he0);

            return null;
        }

        /// <summary>
        /// 点到面片的距离
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public float DistanceToPlane(Vector3 p)
        {
            return Vector3.Dot(normal, p) - planeOffset;
        }

        /// <summary>
        /// 法线
        /// </summary>
        public Vector3 Normal => normal;

        /// <summary>
        /// 中心点
        /// </summary>
        public Vector3 Centroid => centroid;

        /// <summary>
        /// 顶点的数量
        /// </summary>
        public int NumVertices => numVerts;

        public override string ToString()
        {
            string s = null;
            HalfEdge he = he0;
            do
            {
                if (s == null)
                {
                    s = "" + he.Head.index;
                }
                else
                {
                    s += " " + he.Head.index;
                }

                he = he.next;
            } while (he != he0);

            return s;
        }

        /// <summary>
        /// 得到顶点索引
        /// </summary>
        /// <param name="idxs"></param>
        public void GetVertexIndices(int[] idxs)
        {
            HalfEdge he = he0;
            int i = 0;
            do
            {
                idxs[i++] = he.Head.index;
                he = he.next;
            } while (he != he0);
        }

        /// <summary>
        /// 连接两条边
        /// </summary>
        /// <param name="hedgePrev"></param>
        /// <param name="hedge"></param>
        /// <returns></returns>
        private Face ConnectHalfEdges(HalfEdge hedgePrev, HalfEdge hedge)
        {
            Face discardedFace = null;

            if (hedgePrev.OppositeFace == hedge.OppositeFace)
            {
                //然后去掉一个多余的边缘
                Face oppFace = hedge.OppositeFace;
                HalfEdge hedgeOpp;

                if (hedgePrev == he0)
                {
                    he0 = hedge;
                }

                if (oppFace.NumVertices == 3)
                {
                    //这样就可以完全覆盖了
                    hedgeOpp = hedge.Opposite.prev.Opposite;

                    oppFace.mark = c_Deleted;
                    discardedFace = oppFace;
                }
                else
                {
                    hedgeOpp = hedge.Opposite.next;

                    if (oppFace.he0 == hedgeOpp.prev)
                    {
                        oppFace.he0 = hedgeOpp;
                    }

                    hedgeOpp.prev = hedgeOpp.prev.prev;
                    hedgeOpp.prev.next = hedgeOpp;
                }

                hedge.prev = hedgePrev.prev;
                hedge.prev.next = hedge;

                hedge.Opposite = (hedgeOpp);

                //面被修改了 所以需要重新计算
                oppFace.ComputeNormalAndCentroid();
            }
            else
            {
                hedgePrev.next = hedge;
                hedge.prev = hedgePrev;
            }

            return discardedFace;
        }


        /// <summary>
        /// 检查是否是一个平面
        /// </summary>
        private void CheckConsistency()
        {
            //在面上做检查
            HalfEdge hedge = he0;
            float maxd = 0;
            int numv = 0;

            if (numVerts < 3)
            {
                throw new Exception("degenerate face: " + ToString());
            }

            do
            {
                HalfEdge hedgeOpp = hedge.Opposite;
                if (hedgeOpp == null)
                {
                    throw new Exception("face " + ToString() + ": " +
                                        "unreflected half edge " + hedge.ToString());
                }
                else if (hedgeOpp.Opposite != hedge)
                {
                    throw new Exception(
                        "face " + ToString() + ": " +
                        "opposite half edge " + hedgeOpp.ToString() +
                        " has opposite " +
                        hedgeOpp.Opposite.ToString());
                }

                if (hedgeOpp.Head != hedge.Tail ||
                    hedge.Head != hedgeOpp.Tail)
                {
                    throw new Exception(
                        "face " + ToString() + ": " +
                        "half edge " + hedge.ToString() +
                        " reflected by " + hedgeOpp.ToString());
                }

                Face oppFace = hedgeOpp.face;
                if (oppFace == null)
                {
                    throw new Exception(
                        "face " + ToString() + ": " +
                        "no face on half edge " + hedgeOpp.ToString());
                }
                else if (oppFace.mark == c_Deleted)
                {
                    throw new Exception(
                        "face " + ToString() + ": " +
                        "opposite face " + oppFace.ToString() +
                        " not on hull");
                }

                float d = Mathf.Abs(DistanceToPlane(hedge.Head.pnt));
                if (d > maxd)
                {
                    maxd = d;
                }

                numv++;
                hedge = hedge.next;
            } while (hedge != he0);

            if (numv != numVerts)
            {
                throw new Exception(
                    "face " + ToString() + " numVerts=" + numVerts + " should be " + numv);
            }
        }

        /// <summary>
        /// 合并相邻的面
        /// </summary>
        /// <param name="hedgeAdj"></param>
        /// <param name="discarded"></param>
        /// <returns></returns>
        public int MergeAdjacentFace(HalfEdge hedgeAdj, Face[] discarded)
        {
            Face oppFace = hedgeAdj.OppositeFace;
            int numDiscarded = 0;

            discarded[numDiscarded++] = oppFace;
            oppFace.mark = c_Deleted;

            HalfEdge hedgeOpp = hedgeAdj.Opposite;

            HalfEdge hedgeAdjPrev = hedgeAdj.prev;
            HalfEdge hedgeAdjNext = hedgeAdj.next;
            HalfEdge hedgeOppPrev = hedgeOpp.prev;
            HalfEdge hedgeOppNext = hedgeOpp.next;

            while (hedgeAdjPrev.OppositeFace == oppFace)
            {
                hedgeAdjPrev = hedgeAdjPrev.prev;
                hedgeOppNext = hedgeOppNext.next;
            }

            while (hedgeAdjNext.OppositeFace == oppFace)
            {
                hedgeOppPrev = hedgeOppPrev.prev;
                hedgeAdjNext = hedgeAdjNext.next;
            }

            HalfEdge hedge;

            for (hedge = hedgeOppNext; hedge != hedgeOppPrev.next; hedge = hedge.next)
            {
                hedge.face = this;
            }

            if (hedgeAdj == he0)
            {
                he0 = hedgeAdjNext;
            }

            //处理边的头节点
            Face discardedFace;

            discardedFace = ConnectHalfEdges(hedgeOppPrev, hedgeAdjNext);
            if (discardedFace != null)
            {
                discarded[numDiscarded++] = discardedFace;
            }

            //处理边的尾节点
            discardedFace = ConnectHalfEdges(hedgeAdjPrev, hedgeOppNext);
            if (discardedFace != null)
            {
                discarded[numDiscarded++] = discardedFace;
            }

            ComputeNormalAndCentroid();
            CheckConsistency();

            return numDiscarded;
        }

        /// <summary>
        /// 通过两条边  计算平方面积
        /// </summary>
        /// <param name="hedge0"></param>
        /// <param name="hedge1"></param>
        /// <returns></returns>
        private float AreaSquared(HalfEdge hedge0, HalfEdge hedge1)
        {
            //返回两条边产生的面积
            Vector3 p0 = hedge0.Tail.pnt;
            Vector3 p1 = hedge0.Head.pnt;
            Vector3 p2 = hedge1.Head.pnt;

            Vector3 d1 = p1 - p0;
            Vector3 d2 = p2 - p0;


            return Vector3.Cross(d1, d2).sqrMagnitude;
        }

        /// <summary>
        /// 三角化
        /// </summary>
        /// <param name="newFaces"></param>
        /// <param name="minArea"></param>
        public void Triangulate(FaceList newFaces, float minArea)
        {
            HalfEdge hedge;

            if (NumVertices < 4)
            {
                return;
            }

            Vertex v0 = he0.Head;
            Face prevFace = null;

            hedge = he0.next;
            HalfEdge oppPrev = hedge.opposite;
            Face face0 = null;

            for (hedge = hedge.next; hedge != he0.prev; hedge = hedge.next)
            {
                Face face =
                    CreateTriangle(v0, hedge.prev.Head, hedge.Head, minArea);
                face.he0.next.Opposite = (oppPrev);
                face.he0.prev.Opposite = (hedge.opposite);
                oppPrev = face.he0;
                newFaces.Add(face);
                if (face0 == null)
                {
                    face0 = face;
                }
            }

            hedge = new HalfEdge(he0.prev.prev.Head, this);
            hedge.Opposite = (oppPrev);

            hedge.prev = he0;
            hedge.prev.next = hedge;

            hedge.next = he0.prev;
            hedge.next.prev = hedge;

            ComputeNormalAndCentroid(minArea);
            CheckConsistency();

            for (Face face = face0; face != null; face = face.next)
            {
                face.CheckConsistency();
            }
        }
    }
}
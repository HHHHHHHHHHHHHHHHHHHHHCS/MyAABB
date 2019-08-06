using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 三角面
/// </summary>
public class Face
{
    /// <summary>
    /// 状态:可见的
    /// </summary>
    public const int c_visible = 1;

    /// <summary>
    /// 状态:不是凸出的形状
    /// </summary>
    public const int c_noneConvex = 2;

    /// <summary>
    /// 状态:删除的
    /// </summary>
    public const int c_deleted = 3;

    /// <summary>
    /// 第0条边
    /// </summary>
    private HalfEdge he0;

    /// <summary>
    /// 法线
    /// </summary>
    private Vector3 normal;

    /// <summary>
    /// 面积
    /// </summary>
    private float area;

    /// <summary>
    /// 中心点
    /// </summary>
    private Vector3 centroid;

    /// <summary>
    /// 面板偏移
    /// </summary>
    private float planeOffset;

    /// <summary>
    /// face的Index
    /// </summary>
    private int index;

    /// <summary>
    /// 顶点数量
    /// </summary>
    private int numVerts;

    /// <summary>
    /// 下一个三角面 链表用
    /// </summary>
    public Face next;

    /// <summary>
    /// 当前的状态
    /// </summary>
    private int mark = c_visible;

    /// <summary>
    /// 平面上一个特别的点
    /// </summary>
    public Vertex outside;

    /// <summary>
    /// 第一条边
    /// </summary>
    public HalfEdge HE0 => he0;

    /// <summary>
    /// 当前面片的状态
    /// </summary>
    public int Mark => mark;

    public void ComputeCentroid()
    {
        centroid = Vector3.zero;
        var he = he0;
        do
        {
            centroid += he.Head;
            he = he.Next;
        } while (he != he0);

        centroid /= numVerts;
    }


    public void ComputeNormal(float minArea)
    {
        ComputeNormal();

        if (area < minArea)
        {
            //用来处理四边形以上的时候,不再一个平面的法线
            //通过删除最长的边 来让法线更准确

            HalfEdge hedgeMax = null;
            float lenSqrMax = 0;
            var hedge = he0;
            do
            {
                //找出最长的边
                var lenSqr = hedge.LengthSquared();
                if (lenSqr > lenSqrMax) lenSqrMax = lenSqr;

                hedge = hedge.Next;
            } while (hedge != he0);

            var p2 = hedgeMax.Head.pnt;
            var p1 = hedgeMax.Tail.pnt;

            var lenMax = Mathf.Sqrt(lenSqrMax);

            var p21 = (p2 - p1) / lenMax;

            var dot = Vector3.Dot(normal, p21);

            normal -= dot * p21;

            normal.Normalize();
        }
    }


    public void ComputeNormal()
    {
        var he1 = he0.Next;
        var he2 = he1.Next;

        var p0 = he0.Head.pnt;
        var d2 = he1.Head.pnt - p0;

        normal = Vector3.zero;
        numVerts = 2;

        while (he2 != he0)
        {
            var oD2 = d2;

            d2 = he2.Head.pnt - p0;

            normal += Vector3.Cross(oD2, d2);

            he2 = he2.Next;
            numVerts++;
        }

        area = normal.magnitude; //叉积的绝对值的一半 是面积
        normal.Normalize();
    }

    private void ComputeNormalAndCentroid()
    {
        ComputeNormal();
        ComputeCentroid();
        planeOffset = Vector3.Dot(normal, centroid);
        var numv = 0;
        var he = he0;
        do
        {
            numv++;
            he = he.Next;
        } while (he != he0);

        if (numv != numVerts)
            throw new Exception("face:" + GetVertexString() + " numVerts=" + numVerts + " should be " + numv);
    }

    private void ComputeNormalAndCentroid(float minArea)
    {
        ComputeNormal(minArea);
        ComputeCentroid();
        planeOffset = Vector3.Dot(normal, centroid);
    }

    public static Face CreateTriangle(Vertex v0, Vertex v1, Vertex v2)
    {
        return CreateTriangle(v0, v1, v2, 0);
    }

    /// <summary>
    /// 创建一个三角面
    /// </summary>
    /// <param name="v0"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="minArea"></param>
    public static Face CreateTriangle(Vertex v0, Vertex v1, Vertex v2, float minArea)
    {
        var face = new Face();
        var he0 = new HalfEdge(v0, face);
        var he1 = new HalfEdge(v1, face);
        var he2 = new HalfEdge(v2, face);

        he0.Prev = he2;
        he0.Next = he1;
        he1.Prev = he1;
        he1.Next = he2;
        he2.Prev = he1;
        he2.Next = he0;

        face.he0 = he0;

        face.ComputeNormalAndCentroid(minArea);

        return face;
    }

    public static Face Create(Vertex[] vtxArray, int[] indices)
    {
        Face face = new Face();
        HalfEdge hePrev = null;
        for (int i = 0; i < indices.Length; i++)
        {
            HalfEdge he = new HalfEdge(vtxArray[indices[i]], face);
            if (hePrev != null)
            {
                he.Prev = hePrev;
                hePrev.Next = he;
            }
            else
            {
                face.he0 = he;
            }

            hePrev = he;
        }

        face.he0.Prev = hePrev;
        face.ComputeNormalAndCentroid();
        return face;
    }

    public Face()
    {
        normal = Vector3.zero;
        centroid = Vector3.zero;
        mark = c_visible;
    }

    /// <summary>
    /// 得到指定Index的边
    /// i>0 next 查找   i<0 prev 查找
    /// i限制在[0,2]
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public HalfEdge GetEdge(int i)
    {
        HalfEdge he = he0;
        while (i > 0)
        {
            he = he.Next;
            i--;
        }

        while (i < 0)
        {
            he = he.Prev;
            i++;
        }

        return he;
    }

    /// <summary>
    /// 取到第一条边
    /// </summary>
    public HalfEdge FirstEdge => he0;

    /// <summary>
    /// 根据头尾两个点 找到边
    /// </summary>
    /// <param name="vt"></param>
    /// <param name="vh"></param>
    /// <returns></returns>
    public HalfEdge FindEdge(Vertex vt, Vertex vh)
    {
        HalfEdge he = he0;
        do
        {
            if (he.Head == vh && he.Tail == vt)
            {
                return he;
            }

            he = he.Next;
        } while (he != he0);

        return null;
    }

    /// <summary>
    /// 点到平面的距离
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public float DistanceToPlane(Vector3 p)
    {
        return normal.x * p.x + normal.y * p.y + normal.z * p.z - planeOffset;
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
    /// 顶点数量
    /// </summary>
    public int NumVertices => numVerts;

    /// <summary>
    /// 得到顶点的String
    /// </summary>
    /// <returns></returns>
    private string GetVertexString()
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

            he = he.Next;
        } while (he != he0);

        return s;
    }

    public int[] GetVertexIndices()
    {
        int[] idxs = new int[numVerts];
        HalfEdge he = he0;
        int i = 0;
        do
        {
            idxs[i++] = he.Head.index;
            he = he.Next;
        } while (he != he0);

        return idxs;
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
            Face oppFace = hedge.OppositeFace;
            HalfEdge hedgeOpp = null;

            if (hedgePrev == he0)
            {
                he0 = hedge;
            }

            if (oppFace.numVerts == 3)
            {
                //这样就可以完全覆盖了
                hedgeOpp = hedge.Opposite.Prev.Opposite;

                oppFace.mark = c_deleted;
                discardedFace = oppFace;
            }
            else
            {
                hedge = hedge.Opposite.Next;

                if (oppFace.he0 == hedgeOpp.Prev)
                {
                    oppFace.he0 = hedgeOpp;
                }

                hedgeOpp.Prev = hedgeOpp.Prev.Prev;
                hedge.Prev.Next = hedge;

                hedge.Opposite = hedgeOpp;
                hedgeOpp.Opposite = hedge;

                //面被修改了 所以需要重新计算
                oppFace.ComputeNormalAndCentroid();
            }
        }
        else
        {
            hedgePrev.Next = hedge;
            hedge.Prev = hedgePrev;
        }

        return discardedFace;
    }

    /// <summary>
    /// 检查是否是一个平面
    /// </summary>
    private void CheckConsistency()
    {
        HalfEdge hedge = he0;
        float maxd = 0;
        int numv = 0;

        if (numVerts < 3)
        {
            throw new Exception("degenerate face:" + GetVertexString());
        }

        do
        {
            HalfEdge hedgeOpp = hedge.Opposite;
            if (hedgeOpp == null)
            {
                throw new Exception(
                    "face " + GetVertexString() + ": " +
                    "unreflected half edge " + hedge.GetVertexString());
            }
            else if (hedgeOpp.Opposite != hedge)
            {
                throw new Exception(
                    "face " + GetVertexString() + ": " +
                    "opposite half edge " + hedgeOpp.GetVertexString() +
                    " has opposite " +
                    hedgeOpp.Opposite.GetVertexString());
            }

            if (hedgeOpp.Head != hedge.Tail ||
                hedge.Head != hedgeOpp.Tail)
            {
                throw new Exception(
                    "face " + GetVertexString() + ": " +
                    "half edge " + hedge.GetVertexString() +
                    " reflected by " + hedgeOpp.GetVertexString());
            }

            Face oppFace = hedgeOpp.Face;
            if (oppFace == null)
            {
                throw new Exception(
                    "face " + GetVertexString() + ": " +
                    "no face on half edge " + hedgeOpp.GetVertexString());
            }
            else if (oppFace.mark == c_deleted)
            {
                throw new Exception(
                    "face " + GetVertexString() + ": " +
                    "opposite face " + oppFace.GetVertexString() +
                    " not on hull");
            }

            float d = Mathf.Abs(DistanceToPlane(hedge.Head.pnt));
            if (d > maxd)
            {
                maxd = d;
            }

            numv++;
            hedge = hedge.Next;
        } while (hedge != he0);

        if (numv != numVerts)
        {
            throw new Exception(
                "face " + GetVertexString() + " numVerts=" + numVerts + " should be " + numv);
        }
    }

    /// <summary>
    /// 合并相邻的面
    /// </summary>
    /// <param name="hedgeAdj"></param>
    /// <param name="discarded"></param>
    /// <returns></returns>
    public int mergeAdjacentFace(HalfEdge hedgeAdj, Face[] discarded)
    {
        Face oppFace = hedgeAdj.OppositeFace;
        int numDiscarded = 0;

        discarded[numDiscarded++] = oppFace;
        oppFace.mark = c_deleted;

        HalfEdge hedgeOpp = hedgeAdj.Opposite;

        HalfEdge hedgeAdjPrev = hedgeAdj.Prev;
        HalfEdge hedgeAdjNext = hedgeAdj.Next;
        HalfEdge hedgeOppPrev = hedgeOpp.Prev;
        HalfEdge hedgeOppNext = hedgeOpp.Next;

        while (hedgeAdjPrev.OppositeFace == oppFace)
        {
            hedgeAdjPrev = hedgeAdjPrev.Prev;
            hedgeOppNext = hedgeOppNext.Next;
        }

        while (hedgeAdjNext.OppositeFace == oppFace)
        {
            hedgeOppPrev = hedgeOppPrev.Prev;
            hedgeAdjNext = hedgeAdjNext.Next;
        }

        HalfEdge hedge;

        for (hedge = hedgeOppNext; hedge != hedgeOppPrev.Next; hedge = hedge.Next)
        {
            hedge.Face = this;
        }

        if (hedgeAdj == he0)
        {
            he0 = hedgeAdjNext;
        }


        Face discardedFace;

        //处理边的头节点
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
    /// 通过两条边 计算平方面积
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


        Vector3 dx = Vector3.Cross(d1, d2);

        return Vector3.Dot(dx, dx);
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

        hedge = he0.Next;
        HalfEdge oppPrev = hedge.Opposite;
        Face face0 = null;

        for (hedge = hedge.Next; hedge != he0.Prev; hedge = hedge.Next)
        {
            Face face = CreateTriangle(v0, hedge.Prev.Head, hedge.Head, minArea);
            face.he0.Next.Opposite = oppPrev;
            face.he0.Prev.Opposite = hedge.Opposite;
            oppPrev = face.he0;
            newFaces.Add(face);
            if (face0 == null)
            {
                face0 = face;
            }
        }

        hedge = new HalfEdge(he0.Prev.Prev.Head, this);
        hedge.Opposite = oppPrev;

        hedge.Prev = he0;
        hedge.Prev.Next = hedge;

        hedge.Next = he0.Prev;
        hedge.Next.Prev = hedge;

        ComputeNormalAndCentroid(minArea);
        CheckConsistency();

        for (Face face = face0; face != null; face = face.next)
        {
            face.CheckConsistency();
        }
    }
}
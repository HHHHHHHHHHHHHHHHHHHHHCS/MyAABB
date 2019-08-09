namespace QHull
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UnityEngine;

    public class QuickHull3D
    {
        /// <summary>
        /// 按照顺时针输出顶点  正反面用
        /// </summary>
        public const int c_ClockWise = 0x1;

        /// <summary>
        /// 在输出的时候面的顶点是从1开始的
        /// </summary>
        public const int c_IndexFromOne = 0x2;

        /// <summary>
        /// 在输出的时候面的顶点是从0开始的
        /// </summary>
        public const int c_IndexFromZero = 0x4;

        /// <summary>
        /// 在输出的时候面的顶点是相对于输入顶点的编号
        /// </summary>
        public const int c_PointRelative = 0x8;

        /// <summary>
        /// 根据输入点数据自动计算距离公差
        /// </summary>
        public const float c_AutomaticTolerance = -1;

        /// <summary>
        /// 要被查找的index
        /// </summary>
        protected int findIndex = -1;

        /// <summary>
        /// 三视图AABB最长的那条
        /// </summary>
        protected float charLength;


        protected Vertex[] pointBuffer = new Vertex[0];
        protected int[] vertexPointIndices = new int[0];
        private Face[] discardedFaces = new Face[3];

        private Vertex[] maxVtxs = new Vertex[3];
        private Vertex[] minVtxs = new Vertex[3];

        protected List<Face> faces = new List<Face>(16);

        protected List<HalfEdge> horizon = new List<HalfEdge>(16);

        private FaceList newFaces = new FaceList();
        private VertexList unclaimed = new VertexList();
        private VertexList claimed = new VertexList();

        protected int numVertices;
        protected int numFaces;
        protected int numPoints;

        protected float explicitTolerance = c_AutomaticTolerance;
        protected float tolerance;

        /// <summary>
        /// 是否开启debug
        /// </summary>
        public bool IsDebug { get; set; } = false;

        /// <summary>
        /// 间隔最小浮点数
        /// </summary>
        private const float c_FloatPrec = float.Epsilon;


        /// <summary>
        /// 得到距离公差,用于判断哪里凸起
        /// </summary>
        public float DistanceTolerance => tolerance;

        /// <summary>
        /// 自动从点,计算显式距离公差
        /// </summary>
        public float ExplicitDistanceTolerance
        {
            get => explicitTolerance;
            set => explicitTolerance = value;
        }

        private bool isShow = true;

        /// <summary>
        /// 把点加到面里面
        /// </summary>
        /// <param name="vtx"></param>
        /// <param name="face"></param>
        private void AddPointToFace(Vertex vtx, Face face)
        {
            vtx.face = face;

            if (face.outside == null)
            {
                claimed.Add(vtx);
            }
            else
            {
                claimed.InsertBefore(vtx, face.outside);
            }

            face.outside = vtx;
        }

        /// <summary>
        /// 移除面中的点
        /// </summary>
        /// <param name="vtx"></param>
        /// <param name="face"></param>
        private void RemovePointFromFace(Vertex vtx, Face face)
        {
            if (vtx == face.outside)
            {
                if (vtx.next != null && vtx.next.face == face)
                {
                    face.outside = vtx.next;
                }
                else
                {
                    face.outside = null;
                }
            }

            claimed.Delete(vtx);
        }

        /// <summary>
        /// 把面中的点全部移除
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        private Vertex RemoveAllPointsFromFace(Face face)
        {
            if (face.outside != null)
            {
                Vertex end = face.outside;
                while (end.next != null && end.next.face == face)
                {
                    end = end.next;
                }

                claimed.Delete(face.outside, end);
                end.next = null;
                return face.outside;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 空的构造函数
        /// </summary>
        public QuickHull3D()
        {
        }

        /// <summary>
        /// 用float[]->xyz进行构造
        /// </summary>
        /// <param name="coords"></param>
        public QuickHull3D(float[] coords)
        {
            Build(coords, coords.Length / 3);
        }

        /// <summary>
        /// 用位置点进行构造
        /// </summary>
        /// <param name="points"></param>
        public QuickHull3D(Vector3[] points)
        {
            Build(points, points.Length);
        }

        /// <summary>
        /// 根据输入的头尾点暴力查找边
        /// 正常走SetHull流程,所以不常用
        /// </summary>
        /// <param name="tail"></param>
        /// <param name="head"></param>
        /// <returns></returns>
        private HalfEdge FindHalfEdge(Vertex tail, Vertex head)
        {
            foreach (var face in faces)
            {
                HalfEdge he = face.findEdge(tail, head);
                if (he != null)
                {
                    return he;
                }
            }

            return null;
        }

        /// <summary>
        /// 设置凸包
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="nump"></param>
        /// <param name="faceIndices"></param>
        /// <param name="numf"></param>
        protected void SetHull(float[] coords, int nump, int[][] faceIndices, int numf)
        {
            InitBuffers(nump);
            SetPoints(coords, nump);
            ComputeMaxAndMin();
            for (int i = 0; i < numf; i++)
            {
                Face face = Face.Create(pointBuffer, faceIndices[i]);
                HalfEdge he = face.he0;
                do
                {
                    HalfEdge heOpp = FindHalfEdge(he.Head, he.Tail);
                    if (heOpp != null)
                    {
                        he.Opposite = heOpp;
                    }

                    he = he.next;
                } while (he != face.he0);

                faces.Add(face);
            }
        }

        /// <summary>
        /// 根据输入的float[]->转换为xyz点生成凸包
        /// </summary>
        /// <param name="coords"></param>
        public void Build(float[] coords)
        {
            Build(coords, coords.Length / 3);
        }

        /// <summary>
        /// 根据输入的float[]->转换为xyz点生成凸包
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="nump"></param>
        public void Build(float[] coords, int nump)

        {
            if (nump < 4)
            {
                throw new Exception("Less than four input points specified");
            }

            if (coords.Length / 3 < nump)
            {
                throw new Exception("Coordinate array too small for specified number of points");
            }

            InitBuffers(nump);
            SetPoints(coords, nump);
            BuildHull();
        }


        /// <summary>
        /// 根据输入的点生成凸包
        /// </summary>
        /// <param name="points"></param>
        public void Build(Vector3[] points)
        {
            Build(points, points.Length);
        }


        /// <summary>
        /// 根据输入的点生成凸包
        /// </summary>
        /// <param name="points"></param>
        /// <param name="nump"></param>
        public void Build(Vector3[] points, int nump)
        {
            if (nump < 4)
            {
                throw new Exception("Less than four input points specified");
            }

            if (points.Length < nump)
            {
                throw new Exception("Point array too small for specified number of points");
            }

            InitBuffers(nump);
            SetPoints(points, nump);
            BuildHull();
        }

        /// <summary>
        /// 三角化
        /// </summary>
        public void Triangulate()
        {
            float minArea = 1000 * charLength * c_FloatPrec;
            newFaces.Clear();

            foreach (var face in faces)
            {
                if (face.mark == Face.c_Visible)
                {
                    face.Triangulate(newFaces, minArea);
                }
            }

            for (Face face = newFaces.First(); face != null; face = face.next)
            {
                faces.Add(face);
            }
        }

        /// <summary>
        /// 初始化空数据
        /// </summary>
        /// <param name="nump"></param>
        protected void InitBuffers(int nump)
        {
            if (pointBuffer.Length < nump)
            {
                Vertex[] newBuffer = new Vertex[nump];
                vertexPointIndices = new int[nump];
                for (int i = 0; i < pointBuffer.Length; i++)
                {
                    newBuffer[i] = pointBuffer[i];
                }

                for (int i = pointBuffer.Length; i < nump; i++)
                {
                    newBuffer[i] = new Vertex();
                }

                pointBuffer = newBuffer;
            }

            faces.Clear();
            claimed.Clear();
            numFaces = 0;
            numPoints = nump;
        }

        /// <summary>
        /// 把点数据设置进去
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="nump"></param>
        protected void SetPoints(float[] coords, int nump)
        {
            for (int i = 0; i < nump; i++)
            {
                Vertex vtx = pointBuffer[i];
                vtx.pnt.x = coords[i * 3 + 0];
                vtx.pnt.y = coords[i * 3 + 1];
                vtx.pnt.z = coords[i * 3 + 2];
                vtx.index = i;
            }
        }

        /// <summary>
        /// 把点设置进去
        /// </summary>
        /// <param name="pnts"></param>
        /// <param name="nump"></param>
        protected void SetPoints(Vector3[] pnts, int nump)
        {
            for (int i = 0; i < nump; i++)
            {
                Vertex vtx = pointBuffer[i];
                vtx.pnt.x = pnts[i].x;
                vtx.pnt.y = pnts[i].y;
                vtx.pnt.z = pnts[i].z;
                vtx.index = i;
            }
        }

        /// <summary>
        /// 计算AABB最大最小值 得出最大的边
        /// </summary>
        protected void ComputeMaxAndMin()
        {
            Vector3 max = Vector3.zero;
            Vector3 min = Vector3.zero;
            for (int i = 0; i < 3; i++)
            {
                maxVtxs[i] = minVtxs[i] = pointBuffer[0];
            }

            max = (pointBuffer[0].pnt);
            min = (pointBuffer[0].pnt);
            for (int i = 1; i < numPoints; i++)
            {
                Vector3 pnt = pointBuffer[i].pnt;
                if (pnt.x > max.x)
                {
                    max.x = pnt.x;
                    maxVtxs[0] = pointBuffer[i];
                }
                else if (pnt.x < min.x)
                {
                    min.x = pnt.x;
                    minVtxs[0] = pointBuffer[i];
                }

                if (pnt.y > max.y)
                {
                    max.y = pnt.y;
                    maxVtxs[1] = pointBuffer[i];
                }
                else if (pnt.y < min.y)
                {
                    min.y = pnt.y;
                    minVtxs[1] = pointBuffer[i];
                }

                if (pnt.z > max.z)
                {
                    max.z = pnt.z;
                    maxVtxs[2] = pointBuffer[i];
                }
                else if (pnt.z < min.z)
                {
                    min.z = pnt.z;
                    minVtxs[2] = pointBuffer[i];
                }
            }


            charLength = Mathf.Max(max.x - min.x, max.y - min.y, max.z - min.z);
            if (explicitTolerance == c_AutomaticTolerance)
            {
                tolerance =
                    3 * c_FloatPrec * (Mathf.Max(Mathf.Abs(max.x), Mathf.Abs(min.x)) +
                                       Mathf.Max(Mathf.Abs(max.y), Mathf.Abs(min.y)) +
                                       Mathf.Max(Mathf.Abs(max.z), Mathf.Abs(min.z)));
            }
            else
            {
                tolerance = explicitTolerance;
            }
        }


        /// <summary>
        /// 构建初始的凸壳
        /// </summary>
        protected void CreateInitialSimplex()
        {
            //找出最大的轴
            float max = 0;
            int imax = 0;
            for (int i = 0; i < 3; i++)
            {
                float diff = maxVtxs[i].pnt[i] - minVtxs[i].pnt[i];
                if (diff > max)
                {
                    max = diff;
                    imax = i;
                }
            }

            if (max <= tolerance)
            {
                throw new Exception("Input points appear to be coincident");
            }

            Vertex[] vtx = new Vertex[4];

            //把前面两个顶点设置为最大维度的顶点
            vtx[0] = maxVtxs[imax];
            vtx[1] = minVtxs[imax];


            //三个顶点的距离为直线最远的距离
            Vector3 u01 = new Vector3();
            Vector3 diff02 = new Vector3();
            Vector3 nrml = new Vector3();
            Vector3 xprod = new Vector3();
            float maxSqr = 0;
            u01 = vtx[1].pnt + vtx[0].pnt;
            u01.Normalize();
            for (int i = 0; i < numPoints; i++)
            {
                diff02 = (pointBuffer[i].pnt + vtx[0].pnt);
                xprod = Vector3.Cross(u01, diff02);
                float lenSqr = xprod.sqrMagnitude;
                // 检测漏的点
                if (lenSqr > maxSqr &&
                    pointBuffer[i] != vtx[0] && pointBuffer[i] != vtx[1])
                {
                    maxSqr = lenSqr;
                    vtx[2] = pointBuffer[i];
                    nrml.x = xprod.x;
                    nrml.y = xprod.y;
                    nrml.z = xprod.z;
                }
            }

            if (Mathf.Sqrt(maxSqr) <= 100 * tolerance)
            {
                throw new Exception("Input points appear to be colinear");
            }

            nrml.Normalize();

            //重新计算nrml以确保它对U01正常,否则在vtx[2]接近u01时可能会出错
            Vector3 res = new Vector3();
            res = Vector3.Dot(nrml, u01) * u01; // 沿u01的nrml的延长
            nrml += res;
            nrml.Normalize();
            float maxDist = 0;
            float d0 = Vector3.Dot(vtx[2].pnt, nrml);
            for (int i = 0; i < numPoints; i++)
            {
                float dist = Mathf.Abs(Vector3.Dot(pointBuffer[i].pnt, nrml) - d0);
                // 检测漏的点
                if (dist > maxDist &&
                    pointBuffer[i] != vtx[0] &&
                    pointBuffer[i] != vtx[1] &&
                    pointBuffer[i] != vtx[2])
                {
                    maxDist = dist;
                    vtx[3] = pointBuffer[i];
                }
            }

            if (Mathf.Abs(maxDist) <= 100 * tolerance)
            {
                throw new Exception("Input points appear to be coplanar");
            }

            if (IsDebug)
            {
                Debug.Log("initial vertices:");
                Debug.Log(vtx[0].index + ": " + vtx[0].pnt);
                Debug.Log(vtx[1].index + ": " + vtx[1].pnt);
                Debug.Log(vtx[2].index + ": " + vtx[2].pnt);
                Debug.Log(vtx[3].index + ": " + vtx[3].pnt);
            }

            Face[] tris = new Face[4];
            if (Vector3.Dot(vtx[3].pnt,nrml) - d0 < 0)
            {
                tris[0] = Face.CreateTriangle(vtx[0], vtx[1], vtx[2]);
                tris[1] = Face.CreateTriangle(vtx[3], vtx[1], vtx[0]);
                tris[2] = Face.CreateTriangle(vtx[3], vtx[2], vtx[1]);
                tris[3] = Face.CreateTriangle(vtx[3], vtx[0], vtx[2]);
                for (int i = 0; i < 3; i++)
                {
                    int k = (i + 1) % 3;
                    tris[i + 1].GetEdge(1).Opposite=tris[k + 1].GetEdge(0);
                    tris[i + 1].GetEdge(2).Opposite=tris[0].GetEdge(k);
                }
            }
            else
            {
                tris[0] = Face.CreateTriangle(vtx[0], vtx[2], vtx[1]);
                tris[1] = Face.CreateTriangle(vtx[3], vtx[0], vtx[1]);
                tris[2] = Face.CreateTriangle(vtx[3], vtx[1], vtx[2]);
                tris[3] = Face.CreateTriangle(vtx[3], vtx[2], vtx[0]);
                for (int i = 0; i < 3; i++)
                {
                    int k = (i + 1) % 3;
                    tris[i + 1].GetEdge(0).Opposite=tris[k + 1].GetEdge(1);
                    tris[i + 1].GetEdge(2).Opposite=tris[0].GetEdge((3 - i) % 3);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                faces.Add(tris[i]);
            }

            for (int i = 0; i < numPoints; i++)
            {
                Vertex v = pointBuffer[i];
                if (v == vtx[0] || v == vtx[1] || v == vtx[2] || v == vtx[3])
                {
                    continue;
                }

                maxDist = tolerance;
                Face maxFace = null;
                for (int k = 0; k < 4; k++)
                {
                    float dist = tris[k].DistanceToPlane(v.pnt);
                    if (dist > maxDist)
                    {
                        maxFace = tris[k];
                        maxDist = dist;
                    }
                }

                if (maxFace != null)
                {
                    AddPointToFace(v, maxFace);
                }
            }
        }


        /// <summary>
        /// 得到顶点的数量
        /// </summary>
        public int NumVertices => numVertices;


        /// <summary>
        /// 得到顶点
        /// </summary>
        /// <returns></returns>
        public Vector3[] GetVertices()
        {
            Vector3[] vtxs = new Vector3[numVertices];
            for (int i = 0; i < numVertices; i++)
            {
                vtxs[i] = pointBuffer[vertexPointIndices[i]].pnt;
            }

            return vtxs;
        }

        /// <summary>
        /// 得到 x y z 组成的顶点,并且返回顶点长度
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public int GetVertices(float[] coords)
        {
            for (int i = 0; i < numVertices; i++)
            {
                Vector3 pnt = pointBuffer[vertexPointIndices[i]].pnt;
                coords[i * 3 + 0] = pnt.x;
                coords[i * 3 + 1] = pnt.y;
                coords[i * 3 + 2] = pnt.z;
            }

            return numVertices;
        }


        /// <summary>
        /// 得到顶点索引数组
        /// </summary>
        /// <returns></returns>
        public int[] GetVertexPointIndices()
        {
            int[] indices = new int[numVertices];
            for (int i = 0; i < numVertices; i++)
            {
                indices[i] = vertexPointIndices[i];
            }

            return indices;
        }

        /// <summary>
        /// 面片的数量
        /// </summary>
        /// <returns></returns>
        public int NumFaces => faces.Count;

        /// <summary>
        /// 返回与此外壳相光联的面
        /// 每一个面都有一个整数数组表示,该数组给出顶点的索引
        /// 这些索引都是编号 相对于顶点,基于0,逆时针排列
        /// 更多控制在索引格式上 可以使用 GetFaces(int) 和 GetFaces(indexflags)
        ///
        /// 返回整数数组的数组 给出的顶点每个面的索引
        /// </summary>
        /// <returns></returns>
        public int[][] GetFaces()
        {
            return GetFaces(0);
        }

        /// <summary>
        /// 用indexFlags 顺逆时针 得到Faces
        /// </summary>
        /// <param name="indexFlags"></param>
        /// <returns></returns>
        public int[][] GetFaces(int indexFlags)
        {
            int[][] allFaces = new int[faces.Count][];
            int k = 0;
            foreach (var face in faces)
            {
                allFaces[k] = new int[face.NumVertices];
                GetFaceIndices(allFaces[k], face, indexFlags);
                k++;
            }

            return allFaces;
        }

        /// <summary>
        /// 得到面片的索引
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="face"></param>
        /// <param name="flags"></param>
        private void GetFaceIndices(int[] indices, Face face, int flags)
        {
            bool ccw = ((flags & c_ClockWise) == 0);
            bool indexedFromOne = ((flags & c_IndexFromOne) != 0);
            bool pointRelative = ((flags & c_PointRelative) != 0);
            HalfEdge hedge = face.he0;
            int k = 0;
            do
            {
                int idx = hedge.Head.index;
                if (pointRelative)
                {
                    idx = vertexPointIndices[idx];
                }

                if (indexedFromOne)
                {
                    idx++;
                }

                indices[k++] = idx;
                hedge = (ccw ? hedge.next : hedge.prev);
            } while (hedge != face.he0);
        }

        /// <summary>
        /// 处理没有解决(孤儿)的点
        /// </summary>
        /// <param name="newFaces"></param>
        protected void ResolveUnclaimedPoints(FaceList newFaces)
        {
            Vertex vtxNext = unclaimed.First;
            for (Vertex vtx = vtxNext; vtx != null; vtx = vtxNext)
            {
                vtxNext = vtx.next;
                float maxDist = tolerance;
                Face maxFace = null;
                for (Face newFace = newFaces.First();
                    newFace != null;
                    newFace = newFace.next)
                {
                    if (newFace.mark == Face.c_Visible)
                    {
                        float dist = newFace.DistanceToPlane(vtx.pnt);
                        if (dist > maxDist)
                        {
                            maxDist = dist;
                            maxFace = newFace;
                        }

                        if (maxDist > 1000 * tolerance)
                        {
                            break;
                        }
                    }
                }

                if (maxFace != null)
                {
                    AddPointToFace(vtx, maxFace);
                    if (IsDebug && vtx.index == findIndex)
                    {
                        Debug.Log(findIndex + " CLAIMED BY " +
                                  maxFace.GetVertexString());
                    }
                }
                else
                {
                    if (IsDebug && vtx.index == findIndex)
                    {
                        Debug.Log(findIndex + " DISCARDED");
                    }
                }
            }
        }

        /// <summary>
        /// 删除face上包含face的点
        /// </summary>
        /// <param name="face"></param>
        /// <param name="absorbingFace"></param>
        protected void DeleteFacePoints(Face face, Face absorbingFace)
        {
            Vertex faceVtxs = RemoveAllPointsFromFace(face);
            if (faceVtxs != null)
            {
                if (absorbingFace == null)
                {
                    unclaimed.AddRange(faceVtxs);
                }
                else
                {
                    Vertex vtxNext = faceVtxs;
                    for (Vertex vtx = vtxNext; vtx != null; vtx = vtxNext)
                    {
                        vtxNext = vtx.next;
                        float dist = absorbingFace.DistanceToPlane(vtx.pnt);
                        if (dist > tolerance)
                        {
                            AddPointToFace(vtx, absorbingFace);
                        }
                        else
                        {
                            unclaimed.Add(vtx);
                        }
                    }
                }
            }
        }

        private const int c_NoneconvexWrtLargerFace = 1;
        private const int c_NoneConvex = 2;

        /// <summary>
        /// 到对边的距离
        /// </summary>
        /// <param name="he"></param>
        /// <returns></returns>
        protected float OppFaceDistance(HalfEdge he)
        {
            return he.face.DistanceToPlane(he.opposite.face.Centroid);
        }

        /// <summary>
        /// 合并相邻的面片
        /// </summary>
        /// <param name="face"></param>
        /// <param name="mergeType"></param>
        /// <returns></returns>
        private bool DoAdjacentMerge(Face face, int mergeType)
        {
            HalfEdge hedge = face.he0;
            bool convex = true;
            do
            {
                Face oppFace = hedge.OppositeFace;
                bool merge = false;
                float dist1, dist2;
                if (mergeType == c_NoneConvex)
                {
                    //如果是非凸起的面,则进行合并
                    if (OppFaceDistance(hedge) > -tolerance ||
                        OppFaceDistance(hedge.opposite) > -tolerance)
                    {
                        merge = true;
                    }
                }
                else // mergeType == c_noneConvexWrtLargerFace
                {
                    //如果面与较大的面平行或非凸面,则合并面
                    //否则只需将面标记为非凸面,在第二遍进行处理
                    if (face.area > oppFace.area)
                    {
                        if ((dist1 = OppFaceDistance(hedge)) > -tolerance)
                        {
                            merge = true;
                        }
                        else if (OppFaceDistance(hedge.opposite) > -tolerance)
                        {
                            convex = false;
                        }
                    }
                    else
                    {
                        if (OppFaceDistance(hedge.opposite) > -tolerance)
                        {
                            merge = true;
                        }
                        else if (OppFaceDistance(hedge) > -tolerance)
                        {
                            convex = false;
                        }
                    }
                }

                if (merge)
                {
                    if (IsDebug)
                    {
                        Debug.Log("  merging " + face.GetVertexString() + "  and  " +
                                  oppFace.GetVertexString());
                    }

                    int numd = face.MergeAdjacentFace(hedge, discardedFaces);
                    for (int i = 0; i < numd; i++)
                    {
                        DeleteFacePoints(discardedFaces[i], face);
                    }

                    if (IsDebug)
                    {
                        Debug.Log("  result: " + face.GetVertexString());
                    }

                    return true;
                }

                hedge = hedge.next;
            } while (hedge != face.he0);

            if (!convex)
            {
                face.mark = Face.c_NoneConvex;
            }

            return false;
        }

        /// <summary>
        /// 计算地平线
        /// </summary>
        /// <param name="eyePnt"></param>
        /// <param name="edge0"></param>
        /// <param name="face"></param>
        /// <param name="horizon"></param>
        protected void CalculateHorizon(Vector3 eyePnt, HalfEdge edge0, Face face, List<HalfEdge> horizon)
        {
            DeleteFacePoints(face, null);
            face.mark = Face.c_Deleted;
            if (IsDebug)
            {
                Debug.Log("  visiting edge0 " + (edge0 == null ? "null" : edge0.GetVertexString()));
                Debug.Log("  visiting face " + face.GetVertexString());
            }

            HalfEdge edge;
            if (edge0 == null)
            {
                edge0 = face.GetEdge(0);
                edge = edge0;
            }
            else
            {
                edge = edge0.Next;
            }

            if (IsDebug)
            {
                Debug.Log("    edge: " + (edge0 == null ? "null" : edge0.GetVertexString()));
            }

            do
            {
                Face oppFace = edge.OppositeFace;
                if (oppFace.mark == Face.c_Visible)
                {
                    if (oppFace.DistanceToPlane(eyePnt) > tolerance)
                    {
                        CalculateHorizon(eyePnt, edge.Opposite,
                            oppFace, horizon);
                    }
                    else
                    {
                        horizon.Add(edge);
                        if (IsDebug)
                        {
                            Debug.Log("  Adding horizon edge " +
                                      edge.GetVertexString());
                        }
                    }
                }

                edge = edge.Next;
            } while (edge != edge0);
        }

        /// <summary>
        /// 添加相邻的面
        /// </summary>
        /// <param name="eyeVtx"></param>
        /// <param name="he"></param>
        /// <returns></returns>
        private HalfEdge AddAdjoiningFace(Vertex eyeVtx, HalfEdge he)
        {
            Face face = Face.CreateTriangle(
                eyeVtx, he.Tail, he.Head);
            faces.Add(face);
            face.GetEdge(-1).Opposite=he.Opposite;
            return face.GetEdge(0);
        }

        /// <summary>
        /// 添加新的面
        /// </summary>
        /// <param name="newFaces"></param>
        /// <param name="eyeVtx"></param>
        /// <param name="horizon"></param>
        protected void AddNewFaces(
            FaceList newFaces, Vertex eyeVtx, List<HalfEdge> horizon)
        {
            newFaces.Clear();
            HalfEdge hedgeSidePrev = null;
            HalfEdge hedgeSideBegin = null;
            foreach (var horizonHe in horizon)
            {
                HalfEdge hedgeSide = AddAdjoiningFace(eyeVtx, horizonHe);
                if (IsDebug)
                {
                    Debug.Log(
                        "new face: " + hedgeSide.face.GetVertexString());
                }

                if (hedgeSidePrev != null)
                {
                    hedgeSide.next.Opposite=hedgeSidePrev;
                }
                else
                {
                    hedgeSideBegin = hedgeSide;
                }

                newFaces.Add(hedgeSide.Face);
                hedgeSidePrev = hedgeSide;
            }

            hedgeSideBegin.next.Opposite=hedgeSidePrev;
        }

        /// <summary>
        /// 添加下个点
        /// </summary>
        /// <returns></returns>
        protected Vertex NextPointToAdd()
        {
            if (!claimed.IsEmpty)
            {
                Face eyeFace = claimed.First.face;
                Vertex eyeVtx = null;
                float maxDist = 0;
                for (Vertex vtx = eyeFace.outside;
                    vtx != null && vtx.face == eyeFace;
                    vtx = vtx.next)
                {
                    float dist = eyeFace.DistanceToPlane(vtx.pnt);
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        eyeVtx = vtx;
                    }
                }

                return eyeVtx;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 添加点到凸壳里面
        /// </summary>
        /// <param name="eyeVtx"></param>
        protected void AddPointToHull(Vertex eyeVtx)
        {
            horizon.Clear();
            unclaimed.Clear();
            if (IsDebug)
            {
                Debug.Log("Adding point: " + eyeVtx.index);
                Debug.Log(" which is " + eyeVtx.face.DistanceToPlane(eyeVtx.pnt) +
                          " above face " + eyeVtx.face.GetVertexString());
            }

            RemovePointFromFace(eyeVtx, eyeVtx.face);
            CalculateHorizon(eyeVtx.pnt, null, eyeVtx.face, horizon);
            newFaces.Clear();
            AddNewFaces(newFaces, eyeVtx, horizon);


            //第一个合并过程  合并较大的非凸起的面
            for (Face face = newFaces.First(); face != null; face = face.next)
            {
                if (face.mark == Face.c_Visible)
                {
                    while (DoAdjacentMerge(face, c_NoneconvexWrtLargerFace))
                        ;
                }
            }

            //第二个合并过程 合并非凸面的面,与任一面相关
            for (Face face = newFaces.First(); face != null; face = face.next)
            {
                if (face.mark == Face.c_NoneConvex)
                {
                    face.mark = Face.c_Visible;
                    while (DoAdjacentMerge(face, c_NoneConvex))
                        ;
                }
            }

            ResolveUnclaimedPoints(newFaces);
        }

        /// <summary>
        /// 建立凸壳
        /// </summary>
        protected void BuildHull()
        {
            int cnt = 0;
            Vertex eyeVtx;
            ComputeMaxAndMin();
            CreateInitialSimplex();
            while ((eyeVtx = NextPointToAdd()) != null)
            {
                AddPointToHull(eyeVtx);
                cnt++;
                if (IsDebug)
                {
                    Debug.Log("iteration " + cnt + " done");
                }
            }

            ReindexFacesAndVertices();
            if (IsDebug)
            {
                Debug.Log("hull done");
            }
        }

        /// <summary>
        /// 标记face上的顶点
        /// </summary>
        /// <param name="face"></param>
        /// <param name="mark"></param>
        private void MarkFaceVertices(Face face, int mark)
        {
            HalfEdge he0 = face.GetFirstEdge();
            HalfEdge he = he0;
            do
            {
                he.Head.index = mark;
                he = he.next;
            } while (he != he0);
        }

        /// <summary>
        /// 重新建立face和顶戴难道索引
        /// </summary>
        protected void ReindexFacesAndVertices()
        {
            for (int i = 0; i < numPoints; i++)
            {
                pointBuffer[i].index = -1;
            }

            //删除非活动面并标记活动顶点
            numFaces = 0;

            for (int i = faces.Count - 1; i >= 0; i--)
            {
                Face face = faces[i];
                if (face.mark != Face.c_Visible)
                {
                    faces.RemoveAt(i);
                }
                else
                {
                    MarkFaceVertices(face, 0);
                    numFaces++;
                }
            }

            //重新索引顶点
            numVertices = 0;
            for (int i = 0; i < numPoints; i++)
            {
                Vertex vtx = pointBuffer[i];
                if (vtx.index == 0)
                {
                    vertexPointIndices[numVertices] = i;
                    vtx.index = numVertices++;
                }
            }
        }
    }
}
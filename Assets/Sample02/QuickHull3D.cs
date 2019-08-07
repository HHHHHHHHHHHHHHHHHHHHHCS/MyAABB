using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class QuickHull3D
{
    /// <summary>
    /// 按照顺时针输出顶点
    /// </summary>
    public const int c_clockwise = 0x1;

    /// <summary>
    /// 在输出的时候面的顶点是从1开始的
    /// </summary>
    public const int c_indexFromOne = 0x2;

    /// <summary>
    /// 在输出的时候面的顶点是从0开始的
    /// </summary>
    public const int c_indexFromZero = 0x4;

    /// <summary>
    /// 在输出的时候面的顶点是相对于输入顶点的编号
    /// </summary>
    public const int c_pointRelative = 0x8;

    /// <summary>
    /// 根据输入点数据自动计算距离公差
    /// </summary>
    public const float c_automaticTolerance = -1;

    /// <summary>
    /// 间隔最小浮点数
    /// </summary>
    private const float c_floatPrec = float.Epsilon;

    /// <summary>
    /// 要被查找的index
    /// </summary>
    protected int findIndex = -1;

    /// <summary>
    /// 三视图AABB最长的那条
    /// </summary>
    protected float charLength;

    /// <summary>
    /// 设置debug模式
    /// </summary>
    public bool IsDebug { get; set; } = false;

    protected Vertex[] pointBuffer = new Vertex[0];
    protected int[] vertexPointIndices = new int[0];
    private Face[] discardedFaces = new Face[3];

    protected List<Face> faces = new List<Face>(16);
    protected List<HalfEdge> horizon = new List<HalfEdge>(16);


    private Vertex[] maxVtxs = new Vertex[3];
    private Vertex[] minVtxs = new Vertex[3];

    private FaceList newFaces = new FaceList();
    private VertexList unclaimed = new VertexList();
    private VertexList claimed = new VertexList();

    protected int numVertices;
    protected int numFaces;
    protected int numPoints;

    protected float explicitTolerance = c_automaticTolerance;
    protected float tolerance;

    /// <summary>
    /// 得到距离公差,用于判断哪里凸起
    /// </summary>
    public float DistanceTolerance => tolerance;

    /// <summary>
    /// 自动从点,计算显式距离公差
    /// </summary>
    public float ExplicitTolerance
    {
        get => explicitTolerance;
        set => explicitTolerance = value;
    }

    /// <summary>
    /// 给面添加point
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
    /// 移除平面的特定的点
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
    /// 把平面的全部的点都删除
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
    /// 创建一个空的凸点包围盒
    /// </summary>
    public QuickHull3D()
    {
    }

    /// <summary>
    /// 根据输入的float  形成float/3个点  进行计算凸点包围盒
    /// </summary>
    /// <param name="coords"></param>
    public QuickHull3D(float[] coords)
    {
        Build(coords, coords.Length / 3);
    }


    /// <summary>
    /// 根据输入的点,进行生成凸点包围盒
    /// </summary>
    /// <param name="points"></param>
    public QuickHull3D(Vector3[] points)
    {
        Build(points, points.Length);
    }

    /// <summary>
    /// 根据头节点和尾节点 暴力查找边
    /// 正常走SetHull流程,所以不常用
    /// </summary>
    /// <param name="tail"></param>
    /// <param name="head"></param>
    /// <returns></returns>
    private HalfEdge FindHalfEdge(Vertex tail, Vertex head)
    {
        foreach (var item in faces)
        {
            HalfEdge he = item.FindEdge(tail, head);
            return he;
        }

        return null;
    }

    protected void SetHull(float[] coords, int nump, int[][] faceIndices, int numf)
    {
        InitBuffers(nump);
        SetPoints(coords, nump);
        ComputeMaxAndMin();
        for (int i = 0; i < numf; i++)
        {
            Face face = Face.Create(pointBuffer, faceIndices[i]);
            HalfEdge he = face.HE0;
            do
            {
                HalfEdge heOpp = FindHalfEdge(he.Head, he.Tail);
                if (heOpp != null)
                {
                    he.Opposite = heOpp;
                }

                he = he.Next;
            } while (he != face.HE0);

            faces.Add(face);
        }
    }

    private void PrintQhullErrors(Process proc)
    {
        Debug.LogError("Call Error Function->PrintQhullErrors");
        /*
        boolean wrote = false;
        InputStream es = proc.getErrorStream();
        while (es.available() > 0)
        {
            System.out.write(es.read());
            wrote = true;
        }
        if (wrote)
        {
            System.out.println("");
        }
        */
    }

    protected void SetFromQhull(float[] coords, int nump, bool triangulate)
    {
        Debug.LogError("Call Error Function->SetFromQhull");
        /*
        String commandStr = "./qhull i";
        if (Triangulate)
        {
            commandStr += " -Qt";
        }

        try
        {
            Process proc = Runtime.getRuntime().exec(commandStr);
            PrintStream ps = new PrintStream(proc.getOutputStream());
            StreamTokenizer stok =
                new StreamTokenizer(
                    new InputStreamReader(proc.getInputStream()));
            ps.println("3 " + nump);
            for (int i = 0; i < nump; i++)
            {
                ps.println(
                    coords[i * 3 + 0] + " " +
                    coords[i * 3 + 1] + " " +
                    coords[i * 3 + 2]);
            }

            ps.flush();
            ps.close();
            Vector indexList = new Vector(3);
            stok.eolIsSignificant(true);
            printQhullErrors(proc);
            do
            {
                stok.nextToken();
            } while (stok.sval == null ||
                     !stok.sval.startsWith("MERGEexact"));

            for (int i = 0; i < 4; i++)
            {
                stok.nextToken();
            }

            if (stok.ttype != StreamTokenizer.TT_NUMBER)
            {
                System.out.println("Expecting number of faces");
                System.exit(1);
            }

            int numf = (int) stok.nval;
            stok.nextToken(); // clear EOL
            int[][] faceIndices = new int[numf][];
            for (int i = 0; i < numf; i++)
            {
                indexList.clear();
                while (stok.nextToken() != StreamTokenizer.TT_EOL)
                {
                    if (stok.ttype != StreamTokenizer.TT_NUMBER)
                    {
                        System.out.println("Expecting face index");
                        System.exit(1);
                    }

                    indexList.Add(0, new Integer((int) stok.nval));
                }

                faceIndices[i] = new int[indexList.size()];
                int k = 0;
                for (Iterator it = indexList.iterator(); it.hasNext();)
                {
                    faceIndices[i][k++] = ((Integer) it.next()).intValue();
                }
            }

            setHull(coords, nump, faceIndices, numf);
        }
        catch (Exception e)
        {
            e.printStackTrace();
            System.exit(1);
        }
        */
    }

    /// <summary>
    /// 遍历输出全部的点的位置
    /// </summary>
    private void PrintPoints()
    {
        string str = "";
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 pnt = pointBuffer[i].pnt;
            str += i + ":" + pnt + "\n";
        }

        Debug.Log(str);
    }

    /// <summary>
    /// 根据输入的点 转换成 vector3 数组 计算凸包
    /// </summary>
    /// <param name="coords"></param>
    public void Build(float[] coords)
    {
        Build(coords, coords.Length / 3);
    }

    /// <summary>
    /// 根据输入的数据转换为 vector3 nump个数组 计算凸包
    /// nump 需要>=4 而且不在一个平面上
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="nump"></param>
    public void Build(float[] coords, int nump)
    {
        if (nump < 4)
        {
            throw new Exception(
                "Less than four input points specified");
        }

        if (coords.Length / 3 < nump)
        {
            throw new Exception(
                "Coordinate array too small for specified number of points");
        }

        InitBuffers(nump);
        SetPoints(coords, nump);
        BuildHull();
    }

    /// <summary>
    /// 里用一堆Vector3计算凸包
    /// </summary>
    /// <param name="points"></param>
    public void Build(Vector3[] points)
    {
        Build(points, points.Length);
    }

    /// <summary>
    /// 根据顶点 和 要的长度  生成凸包
    /// </summary>
    /// <param name="points"></param>
    /// <param name="nump"></param>
    public void Build(Vector3[] points, int nump)
    {
        if (nump < 4)
        {
            throw new Exception(
                "Less than four input points specified");
        }

        if (points.Length < nump)
        {
            throw new Exception(
                "Point array too small for specified number of points");
        }

        InitBuffers(nump);
        SetPoints(points, nump);
        BuildHull();
    }


    /// <summary>
    /// 计算三角花
    /// </summary>
    public void Triangulate()
    {
        float minArea = 1000 * charLength * c_floatPrec;
        newFaces.Clear();

        foreach (var face in faces)
        {
            if (face.Mark == Face.c_visible)
            {
                face.Triangulate(newFaces, minArea);
            }
        }

        for (Face face = newFaces.First; face != null; face = face.next)
        {
            faces.Add(face);
        }
    }

    /// <summary>
    /// 初始化数据
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
    /// 根据float[] 转换成 Vector3 设置点的数据
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="nump"></param>
    protected void SetPoints(float[] coords, int nump)
    {
        for (int i = 0; i < nump; i++)
        {
            Vertex vtx = pointBuffer[i];
            vtx.pnt = new Vector3(coords[i * 3 + 0], coords[i * 3 + 1], coords[i * 3 + 2]);
            vtx.index = i;
        }
    }

    /// <summary>
    /// 直接用Vector3 设置点的数据
    /// </summary>
    /// <param name="pnts"></param>
    /// <param name="nump"></param>
    protected void SetPoints(Vector3[] pnts, int nump)
    {
        for (int i = 0; i < nump; i++)
        {
            Vertex vtx = pointBuffer[i];
            vtx.pnt = pnts[i];
            vtx.index = i;
        }
    }

    /// <summary>
    /// 计算最大和最小的点  类似于AABB , 同时也会生成面积
    /// </summary>
    protected void ComputeMaxAndMin()
    {
        Vector3 max = Vector3.zero;
        Vector3 min = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            maxVtxs[i] = minVtxs[i] = pointBuffer[0];
        }

        max = pointBuffer[0].pnt;
        min = pointBuffer[0].pnt;
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
        if (explicitTolerance == c_automaticTolerance)
        {
            tolerance = 3 * c_floatPrec * (Mathf.Max(Mathf.Abs(max.x), Mathf.Abs(min.x)) +
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

        //把前两个顶点设置最大维度的顶点
        vtx[0] = maxVtxs[imax];
        vtx[1] = minVtxs[imax];


        //三个顶点的距离为 直线最远的距离
        Vector3 u01 = Vector3.zero;
        Vector3 diff02 = Vector3.zero;
        Vector3 nrml = Vector3.zero;
        Vector3 xprod = Vector3.zero;
        float maxSqr = 0;
        u01 = (vtx[1].pnt - vtx[0].pnt).normalized;
        for (int i = 0; i < numPoints; i++)
        {
            diff02 = pointBuffer[i].pnt - vtx[0].pnt;
            xprod = Vector3.Cross(u01, diff02);
            float lenSqr = xprod.sqrMagnitude;
            if (lenSqr > maxSqr &&
                pointBuffer[i] != vtx[0] && // paranoid
                pointBuffer[i] != vtx[1])
            {
                maxSqr = lenSqr;
                vtx[2] = pointBuffer[i];
                nrml = xprod;
            }
        }

        if (Mathf.Sqrt(maxSqr) <= 100 * tolerance)
        {
            throw new Exception(
                "Input points appear to be colinear");
        }

        nrml.Normalize();


        //重新计算nrml以确保它对U01正常,否则在vtx[2]接近u01时可能会出错
        Vector3 res = Vector3.zero;
        res = Vector3.Dot(nrml, u01) * u01; // 沿u01的nrml的延长
        nrml += (res);
        nrml.Normalize();
        float maxDist = 0;
        float d0 = Vector3.Dot(vtx[2].pnt, nrml);
        for (int i = 0; i < numPoints; i++)
        {
            float dist = Mathf.Abs(Vector3.Dot(pointBuffer[i].pnt, nrml) - d0);
            if (dist > maxDist &&
                pointBuffer[i] != vtx[0] && // paranoid
                pointBuffer[i] != vtx[1] &&
                pointBuffer[i] != vtx[2])
            {
                maxDist = dist;
                vtx[3] = pointBuffer[i];
            }
        }

        if (Mathf.Abs(maxDist) <= 100 * tolerance)
        {
            throw new Exception(
                "Input points appear to be coplanar");
        }

        if (IsDebug)
        {
            string str = "initial vertices:"
                         + $"{vtx[0].index} : {vtx[0].pnt} \n"
                         + $"{vtx[1].index} : {vtx[1].pnt} \n"
                         + $"{vtx[2].index} : {vtx[2].pnt} \n"
                         + $"{vtx[3].index} : {vtx[3].pnt} \n";
            Debug.Log(str);
        }

        Face[] tris = new Face[4];
        //方向长度判断 来生成面
        if (Vector3.Dot(vtx[3].pnt, nrml) - d0 < 0)
        {
            tris[0] = Face.CreateTriangle(vtx[0], vtx[1], vtx[2]);
            tris[1] = Face.CreateTriangle(vtx[3], vtx[1], vtx[0]);
            tris[2] = Face.CreateTriangle(vtx[3], vtx[2], vtx[1]);
            tris[3] = Face.CreateTriangle(vtx[3], vtx[0], vtx[2]);
            for (int i = 0; i < 3; i++)
            {
                int k = (i + 1) % 3;
                tris[i + 1].GetEdge(1).Opposite = (tris[k + 1].GetEdge(0));
                tris[i + 1].GetEdge(2).Opposite = (tris[0].GetEdge(k));
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
                tris[i + 1].GetEdge(0).Opposite = (tris[k + 1].GetEdge(1));
                tris[i + 1].GetEdge(2).Opposite = (tris[0].GetEdge((3 - i) % 3));
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
    /// <returns></returns>
    public int NumVertices => numVertices;

    /// <summary>
    /// 得到顶点
    /// </summary>
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
        coords = new float[numVertices * 3];
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
    public int NumFaces() => faces.Count;


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
    /// 用indexFlags特定索引特征得到Faces
    /// </summary>
    /// <param name="indexFlags"></param>
    /// <returns></returns>
    public int[][] GetFaces(int indexFlags)
    {
        int[][] allFaces = new int[faces.Count][];
        int k = 0;
        for (int i = 0; i < faces.Count; i++)
        {
            Face face = faces[i];
            allFaces[k] = new int[face.NumVertices];
            GetFaceIndices(allFaces[k], face, indexFlags);
        }

        return allFaces;
    }


    /// <summary>
    /// 输出顶点和面片
    /// </summary>
    public void Print()
    {
        Print(0);
    }


    /// <summary>
    /// 根据indexFlags规则输出顶点
    /// </summary>
    /// <param name="indexFlags"></param>
    public void Print(int indexFlags)
    {
        if ((indexFlags & c_indexFromZero) == 0)
        {
            indexFlags |= c_indexFromOne;
        }

        for (int i = 0; i < numVertices; i++)
        {
            Vector3 pnt = pointBuffer[vertexPointIndices[i]].pnt;
            Debug.Log($"v:({pnt.x} , {pnt.y} , {pnt.z})");
        }

        string str = "";

        foreach (var face in faces)
        {
            int[] indices = new int[face.NumVertices];
            GetFaceIndices(indices, face, indexFlags);
            str += "f:";
            for (int k = 0; k < indices.Length; k++)
            {
                str += " " + indices[k];
            }

            str += "\n";
        }

        Debug.Log(str);
    }

    /// <summary>
    /// 得到面片的索引
    /// </summary>
    /// <param name="indices"></param>
    /// <param name="face"></param>
    /// <param name="flags"></param>
    private void GetFaceIndices(int[] indices, Face face, int flags)
    {
        bool ccw = ((flags & c_clockwise) == 0);
        bool indexedFromOne = ((flags & c_indexFromOne) != 0);
        bool pointRelative = ((flags & c_pointRelative) != 0);
        HalfEdge hedge = face.HE0;
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
            hedge = (ccw ? hedge.Next : hedge.Prev);
        } while (hedge != face.HE0);
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
            for (Face newFace = newFaces.First; newFace != null; newFace = newFace.next)
            {
                if (newFace.Mark == Face.c_visible)
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
                    Debug.Log(findIndex + " CLAIMED BY " + maxFace.GetVertexString());
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
    /// 删除face上的点
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
                unclaimed.AddAll(faceVtxs);
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

    /// <summary>
    /// 较大的非凸起面
    /// </summary>
    private const int c_noneConvexWrtLargerFace = 1;

    /// <summary>
    /// 非凸起面,第二次判断用
    /// </summary>
    private const int c_noneConvex = 2;

    /// <summary>
    /// 到对边的距离
    /// </summary>
    /// <param name="he"></param>
    /// <returns></returns>
    protected float OppFaceDistance(HalfEdge he)
    {
        return he.Face.DistanceToPlane(he.Opposite.Face.Centroid);
    }

    /// <summary>
    /// 合并相邻的面片
    /// </summary>
    /// <param name="face"></param>
    /// <param name="mergeType"></param>
    /// <returns></returns>
    private bool DoAdjacentMerge(Face face, int mergeType)
    {
        HalfEdge hedge = face.HE0;
        bool convex = true;
        do
        {
            Face oppFace = hedge.OppositeFace;
            bool merge = false;
            float dist1, dist2;
            if (mergeType == c_noneConvex)
            {
                //如果是非凸起的面,则进行合并
                if (OppFaceDistance(hedge) > -tolerance || OppFaceDistance(hedge.Opposite) > -tolerance)
                {
                    merge = true;
                }
            }
            else // mergeType == c_noneConvexWrtLargerFace
            {
                //如果面与较大的面平行或非凸面,则合并面
                //否则只需将面标记为非凸面,在第二遍进行处理
                if (face.Area > oppFace.Area)
                {
                    if ((dist1 = OppFaceDistance(hedge)) > -tolerance)
                    {
                        merge = true;
                    }
                    else if (OppFaceDistance(hedge.Opposite) > -tolerance)
                    {
                        convex = false;
                    }
                }
                else
                {
                    if (OppFaceDistance(hedge.Opposite) > -tolerance)
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
                    Debug.Log("  merging " + face.GetVertexString() + "  and  " + oppFace.GetVertexString());
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

            hedge = hedge.Next;
        } while (hedge != face.HE0);

        if (!convex)
        {
            face.Mark = Face.c_noneConvex;
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
        face.Mark = Face.c_deleted;
        if (IsDebug)
        {
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

        do
        {
            Face oppFace = edge.OppositeFace;
            if (oppFace.Mark == Face.c_visible)
            {
                if (oppFace.DistanceToPlane(eyePnt) > tolerance)
                {
                    CalculateHorizon(eyePnt, edge.Opposite, oppFace, horizon);
                }
                else
                {
                    horizon.Add(edge);
                    if (IsDebug)
                    {
                        Debug.Log("  adding horizon edge " + edge.GetVertexString());
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
        Face face = Face.CreateTriangle(eyeVtx, he.Tail, he.Head);
        faces.Add(face);
        face.GetEdge(-1).Opposite = he.Opposite;
        return face.GetEdge(0);
    }

    /// <summary>
    /// 添加新的面
    /// </summary>
    /// <param name="newFaces"></param>
    /// <param name="eyeVtx"></param>
    /// <param name="horizon"></param>
    protected void AddNewFaces(FaceList newFaces, Vertex eyeVtx, List<HalfEdge> horizon)
    {
        newFaces.Clear();
        HalfEdge hedgeSidePrev = null;
        HalfEdge hedgeSideBegin = null;

        foreach (var horizonHe in horizon)
        {
            HalfEdge hedgeSide = AddAdjoiningFace(eyeVtx, horizonHe);
            if (IsDebug)
            {
                Debug.Log("new face: " + hedgeSide.Face.GetVertexString());
            }

            if (hedgeSidePrev != null)
            {
                hedgeSide.Next.Opposite = hedgeSidePrev;
            }
            else
            {
                hedgeSideBegin = hedgeSide;
            }

            newFaces.Add(hedgeSide.Face);
            hedgeSidePrev = hedgeSide;
        }

        hedgeSideBegin.Next.Opposite = hedgeSidePrev;
    }

    /// <summary>
    /// 添加下个点
    /// </summary>
    /// <returns></returns>
    protected Vertex NextPointToAdd()
    {
        if (!claimed.IsEmpty())
        {
            Face eyeFace = claimed.First.face;
            Vertex eyeVtx = null;
            float maxDist = 0;
            for (Vertex vtx = eyeFace.outside; vtx != null && vtx.face == eyeFace; vtx = vtx.next)
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

        return null;
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
            Debug.Log("Adding point: " + eyeVtx.index + "\n" +
                      " which is " + eyeVtx.face.DistanceToPlane(eyeVtx.pnt) +
                      " above face " + eyeVtx.face.GetVertexString());
        }

        RemovePointFromFace(eyeVtx, eyeVtx.face);
        CalculateHorizon(eyeVtx.pnt, null, eyeVtx.face, horizon);
        newFaces.Clear();
        AddNewFaces(newFaces, eyeVtx, horizon);

        //第一个合并过程  合并较大的非凸起的面
        for (Face face = newFaces.First; face != null; face = face.next)
        {
            if (face.Mark == Face.c_visible)
            {
                while (DoAdjacentMerge(face, c_noneConvexWrtLargerFace)) ;
            }
        }


        //第二个合并过程 合并非凸面的面,与任一面相关
        for (Face face = newFaces.First; face != null; face = face.next)
        {
            if (face.Mark == Face.c_noneConvex)
            {
                face.Mark = Face.c_visible;
                while (DoAdjacentMerge(face, c_noneConvex)) ;
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
        HalfEdge he0 = face.FirstEdge;
        HalfEdge he = he0;
        do
        {
            he.Head.index = mark;
            he = he.Next;
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
        foreach (var face in faces)
        {
        }

        for (int i = faces.Count - 1; i >= 0; i--)
        {
            Face face = faces[i];
            if (face.Mark != Face.c_visible)
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

    /// <summary>
    /// 检查方格凸起程度
    /// </summary>
    /// <param name="face"></param>
    /// <param name="tol"></param>
    /// <param name="ps"></param>
    /// <returns></returns>
    protected bool CheckFaceConvexity(Face face, float tol)
    {
        float dist;
        HalfEdge he = face.HE0;
        do
        {
            face.CheckConsistency();
            //确保边缘凸起
            dist = OppFaceDistance(he);
            if (dist > tol)
            {
                Debug.Log("Edge " + he.GetVertexString() + " non-convex by " + dist);

                return false;
            }

            dist = OppFaceDistance(he.Opposite);
            if (dist > tol)
            {
                Debug.Log("Opposite edge " + he.Opposite.GetVertexString() + " non-convex by " + dist);

                return false;
            }

            if (he.Next.OppositeFace == he.OppositeFace)
            {
                Debug.Log("Redundant vertex " + he.Head.index + " in face " + face.GetVertexString());

                return false;
            }

            he = he.Next;
        } while (he != face.HE0);

        return true;
    }

    /// <summary>
    /// 检查边缘的凸起度
    /// </summary>
    /// <param name="tol"></param>
    /// <returns></returns>
    protected bool CheckFaces(float tol)
    {
        bool convex = true;
        foreach (var face in faces)
        {
            if (face.Mark == Face.c_visible)
            {
                if (!CheckFaceConvexity(face, tol))
                {
                    convex = false;
                }
            }
        }

        return convex;
    }

    /// <summary>
    /// 使用距离公差检查凸壳的正确性
    /// </summary>
    /// <returns></returns>
    public bool Check()
    {
        return Check(DistanceTolerance);
    }


    /// <summary>
    /// 检查凸壳的正确性,这是通过确保没有一个面是非凸的,也没有点在任何面之外
    /// 这些测试是使用距离公差进行的
    /// 如果任何边都是非凸的,则认为面是非凸的,
    /// 并且如果任一相邻面的中心大于在另一个面的上方,
    /// 同样的如果点与面之间的距离大于10倍
    /// </summary>
    /// <param name="tol"></param>
    /// <returns></returns>
    public bool Check(float tol)
    {
        //检查所有边缘是凸起的并且完全连接
        float dist;
        float pointTol = 10 * tol;
        if (!CheckFaces(tolerance))
        {
            return false;
        }

        //检查点都包含进去了
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 pnt = pointBuffer[i].pnt;
            foreach (var face in faces)
            {
                if (face.Mark == Face.c_visible)
                {
                    dist = face.DistanceToPlane(pnt);
                    if (dist > pointTol)
                    {
                        Debug.Log("Point " + i + " " + dist + " above face " +
                                  face.GetVertexString());

                        return false;
                    }
                }
            }
        }

        return true;
    }
}
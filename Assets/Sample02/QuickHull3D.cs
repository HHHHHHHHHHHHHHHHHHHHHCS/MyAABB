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
    public const int c_Clockwise = 0x1;

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
    /// 间隔最小浮点数
    /// </summary>
    private const float c_FloatPrec = float.Epsilon;

    protected int findIndex = -1;

    /// <summary>
    /// 三视图AABB最长的那条
    /// </summary>
    protected float charLength;

    protected bool isDebug = false;

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

    protected float explicitTolerance = c_AutomaticTolerance;
    protected float tolerance;

    /// <summary>
    /// 设置debug模式
    /// </summary>
    public bool IsDebug
    {
        get => isDebug;
        set => isDebug = value;
    }

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
            claimed.add(vtx);
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
        computeMaxAndMin();
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

                    indexList.add(0, new Integer((int) stok.nval));
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
        buildHull();
    }


    /// <summary>
    /// 计算三角花
    /// </summary>
    public void Triangulate()
    {
        float minArea = 1000 * charLength * c_FloatPrec;
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
    /// 计算最大和最小的点  类似于AABB
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
        if (explicitTolerance == c_AutomaticTolerance)
        {
            tolerance = 3 * float_PREC * (Math.max(Math.abs(max.x), Math.abs(min.x)) +
                                          Math.max(Math.abs(max.y), Math.abs(min.y)) +
                                          Math.max(Math.abs(max.z), Math.abs(min.z)));
        }
        else
        {
            tolerance = explicitTolerance;
        }
    }

/**
 * Creates the initial simplex from which the hull will be built.
 */
    protected void createInitialSimplex()
    throws IllegalArgumentException {
    float max = 0;

    int imax = 0;
        for (int i = 0; i< 3; i++)
    {
        float diff = maxVtxs[i].pnt.get(i) - minVtxs[i].pnt.get(i);
        if (diff > max)
        {
            max = diff;
            imax = i;
        }
    }
    if (max <= tolerance)
{
throw new IllegalArgumentException(
"Input points appear to be coincident");
}
Vertex []
vtx = new Vertex[4];
// set first two vertices to be those with the greatest
// one dimensional separation
vtx[0] = maxVtxs[imax];
vtx[1] = minVtxs[imax];

// set third vertex to be the vertex farthest from
// the line between vtx0 and vtx1
Vector3d u01 = new Vector3d();
Vector3d diff02 = new Vector3d();
Vector3d nrml = new Vector3d();
Vector3d xprod = new Vector3d();
float maxSqr = 0;
u01.sub(vtx[1].pnt, vtx[0].pnt);
u01.normalize();
for (int i = 0; i<numPoints; i++)
{
diff02.sub(pointBuffer[i].pnt, vtx[0].pnt);
xprod.cross(u01, diff02);
float lenSqr = xprod.normSquared();
if (lenSqr > maxSqr &&
pointBuffer[i] != vtx[0] && // paranoid
pointBuffer[i] != vtx[1])
{
maxSqr = lenSqr;
vtx[2] = pointBuffer[i];
nrml.set(xprod);
}
}
if (Math.sqrt(maxSqr) <= 100 * tolerance)
{
throw new IllegalArgumentException(
"Input points appear to be colinear");
}
nrml.normalize();

// recompute nrml to make sure it is normal to u10 - otherwise could
// be errors in case vtx[2] is close to u10
Vector3d res = new Vector3d();
res.scale(nrml.dot(u01), u01); // component of nrml along u01
nrml.sub(res);
nrml.normalize();
float maxDist = 0;
float d0 = vtx[2].pnt.dot(nrml);
for (int i = 0; i<numPoints; i++)
{
float dist = Math.abs(pointBuffer[i].pnt.dot(nrml) - d0);
if (dist > maxDist &&
pointBuffer[i] != vtx[0] && // paranoid
pointBuffer[i] != vtx[1] &&
pointBuffer[i] != vtx[2])
{
maxDist = dist;
vtx[3] = pointBuffer[i];
}
}
if (Math.abs(maxDist) <= 100 * tolerance)
{
throw new IllegalArgumentException(
"Input points appear to be coplanar");
}
if (debug)
{
System.out.println("initial vertices:");
System.out.println(vtx[0].index + ": " + vtx[0].pnt);
System.out.println(vtx[1].index + ": " + vtx[1].pnt);
System.out.println(vtx[2].index + ": " + vtx[2].pnt);
System.out.println(vtx[3].index + ": " + vtx[3].pnt);
}
Face[] tris = new Face[4];
if (vtx[3].pnt.dot(nrml) - d0< 0)
{
tris[0] = Face.createTriangle(vtx[0], vtx[1], vtx[2]);
tris[1] = Face.createTriangle(vtx[3], vtx[1], vtx[0]);
tris[2] = Face.createTriangle(vtx[3], vtx[2], vtx[1]);
tris[3] = Face.createTriangle(vtx[3], vtx[0], vtx[2]);
for (int i = 0; i< 3; i++)
{
int k = (i + 1) % 3;
tris[i + 1].getEdge(1).setOpposite(tris[k + 1].getEdge(0));
tris[i + 1].getEdge(2).setOpposite(tris[0].getEdge(k));
}
} else
{
tris[0] = Face.createTriangle(vtx[0], vtx[2], vtx[1]);
tris[1] = Face.createTriangle(vtx[3], vtx[0], vtx[1]);
tris[2] = Face.createTriangle(vtx[3], vtx[1], vtx[2]);
tris[3] = Face.createTriangle(vtx[3], vtx[2], vtx[0]);
for (int i = 0; i< 3; i++)
{
int k = (i + 1) % 3;
tris[i + 1].getEdge(0).setOpposite(tris[k + 1].getEdge(1));
tris[i + 1].getEdge(2).setOpposite(tris[0].getEdge((3 - i) % 3));
}
}
for (int i = 0; i< 4; i++)
{
faces.add(tris[i]);
}
for (int i = 0; i<numPoints; i++)
{
Vertex v = pointBuffer[i];
if (v == vtx[0] || v == vtx[1] || v == vtx[2] || v == vtx[3])
{
continue;
}
maxDist = tolerance;
Face maxFace = null;
for (int k = 0; k< 4; k++)
{
float dist = tris[k].distanceToPlane(v.pnt);
if (dist > maxDist)
{
maxFace = tris[k];
maxDist = dist;
}
}
if (maxFace != null)
{
addPointToFace(v, maxFace);
}
}
}
/**
 * Returns the number of vertices in this hull.
 *
 * @return number of vertices
 */
public int getNumVertices()
{
return numVertices;
}
/**
 * Returns the vertex points in this hull.
 *
 * @return array of vertex points
 * @see QuickHull3D#getVertices(float[])
 * @see QuickHull3D#getFaces()
 */
public Point3d[] getVertices()
{
Point3d[] vtxs = new Point3d[numVertices];
for (int i = 0; i < numVertices; i++)
{
vtxs[i] = pointBuffer[vertexPointIndices[i]].pnt;
}
return vtxs;
}
/**
 * Returns the coordinates of the vertex points of this hull.
 *
 * @param coords returns the x, y, z coordinates of each vertex.
 *               This length of this array must be at least three times
 *               the number of vertices.
 * @return the number of vertices
 * @see QuickHull3D#getVertices()
 * @see QuickHull3D#getFaces()
 */
public int getVertices(float[] coords)
{
for (int i = 0; i < numVertices; i++)
{
Point3d pnt = pointBuffer[vertexPointIndices[i]].pnt;
coords[i * 3 + 0] = pnt.x;
coords[i * 3 + 1] = pnt.y;
coords[i * 3 + 2] = pnt.z;
}
return numVertices;
}
/**
 * Returns an array specifing the index of each hull vertex
 * with respect to the original input points.
 *
 * @return vertex indices with respect to the original points
 */
public int[] getVertexPointIndices()
{
int[] indices = new int[numVertices];
for (int i = 0; i < numVertices; i++)
{
indices[i] = vertexPointIndices[i];
}
return indices;
}
/**
 * Returns the number of faces in this hull.
 *
 * @return number of faces
 */
public int getNumFaces()
{
return faces.size();
}
/**
 * Returns the faces associated with this hull.
 *
 * <p>Each face is represented by an integer array which gives the
 * indices of the vertices. These indices are numbered
 * relative to the
 * hull vertices, are zero-based,
 * and are arranged counter-clockwise. More control
 * over the index format can be obtained using
 * {@link #getFaces(int) getFaces(indexFlags)}.
 *
 * @return array of integer arrays, giving the vertex
 * indices for each face.
 * @see QuickHull3D#getVertices()
 * @see QuickHull3D#getFaces(int)
 */
public int[][] getFaces()
{
return getFaces(0);
}
/**
 * Returns the faces associated with this hull.
 *
 * <p>Each face is represented by an integer array which gives the
 * indices of the vertices. By default, these indices are numbered with
 * respect to the hull vertices (as opposed to the input points), are
 * zero-based, and are arranged counter-clockwise. However, this
 * can be changed by setting {@link #POINT_RELATIVE
 * POINT_RELATIVE}, {@link #INDEXED_FROM_ONE INDEXED_FROM_ONE}, or
 * {@link #CLOCKWISE CLOCKWISE} in the indexFlags parameter.
 *
 * @param indexFlags specifies index characteristics (0 results
 *                   in the default)
 * @return array of integer arrays, giving the vertex
 * indices for each face.
 * @see QuickHull3D#getVertices()
 */
public int[][] getFaces(int indexFlags)
{
int[][] allFaces = new int[faces.size()][];
int k = 0;
for (Iterator it = faces.iterator(); it.hasNext();)
{
Face face = (Face)it.next();
allFaces[k] = new int[face.numVertices()];
getFaceIndices(allFaces[k], face, indexFlags);
k++;
}
return allFaces;
}
/**
 * Prints the vertices and faces of this hull to the stream ps.
 *
 * <p>
 * This is done using the Alias Wavefront .obj file
 * format, with the vertices printed first (each preceding by
 * the letter <code>v</code>), followed by the vertex indices
 * for each face (each
 * preceded by the letter <code>f</code>).
 *
 * <p>The face indices are numbered with respect to the hull vertices
 * (as opposed to the input points), with a lowest index of 1, and are
 * arranged counter-clockwise. More control over the index format can
 * be obtained using
 * {@link #print(PrintStream, int) print(ps,indexFlags)}.
 *
 * @param ps stream used for printing
 * @see QuickHull3D#print(PrintStream, int)
 * @see QuickHull3D#getVertices()
 * @see QuickHull3D#getFaces()
 */
public void print(PrintStream ps)
{
print(ps, 0);
}
/**
 * Prints the vertices and faces of this hull to the stream ps.
 *
 * <p> This is done using the Alias Wavefront .obj file format, with
 * the vertices printed first (each preceding by the letter
 * <code>v</code>), followed by the vertex indices for each face (each
 * preceded by the letter <code>f</code>).
 *
 * <p>By default, the face indices are numbered with respect to the
 * hull vertices (as opposed to the input points), with a lowest index
 * of 1, and are arranged counter-clockwise. However, this
 * can be changed by setting {@link #POINT_RELATIVE POINT_RELATIVE},
 * {@link #INDEXED_FROM_ONE INDEXED_FROM_ZERO}, or {@link #CLOCKWISE
 * CLOCKWISE} in the indexFlags parameter.
 *
 * @param ps         stream used for printing
 * @param indexFlags specifies index characteristics
 *                   (0 results in the default).
 * @see QuickHull3D#getVertices()
 * @see QuickHull3D#getFaces()
 */
public void print(PrintStream ps, int indexFlags)
{
if ((indexFlags & INDEXED_FROM_ZERO) == 0)
{
indexFlags |= INDEXED_FROM_ONE;
}
for (int i = 0; i < numVertices; i++)
{
Point3d pnt = pointBuffer[vertexPointIndices[i]].pnt;
ps.println("v " + pnt.x + " " + pnt.y + " " + pnt.z);
}
for (Iterator fi = faces.iterator(); fi.hasNext();)
{
Face face = (Face)fi.next();
int[] indices = new int[face.numVertices()];
getFaceIndices(indices, face, indexFlags);
ps.print("f");
for (int k = 0; k < indices.length; k++)
{
ps.print(" " + indices[k]);
}
ps.println("");
}
}
private void getFaceIndices(int[] indices, Face face, int flags)
{
boolean ccw = ((flags & CLOCKWISE) == 0);
boolean indexedFromOne = ((flags & INDEXED_FROM_ONE) != 0);
boolean pointRelative = ((flags & POINT_RELATIVE) != 0);
HalfEdge hedge = face.he0;
int k = 0;
do
{
int idx = hedge.head().index;
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
}
while (hedge != face.he0);
}
protected void resolveUnclaimedPoints(FaceList newFaces)
{
Vertex vtxNext = unclaimed.first();
for (Vertex vtx = vtxNext; vtx != null; vtx = vtxNext)
{
vtxNext = vtx.next;
float maxDist = tolerance;
Face maxFace = null;
for (Face newFace = newFaces.first(); newFace != null;
newFace = newFace.next)
{
if (newFace.mark == Face.VISIBLE)
{
float dist = newFace.distanceToPlane(vtx.pnt);
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
addPointToFace(vtx, maxFace);
if (debug && vtx.index == findIndex)
{
System.out.println(findIndex + " CLAIMED BY " +
maxFace.getVertexString());
}
}
else
{
if (debug && vtx.index == findIndex)
{
System.out.println(findIndex + " DISCARDED");
}
}
}
}
protected void deleteFacePoints(Face face, Face absorbingFace)
{
Vertex faceVtxs = removeAllPointsFromFace(face);
if (faceVtxs != null)
{
if (absorbingFace == null)
{
unclaimed.addAll(faceVtxs);
}
else
{
Vertex vtxNext = faceVtxs;
for (Vertex vtx = vtxNext; vtx != null; vtx = vtxNext)
{
vtxNext = vtx.next;
float dist = absorbingFace.distanceToPlane(vtx.pnt);
if (dist > tolerance)
{
addPointToFace(vtx, absorbingFace);
}
else
{
unclaimed.add(vtx);
}
}
}
}
}
private static final int NONCONVEX_WRT_LARGER_FACE = 1;
private static final int NONCONVEX = 2;
protected float oppFaceDistance(HalfEdge he)
{
return he.face.distanceToPlane(he.opposite.face.getCentroid());
}
private boolean doAdjacentMerge(Face face, int mergeType)
{
HalfEdge hedge = face.he0;
boolean convex = true;
do
{
Face oppFace = hedge.oppositeFace();
boolean merge = false;
float dist1, dist2;
if (mergeType == NONCONVEX)
{
// then merge faces if they are definitively non-convex
if (oppFaceDistance(hedge) > -tolerance ||
oppFaceDistance(hedge.opposite) > -tolerance)
{
merge = true;
}
}
else // mergeType == NONCONVEX_WRT_LARGER_FACE
{
// merge faces if they are parallel or non-convex
// wrt to the larger face; otherwise, just mark
// the face non-convex for the second pass.
if (face.area > oppFace.area)
{
if ((dist1 = oppFaceDistance(hedge)) > -tolerance)
{
merge = true;
}
else if (oppFaceDistance(hedge.opposite) > -tolerance)
{
convex = false;
}
}
else
{
if (oppFaceDistance(hedge.opposite) > -tolerance)
{
merge = true;
}
else if (oppFaceDistance(hedge) > -tolerance)
{
convex = false;
}
}
}
if (merge)
{
if (debug)
{
System.out.println(
"  merging " + face.getVertexString() + "  and  " +
oppFace.getVertexString());
}
int numd = face.mergeAdjacentFace(hedge, discardedFaces);
for (int i = 0; i < numd; i++)
{
deleteFacePoints(discardedFaces[i], face);
}
if (debug)
{
System.out.println(
"  result: " + face.getVertexString());
}
return true;
}
hedge = hedge.next;
}
while (hedge != face.he0);
if (!convex)
{
face.mark = Face.NON_CONVEX;
}
return false;
}
protected void calculateHorizon(
Point3d eyePnt, HalfEdge edge0, Face face, Vector horizon)
{
//         oldFaces.add (face);
deleteFacePoints(face, null);
face.mark = Face.DELETED;
if (debug)
{
System.out.println("  visiting face " + face.getVertexString());
}
HalfEdge edge;
if (edge0 == null)
{
edge0 = face.getEdge(0);
edge = edge0;
}
else
{
edge = edge0.getNext();
}
do
{
Face oppFace = edge.oppositeFace();
if (oppFace.mark == Face.VISIBLE)
{
if (oppFace.distanceToPlane(eyePnt) > tolerance)
{
calculateHorizon(eyePnt, edge.getOpposite(),
oppFace, horizon);
}
else
{
horizon.add(edge);
if (debug)
{
System.out.println("  adding horizon edge " +
edge.getVertexString());
}
}
}
edge = edge.getNext();
}
while (edge != edge0);
}
private HalfEdge addAdjoiningFace(
Vertex eyeVtx, HalfEdge he)
{
Face face = Face.createTriangle(
eyeVtx, he.tail(), he.head());
faces.add(face);
face.getEdge(-1).setOpposite(he.getOpposite());
return face.getEdge(0);
}
protected void addNewFaces(
FaceList newFaces, Vertex eyeVtx, Vector horizon)
{
newFaces.clear();
HalfEdge hedgeSidePrev = null;
HalfEdge hedgeSideBegin = null;
for (Iterator it = horizon.iterator(); it.hasNext();)
{
HalfEdge horizonHe = (HalfEdge)it.next();
HalfEdge hedgeSide = addAdjoiningFace(eyeVtx, horizonHe);
if (debug)
{
System.out.println(
"new face: " + hedgeSide.face.getVertexString());
}
if (hedgeSidePrev != null)
{
hedgeSide.next.setOpposite(hedgeSidePrev);
}
else
{
hedgeSideBegin = hedgeSide;
}
newFaces.add(hedgeSide.getFace());
hedgeSidePrev = hedgeSide;
}
hedgeSideBegin.next.setOpposite(hedgeSidePrev);
}
protected Vertex nextPointToAdd()
{
if (!claimed.isEmpty())
{
Face eyeFace = claimed.first().face;
Vertex eyeVtx = null;
float maxDist = 0;
for (Vertex vtx = eyeFace.outside;
vtx != null && vtx.face == eyeFace;
vtx = vtx.next)
{
float dist = eyeFace.distanceToPlane(vtx.pnt);
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
protected void addPointToHull(Vertex eyeVtx)
{
horizon.clear();
unclaimed.clear();
if (debug)
{
System.out.println("Adding point: " + eyeVtx.index);
System.out.println(
" which is " + eyeVtx.face.distanceToPlane(eyeVtx.pnt) +
" above face " + eyeVtx.face.getVertexString());
}
removePointFromFace(eyeVtx, eyeVtx.face);
calculateHorizon(eyeVtx.pnt, null, eyeVtx.face, horizon);
newFaces.clear();
addNewFaces(newFaces, eyeVtx, horizon);

// first merge pass ... merge faces which are non-convex
// as determined by the larger face
for (Face face = newFaces.first(); face != null; face = face.next)
{
if (face.mark == Face.VISIBLE)
{
while (doAdjacentMerge(face, NONCONVEX_WRT_LARGER_FACE))
;
}
}
// second merge pass ... merge faces which are non-convex
// wrt either face
for (Face face = newFaces.first(); face != null; face = face.next)
{
if (face.mark == Face.NON_CONVEX)
{
face.mark = Face.VISIBLE;
while (doAdjacentMerge(face, NONCONVEX))
;
}
}
resolveUnclaimedPoints(newFaces);
}
protected void buildHull()
{
int cnt = 0;
Vertex eyeVtx;
computeMaxAndMin();
createInitialSimplex();
while ((eyeVtx = nextPointToAdd()) != null)
{
addPointToHull(eyeVtx);
cnt++;
if (debug)
{
System.out.println("iteration " + cnt + " done");
}
}
reindexFacesAndVertices();
if (debug)
{
System.out.println("hull done");
}
}
private void markFaceVertices(Face face, int mark)
{
HalfEdge he0 = face.getFirstEdge();
HalfEdge he = he0;
do
{
he.head().index = mark;
he = he.next;
}
while (he != he0);
}
protected void reindexFacesAndVertices()
{
for (int i = 0; i < numPoints; i++)
{
pointBuffer[i].index = -1;
}
// remove inactive faces and mark active vertices
numFaces = 0;
for (Iterator it = faces.iterator(); it.hasNext();)
{
Face face = (Face)it.next();
if (face.mark != Face.VISIBLE)
{
it.remove();
}
else
{
markFaceVertices(face, 0);
numFaces++;
}
}
// reindex vertices
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
protected boolean checkFaceConvexity(
Face face, float tol, PrintStream ps)
{
float dist;
HalfEdge he = face.he0;
do
{
face.checkConsistency();
// make sure edge is convex
dist = oppFaceDistance(he);
if (dist > tol)
{
if (ps != null)
{
ps.println("Edge " + he.getVertexString() +
" non-convex by " + dist);
}
return false;
}
dist = oppFaceDistance(he.opposite);
if (dist > tol)
{
if (ps != null)
{
ps.println("Opposite edge " +
he.opposite.getVertexString() +
" non-convex by " + dist);
}
return false;
}
if (he.next.oppositeFace() == he.oppositeFace())
{
if (ps != null)
{
ps.println("Redundant vertex " + he.head().index +
" in face " + face.getVertexString());
}
return false;
}
he = he.next;
}
while (he != face.he0);
return true;
}
protected boolean checkFaces(float tol, PrintStream ps)
{
// check edge convexity
boolean convex = true;
for (Iterator it = faces.iterator(); it.hasNext();)
{
Face face = (Face)it.next();
if (face.mark == Face.VISIBLE)
{
if (!checkFaceConvexity(face, tol, ps))
{
convex = false;
}
}
}
return convex;
}
/**
 * Checks the correctness of the hull using the distance tolerance
 * returned by {@link QuickHull3D#getDistanceTolerance
 * getDistanceTolerance}; see
 * {@link QuickHull3D#check(PrintStream, float)
 * check(PrintStream,float)} for details.
 *
 * @param ps print stream for diagnostic messages; may be
 *           set to <code>null</code> if no messages are desired.
 * @return true if the hull is valid
 * @see QuickHull3D#check(PrintStream, float)
 */
public boolean check(PrintStream ps)
{
return check(ps, getDistanceTolerance());
}
/**
 * Checks the correctness of the hull. This is done by making sure that
 * no faces are non-convex and that no points are outside any face.
 * These tests are performed using the distance tolerance <i>tol</i>.
 * Faces are considered non-convex if any edge is non-convex, and an
 * edge is non-convex if the centroid of either adjoining face is more
 * than <i>tol</i> above the plane of the other face. Similarly,
 * a point is considered outside a face if its distance to that face's
 * plane is more than 10 times <i>tol</i>.
 *
 * <p>If the hull has been {@link #Triangulate triangulated},
 * then this routine may fail if some of the resulting
 * triangles are very small or thin.
 *
 * @param ps  print stream for diagnostic messages; may be
 *            set to <code>null</code> if no messages are desired.
 * @param tol distance tolerance
 * @return true if the hull is valid
 * @see QuickHull3D#check(PrintStream)
 */
public boolean check(PrintStream ps, float tol)
{
// check to make sure all edges are fully connected
// and that the edges are convex
float dist;
float pointTol = 10 * tol;
if (!checkFaces(tolerance, ps))
{
return false;
}

// check point inclusion
for (int i = 0; i < numPoints; i++)
{
Point3d pnt = pointBuffer[i].pnt;
for (Iterator it = faces.iterator(); it.hasNext();)
{
Face face = (Face)it.next();
if (face.mark == Face.VISIBLE)
{
dist = face.distanceToPlane(pnt);
if (dist > pointTol)
{
if (ps != null)
{
ps.println(
"Point " + i + " " + dist + " above face " +
face.getVertexString());
}
return false;
}
}
}
}
return true;
}
}
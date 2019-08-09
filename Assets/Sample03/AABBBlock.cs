using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EightPoint
{
    public float xMin, xMax, yMin, yMax, zMin, zMax;


    private Vector3 center = Vector3.zero;


    public Vector3 Center
    {
        get
        {
            center.x = (xMax + xMin) / 2;
            center.y = (yMax + yMin) / 2;
            center.z = (zMax + zMin) / 2;
            return center;
        }
    }

    public EightPoint()
    {
    }

    public EightPoint(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        this.xMin = xMin;
        this.xMax = xMax;
        this.yMin = yMin;
        this.yMax = yMax;
        this.zMin = zMin;
        this.zMax = zMax;
    }


    /// <summary>
    /// 得到block的八个点
    /// </summary>
    public List<Vector3> GetEightPoints()
    {
        List<Vector3> list = new List<Vector3>
        {
            new Vector3(xMin, yMin, zMin),
            new Vector3(xMax, yMin, zMin),
            new Vector3(xMin, yMin, zMax),
            new Vector3(xMax, yMin, zMax),
            new Vector3(xMin, yMax, zMin),
            new Vector3(xMax, yMax, zMin),
            new Vector3(xMin, yMax, zMax),
            new Vector3(xMax, yMax, zMax)
        };


        return list;
    }
}

public class AABBBlock
{
    public List<Vector3> points;
    public EightPoint eightPoint;

    private Vector3 center;


    public void Build(List<Vector3> _points, EightPoint _eightPoint
        , out List<List<Vector3>> _eightBlocks, out List<EightPoint> _maxMinBox)
    {
        points = _points;
        eightPoint = _eightPoint;
        center = eightPoint.Center;
        _eightBlocks = CutBlocks();
        _maxMinBox = CutMaxMin();
        RemoveEmpty(_eightBlocks, _maxMinBox);
    }


    public static EightPoint FindFirstMaxMin(List<Vector3> ps)
    {
        EightPoint ep = new EightPoint();
        if (ps.Count > 0)
        {
            ep.xMin = ep.xMax = ps[0].x;
            ep.yMin = ep.yMax = ps[0].y;
            ep.zMin = ep.zMax = ps[0].z;
        }
        else
        {
            throw new Exception("Points Length is zero");
        }

        foreach (var point in ps)
        {
            if (point.x < ep.xMin)
            {
                ep.xMin = point.x;
            }
            else if (point.x > ep.xMax)
            {
                ep.xMax = point.x;
            }

            if (point.y < ep.yMin)
            {
                ep.yMin = point.y;
            }
            else if (point.y > ep.yMax)
            {
                ep.yMax = point.y;
            }

            if (point.z < ep.zMin)
            {
                ep.zMin = point.z;
            }
            else if (point.z > ep.zMax)
            {
                ep.zMax = point.z;
            }
        }

        return ep;
    }

    public List<List<Vector3>> CutBlocks()
    {
        //分成八个块
        //0->0,0,0  1->1,0,0    2->0,0,1    3->1,0,1
        //4->0,1,0  5->1,1,0    6->0,1,1    7->1,1,1
        List<List<Vector3>> eightBlocks = new List<List<Vector3>>(8);
        for (int i = 0; i < 8; i++)
        {
            var list = new List<Vector3>();
            eightBlocks.Add(list);
        }

        foreach (var point in points)
        {
            int i = -1;
            if (point.x <= center.x && point.y <= center.y && point.z <= center.z)
            {
                i = 0;
            }
            else if (point.x > center.x && point.y <= center.y && point.z <= center.z)
            {
                i = 1;
            }
            else if (point.x <= center.x && point.y <= center.y && point.z > center.z)
            {
                i = 2;
            }
            else if (point.x > center.x && point.y <= center.y && point.z > center.z)
            {
                i = 3;
            }
            else if (point.x <= center.x && point.y > center.y && point.z <= center.z)
            {
                i = 4;
            }
            else if (point.x > center.x && point.y > center.y && point.z <= center.z)
            {
                i = 5;
            }
            else if (point.x <= center.x && point.y > center.y && point.z > center.z)
            {
                i = 6;
            }
            else if (point.x > center.x && point.y > center.y && point.z > center.z)
            {
                i = 7;
            }

            if (i < 0)
            {
                throw new Exception("i is <0");
            }

            eightBlocks[i].Add(point);
        }

        return eightBlocks;
    }


    public List<EightPoint> CutMaxMin()
    {
        List<EightPoint> eps = new List<EightPoint>(8);
        EightPoint ep0 =
            new EightPoint(eightPoint.xMin, center.x, eightPoint.yMin, center.y, eightPoint.zMin, center.z);
        EightPoint ep1 =
            new EightPoint(center.x, eightPoint.xMax, eightPoint.yMin, center.y, eightPoint.zMin, center.z);
        EightPoint ep2 =
            new EightPoint(eightPoint.xMin, center.x, eightPoint.yMin, center.y, center.z, eightPoint.zMax);
        EightPoint ep3 =
            new EightPoint(center.x, eightPoint.xMax, eightPoint.yMin, center.y, center.z, eightPoint.zMax);
        EightPoint ep4 =
            new EightPoint(eightPoint.xMin, center.x, center.y, eightPoint.yMax, eightPoint.zMin, center.z);
        EightPoint ep5 =
            new EightPoint(center.x, eightPoint.xMax, center.y, eightPoint.yMax, eightPoint.zMin, center.z);
        EightPoint ep6 =
            new EightPoint(eightPoint.xMin, center.x, center.y, eightPoint.yMax, center.z, eightPoint.zMax);
        EightPoint ep7 =
            new EightPoint(center.x, eightPoint.xMax, center.y, eightPoint.yMax, center.z, eightPoint.zMax);
        eps.Add(ep0);
        eps.Add(ep1);
        eps.Add(ep2);
        eps.Add(ep3);
        eps.Add(ep4);
        eps.Add(ep5);
        eps.Add(ep6);
        eps.Add(ep7);
        return eps;
    }

    public void RemoveEmpty(List<List<Vector3>> _eightBlocks, List<EightPoint> _maxMinBox)
    {
        for (int i = _eightBlocks.Count - 1; i >= 0; i--)
        {
            if (_eightBlocks[i].Count == 0)
            {
                _eightBlocks.RemoveAt(i);
                _maxMinBox.RemoveAt(i);
            }
        }
        if (_eightBlocks.Count == 0)
        {
            _eightBlocks = null;
            _maxMinBox = null;
        }
    }
}
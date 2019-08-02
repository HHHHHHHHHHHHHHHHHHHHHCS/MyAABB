using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public Vector3 pnt;
    public int index;
    public Vertex prev;
    public Vertex next;
    public Face face;

    public Vertex()
    {
        pnt = Vector3.zero;
    }

    public Vertex(float x, float y, float z, int idx)
    {
        pnt = new Vector3(x, y, z);
        index = idx;
    }

    public Vertex(Vector3 p, int idx)
    {
        pnt = p;
        index = idx;
    }

    public static implicit operator Vector3(Vertex vertex)
    {
        return vertex.pnt;
    }
}
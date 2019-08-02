using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExVector3
{
    public static float DistanceSquared(this Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        float dz = a.z - b.z;
        return dx * dx + dy * dy + dz * dz;
    }
}
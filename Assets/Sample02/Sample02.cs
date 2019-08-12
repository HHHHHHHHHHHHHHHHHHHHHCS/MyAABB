namespace QHull
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;


    public class Sample02 : MonoBehaviour
    {
        public MeshFilter ori, col;

        public void Awake()
        {
            QuickHull3D hull = new QuickHull3D();
            hull.Build(ori.mesh.vertices);

            Vector3[] vertices = hull.GetVertices();

            int[] faceIndices = hull.GetFaces();

            Mesh mesh = new Mesh {vertices = vertices, triangles = faceIndices };
            col.mesh = mesh;
        }
    }
}
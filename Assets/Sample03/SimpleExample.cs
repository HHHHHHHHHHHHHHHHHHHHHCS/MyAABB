namespace QHull
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

/**
 * Simple example usage of QuickHull3D. Run as the command
 * <pre>
 *   java quickhull3d.SimpleExample
 * </pre>
 */
    public class SimpleExample : MonoBehaviour
    {
        public MeshFilter mesh;
        public GameObject prefab_s,prefab_m;

        /**
         * Run for a simple demonstration of QuickHull3D.
         */
        public void Awake()
        {
            Vector3[] v3s = mesh.mesh.vertices;
            AABB aabb = new AABB();
            var abps = aabb.Build(v3s);
            var points = ToPoint3d(abps.ToArray());

            foreach (var point in abps)
            {
                //Instantiate(prefab_s, point, Quaternion.identity);
            }

            Debug.Log("Ori Vertex Count:" + v3s.Length);

            QuickHull3D hull = new QuickHull3D();
            hull.build(points);

            Point3d[] vertices = hull.getVertices();
            StringBuilder sb = new StringBuilder();
            sb.Append($"Vertices:{vertices.Length}\n");
            for (int i = 0; i < vertices.Length; i++)
            {
                Point3d pnt = vertices[i];
                sb.Append($"  {pnt.x},{pnt.y},{pnt.z}\n");
            }

            Debug.Log(sb.ToString());

            sb.Clear();
            int[][] faceIndices = hull.getFaces();
            sb.Append($"Faces:{vertices.Length}\n");
            for (int i = 0; i < vertices.Length; i++)
            {
                for (int k = 0; i < faceIndices.Length && k < faceIndices[i].Length; k++)
                {
                    sb.Append($"{faceIndices[i][k]} ");

                    Instantiate(prefab_m, ToVector3(vertices[faceIndices[i][k]]), Quaternion.identity);
                }

                sb.Append('\n');
            }

            Debug.Log(sb.ToString());
            sb.Clear();
        }

        public static Point3d[] ToPoint3d(Vector3[] v3s)
        {
            Point3d[] p3s = new Point3d[v3s.Length];
            for (int i = 0; i < v3s.Length; i++)
            {
                Vector3 v3 = v3s[i];
                p3s[i] = new Point3d(v3.x, v3.y, v3.z);
            }

            return p3s;
        }

        public static Vector3 ToVector3(Point3d p3d)
        {
            return new Vector3((float) p3d.x, (float) p3d.y, (float) p3d.z);
        }
    }
}
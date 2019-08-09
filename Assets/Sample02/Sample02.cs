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
        public GameObject prefab_s, prefab_m;
        private int[] faceArray, triArray;


        public void Awake()
        {
            Vector3[] v3s = ori.mesh.vertices;
            AABB aabb = new AABB();
            var abps = aabb.Build(v3s);
            var points = abps.ToArray();

            foreach (var point in abps)
            {
                //Instantiate(prefab_s, point, Quaternion.identity);
            }

            Debug.Log("Ori Vertex Count:" + v3s.Length);

            QuickHull3D hull = new QuickHull3D();
            hull.build(ToPoint3d(points));

            Point3d[] vertices = hull.getVertices();
            StringBuilder sb = new StringBuilder();
            sb.Append($"Vertices:{vertices.Length}\n");
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3d pnt = vertices[i];
                sb.Append($"  {pnt.x},{pnt.y},{pnt.z}\n");
            }

            Debug.Log(sb.ToString());


            sb.Clear();
            List<int> faces = new List<int>();
            int[][] faceIndices = hull.getFaces();
            sb.Append($"Faces:{vertices.Length}\n");
            for (int i = 0; i < vertices.Length; i++)
            {
                for (int j = 0; i < faceIndices.Length && j < faceIndices[i].Length; j++)
                {
                    sb.Append($"{faceIndices[i][j]} ");
                    if (j >= 2)
                    {
                        faces.Add(faceIndices[i][0]);
                        for (int k = -1; k <= 0; k++)
                        {
                            faces.Add(faceIndices[i][j + k]);
                        }
                    }

                    Instantiate(prefab_m, ToVector3(vertices[faceIndices[i][j]]), Quaternion.identity);
                }

                sb.Append('\n');
            }

            Debug.Log(sb.ToString());
            sb.Clear();

            faceArray = faces.ToArray();
            triArray = new int[faceArray.Length];
            Mesh mesh = new Mesh {vertices = ToVector3(vertices), triangles = faceArray};
            col.mesh = mesh;
            //StartCoroutine(NewMesh());
        }

        private IEnumerator NewMesh()
        {
            for (int i = 0; i < faceArray.Length / 3; i++)
            {
                triArray[3 * i + 0] = faceArray[3 * i + 0];
                triArray[3 * i + 1] = faceArray[3 * i + 1];
                triArray[3 * i + 2] = faceArray[3 * i + 2];
                col.mesh.triangles = triArray;
                yield return new WaitForSeconds(0.05f);
            }
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
            return new Vector3((float)p3d.x, (float)p3d.y, (float)p3d.z);
        }

        public static Vector3[] ToVector3(Point3d[] p3ds)
        {
            Vector3[] v3s = new Vector3[p3ds.Length];
            for (int i = 0; i < p3ds.Length; i++)
            {
                v3s[i] = ToVector3(p3ds[i]);
            }

            return v3s;
        }
    }
}
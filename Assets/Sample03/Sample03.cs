namespace QHull
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;


    public class Sample03 : MonoBehaviour
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
            hull.Build(points);

            Vector3[] vertices = hull.GetVertices();
            StringBuilder sb = new StringBuilder();
            sb.Append($"Vertices:{vertices.Length}\n");
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 pnt = vertices[i];
                sb.Append($"  {pnt.x},{pnt.y},{pnt.z}\n");
            }

            Debug.Log(sb.ToString());


            sb.Clear();
            List<int> faces = new List<int>();
            int[][] faceIndices = hull.GetFaces();
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

                    Instantiate(prefab_m, vertices[faceIndices[i][j]], Quaternion.identity);
                }

                sb.Append('\n');
            }

            Debug.Log(sb.ToString());
            sb.Clear();

            faceArray = faces.ToArray();
            triArray = new int[faceArray.Length];
            Mesh mesh = new Mesh {vertices = vertices, triangles = faceArray};
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
    }
}
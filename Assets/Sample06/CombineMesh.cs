using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using QHull;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;


[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class CombineMesh : MonoBehaviour
{
    void Start()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(); //包括了自己所以要跳过
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];
        Material[] mats = new Material[meshFilters.Length - 1];
        Matrix4x4 matrix = transform.worldToLocalMatrix;
        int index = 0;
        for (int i = 1; i < meshFilters.Length; i++)
        {
            MeshFilter mf = meshFilters[i];
            MeshRenderer mr = meshFilters[i].GetComponent<MeshRenderer>();
            if (mr == null)
            {
                continue;
            }

            combine[index].mesh = mf.sharedMesh;
            combine[index].transform = matrix * mf.transform.localToWorldMatrix;
            mr.enabled = false;
            mats[index] = mr.sharedMaterial;
            index++;
        }

        MeshFilter thisMeshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh {name = "Combined"};
        thisMeshFilter.mesh = mesh;
        mesh.CombineMeshes(combine, false);
        MeshRenderer thisMeshRenderer = GetComponent<MeshRenderer>();
        thisMeshRenderer.sharedMaterials = mats;
        thisMeshRenderer.enabled = true;

        MeshCollider thisMeshCollider = GetComponent<MeshCollider>();
        if (thisMeshCollider != null)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Vector3[] points = mesh.vertices;


            EightBlockTree eightTree = new EightBlockTree();
            points = eightTree.Build(points, 8);

            QuickHull3D hull = new QuickHull3D();
            hull.Build(points);


            Vector3[] vertices = hull.GetVertices();

            int[] faceIndices = hull.GetFaces();

            Mesh colMesh = new Mesh {vertices = vertices, triangles = faceIndices};
            sw.Stop();
            Debug.Log(sw.Elapsed);

            thisMeshCollider.sharedMesh = colMesh;
        }
        else
        {
            thisMeshCollider = gameObject.AddComponent<MeshCollider>();
            thisMeshCollider.sharedMesh = mesh;
            thisMeshCollider.convex = true;
        }
    }
}
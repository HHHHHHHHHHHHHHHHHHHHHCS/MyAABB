using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QHull;
using UnityEditor;
using UnityEngine;

public class CreateColliderWindow : EditorWindow
{
    //注意 模型要-90  而且 File0.1cm->unity0.01m

    public string folderPath = "";
    public List<string> objsPath = new List<string>();
    public List<string> removePath = new List<string>();

    private readonly string[] blockLvString = { "1", "2", "3", "4", "5", "6", "7", "8" };
    private readonly GUIStyle style = new GUIStyle();


    private int blockLv = 2;//使用的时候要+1  选择数组是从0开始的
    private Vector2 viewPos;
    private bool isFoldObjs = true;
    private bool useAABB;

    [MenuItem("Tools/CreateCollider")]
    private static void CreateWindow()
    {
        GetWindow<CreateColliderWindow>(false, "ColliderWindow", true).Show();
    }

    private void OnGUI()
    {
        viewPos = GUILayout.BeginScrollView(viewPos);


        GUILayout.BeginHorizontal();
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        GUILayout.Label("Path:" + folderPath, style);
        if (GUILayout.Button("选择", GUILayout.Width(100f)))
        {
            var str = EditorUtility.OpenFolderPanel("模型文件夹", folderPath, "");
            if (!string.IsNullOrEmpty(str))
            {
                folderPath = str.Remove(0, Application.dataPath.Length - 6);

                objsPath.Clear();
                string[] allPath = AssetDatabase.FindAssets("t:Model", new[] {folderPath});
                foreach (var s in allPath)
                {
                    var path = AssetDatabase.GUIDToAssetPath(s);
                    objsPath.Add(path);
                }
            }
        }

        GUILayout.EndHorizontal();


        GUILayout.Space(10);
        useAABB = GUILayout.Toggle(useAABB, "使用切块");
        if (useAABB)
        {
            GUILayout.Label("切块等级(数量是2的次方):");
            blockLv = GUILayout.SelectionGrid(blockLv, blockLvString, blockLvString.Length/2);
        }

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("生成", GUILayout.Width(100f)))
        {
            for (int i = 0; i < objsPath.Count; i++)
            {
                var path = objsPath[i];
                EditorUtility.DisplayProgressBar("处理Mesh Collider", $"处理个数{i}/{objsPath.Count}", (float)i / objsPath.Count);
                var mesh = CalcMesh(path);
                var newPath = path.Substring(0, path.LastIndexOf('.'));
                AssetDatabase.CreateAsset(mesh, newPath + "_mc.asset");
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        style.fontStyle = FontStyle.Normal;
        style.fontSize = 15;
        isFoldObjs = EditorGUILayout.Foldout(isFoldObjs, "Foldout");
        if (isFoldObjs)
        {
            removePath.Clear();
            foreach (var item in objsPath)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("   " + item, style);
                if (GUILayout.Button("X", GUILayout.Width(50f)))
                {
                    removePath.Add(item);
                }

                GUILayout.EndHorizontal();
            }

            foreach (var item in removePath)
            {
                objsPath.Remove(item);
            }
        }


        GUILayout.EndScrollView();
    }

    private Mesh CalcMesh(string path)
    {
        var oriMesh = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        List<Vector3> allPoints = new List<Vector3>(1024);

        foreach (var mesh in oriMesh.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            allPoints.AddRange(mesh.sharedMesh.vertices);
        }

        foreach (var mesh in oriMesh.GetComponentsInChildren<MeshFilter>())
        {
            allPoints.AddRange(mesh.sharedMesh.vertices);
        }

        Vector3[] points = allPoints.ToArray();

        if (useAABB)
        {
            EightBlockTree eightTree = new EightBlockTree();
            points = eightTree.Build(points, blockLv + 1);
        }

        QuickHull3D hull = new QuickHull3D();
        hull.Build(points);

        Vector3[] vertices = hull.GetVertices();

        int[] faceIndices = hull.GetFaces();

        Mesh newMesh = new Mesh { vertices = vertices, triangles = faceIndices };

        return newMesh;
    }
}
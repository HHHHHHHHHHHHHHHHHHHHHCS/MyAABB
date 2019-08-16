using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using QHull;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CreateColliderWindow : EditorWindow
{
    //注意 模型要-90  而且 File0.1cm->unity0.01m

    public string folderPath = "";
    public List<string> objsPath = new List<string>();
    public List<string> removePath = new List<string>();

    private readonly string[] blockLvString = { "1", "2", "3", "4", "5", "6", "7", "8" };
    private readonly GUIStyle style = new GUIStyle();


    private int blockLv = 3;
    private Vector2 viewPos;
    private bool isFoldObjs = true;
    private bool isOutLogs = true;
    private bool useEightBlocks = true;

    private StringBuilder strBuilder = new StringBuilder();

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
                string[] allPath = AssetDatabase.FindAssets("t:Model", new[] { folderPath });
                foreach (var s in allPath)
                {
                    var path = AssetDatabase.GUIDToAssetPath(s);
                    objsPath.Add(path);
                }
            }
        }

        GUILayout.EndHorizontal();


        GUILayout.Space(10);
        isOutLogs = GUILayout.Toggle(isOutLogs, "输出Logs信息");
        useEightBlocks = GUILayout.Toggle(useEightBlocks, "使用切块");
        if (useEightBlocks)
        {
            GUILayout.Label("切块等级(数量是2的次方):");
            int temp = blockLv - 1;
            temp = GUILayout.SelectionGrid(temp, blockLvString, blockLvString.Length / 2);
            blockLv = temp + 1;
        }

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("不使用切块 和 1-5级切块 多级生成", GUILayout.Width(200f)))
        {
            for (int lod = 0; lod <= 5; lod++)
            {
                bool _useEightBlocks = lod > 0;
                int _blockLv = lod;

                Stopwatch sw = Stopwatch.StartNew();
                string endPath = $"_mc_{_blockLv}.asset";
                for (int i = 0; i < objsPath.Count; i++)
                {
                    var path = objsPath[i];
                    EditorUtility.DisplayProgressBar("处理Mesh Collider", $"处理个数{i}/{objsPath.Count}",
                        (float)i / objsPath.Count);
                    var mesh = CalcMesh(path, _useEightBlocks, _blockLv);
                    var newPath = path.Substring(0, path.LastIndexOf('.'));
                    AssetDatabase.CreateAsset(mesh, newPath + endPath);
                }

                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                sw.Stop();
                InsertLogLine(0, $"UseTime:{sw.Elapsed} "
                                 + (_useEightBlocks ? "Use Eight Block Cut:" + _blockLv : "No Use Eight Block Cut"));
                OutLog();
            }
        }

        if (GUILayout.Button("指定生成", GUILayout.Width(100f)))
        {
            Stopwatch sw = Stopwatch.StartNew();
            string endPath = $"_mc_{blockLv}.asset";
            for (int i = 0; i < objsPath.Count; i++)
            {
                var path = objsPath[i];
                EditorUtility.DisplayProgressBar("处理Mesh Collider", $"处理个数{i}/{objsPath.Count}",
                    (float)i / objsPath.Count);
                var mesh = CalcMesh(path, useEightBlocks, blockLv);
                var newPath = path.Substring(0, path.LastIndexOf('.'));
                //AssetDatabase.CreateAsset(mesh, newPath + endPath);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            sw.Stop();
            InsertLogLine(0, $"UseTime:{sw.Elapsed} "
                             + (useEightBlocks ? "Use Eight Block Cut:" + blockLv : "No Use Eight Block Cut"));
            OutLog();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        style.fontStyle = FontStyle.Normal;
        style.fontSize = 15;
        isFoldObjs = EditorGUILayout.Foldout(isFoldObjs, "Models:" + objsPath.Count);
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

    private Mesh CalcMesh(string _path, bool _useEightBlocks, int _blockLv)
    {
        var oriMesh = AssetDatabase.LoadAssetAtPath<GameObject>(_path);

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


        AppendLogLine(_path);
        AppendLogLine("  Ori Vertexs:", points.Length.ToString());

        if (_useEightBlocks)
        {
            EightBlockTree eightTree = new EightBlockTree();
            points = eightTree.Build(points, _blockLv);
            AppendLogLine("  EightBlockTree Vertexs:", points.Length.ToString());
        }

        QuickHull3D hull = new QuickHull3D();
        hull.Build(points);

        Vector3[] vertices = hull.GetVertices();

        int[] faceIndices = hull.GetFaces();

        Mesh newMesh = new Mesh { vertices = vertices, triangles = faceIndices };
        AppendLogLine("  End vertices:", vertices.Length.ToString());
        AppendLogLine("  End faceIndices:", faceIndices.Length.ToString());

        return newMesh;
    }

    private void AppendLogLine(params string[] strs)
    {
        if (isOutLogs)
        {
            foreach (var str in strs)
            {
                strBuilder.Append(str);
            }

            strBuilder.AppendLine();
        }
    }

    private void InsertLogLine(int index, string str)
    {
        if (isOutLogs)
        {
            strBuilder.Insert(index, str + '\n');
        }
    }

    private void OutLog()
    {
        if (isOutLogs)
        {
            Debug.Log(strBuilder.ToString());
            strBuilder.Clear();
        }
    }
}
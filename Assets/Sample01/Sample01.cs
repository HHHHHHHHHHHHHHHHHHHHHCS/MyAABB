using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Sample01 : MonoBehaviour
{
    public List<Vector2> vec2s;
    public List<int> paths;

    private void Awake()
    {
        vec2s = new List<Vector2>(100);
        paths = new List<int>(10);
        Spawn();
        StartCoroutine(DoMain_1());
    }

    #region Example01

    private void Spawn()
    {
        for (int i = 0; i < 100; i++)
        {
            Add();
        }
    }

    private void Add(float? x = null, float? z = null)
    {
        Vector3 v3 = Random.insideUnitCircle * 10;
        if (x == null)
        {
            x = v3.x;
        }

        if (z == null)
        {
            z = v3.y;
        }

        vec2s.Add(new Vector2(x.Value, z.Value));
    }

    private void OnDrawGizmos()
    {
        if (vec2s != null && vec2s.Count > 0)
        {
            Gizmos.color = Color.black;
            foreach (var item in vec2s)
            {
                Gizmos.DrawSphere(V2TOV3(item), 0.1f);
            }

            Gizmos.color = Color.red;

            foreach (var item in paths)
            {
                Gizmos.DrawSphere(V2TOV3(item), 0.15f);
            }


            Gizmos.color = Color.green;
            //Gizmos.DrawSphere(V2TOV3(next), 0.15f);

            Gizmos.color = Color.blue;
            //Gizmos.DrawSphere(V2TOV3(check), 0.15f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(V2TOV3(now), V2TOV3(next));

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(V2TOV3(now), V2TOV3(check));
        }
    }


    private Vector3 V2TOV3(Vector2 v2) => new Vector3(v2.x, 0, v2.y);

    private Vector3 V2TOV3(int index) => new Vector3(vec2s[index].x, 0, vec2s[index].y);

    #endregion

    private int now, next, check;

    private IEnumerator DoMain_1()
    {
        SortMinX();
        now = 0;
        next = now + 1;
        paths.Add(now);

        for (int i = 0; i < 10000; i++)
        {
            for (check = next + 1; check < vec2s.Count; check++)
            {
                if (paths.Contains(check) || check == next)
                {
                    continue;
                }

                var a = vec2s[next] - vec2s[now];
                var b = vec2s[check] - vec2s[now];
                var cross = Vector3.Cross(V2TOV3(a), V2TOV3(b));

                if (cross.y <= 0)
                {
                    next = check;
                }


                if (check == vec2s.Count - 1)
                {
                    now = next;
                    paths.Add(now);
                    next = Mathf.Min(next + 1, vec2s.Count - 1);
                    break;
                }
            }

            if (next == vec2s.Count - 1)
            {
                now = next;
                paths.Add(now);
                break;
            }

            yield return new WaitForSeconds(0.01f);
        }


        for (int i = 0; i < 10000; i++)
        {
            for (check = next - 1; check >= 0; check--)
            {
                if (check != 0 && paths.Contains(check))
                {
                    continue;
                }

                var a = vec2s[next] - vec2s[now];
                var b = vec2s[check] - vec2s[now];
                var cross = Vector3.Cross(V2TOV3(a), V2TOV3(b));

                if (cross.y <= 0)
                {
                    next = check;
                }


                if (check == 0)
                {
                    now = next;
                    paths.Add(now);
                    next = Mathf.Max(next - 1, 0);
                    break;
                }
            }

            if (next == 0)
            {
                break;
            }
        }
    }

    private void SortMinX()
    {
        vec2s.Sort((x, y) => (x.x < y.x ? -1 : 1));
    }
}
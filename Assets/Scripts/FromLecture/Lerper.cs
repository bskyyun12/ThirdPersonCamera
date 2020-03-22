using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class Lerper : MonoBehaviour
{

    [Range(0, 1)]
    public float t = 0;

    public Transform[] p;
    public Camera cam;

    void OnDrawGizmos()
    {
        Vector3[] pts = p.Select(x => x.position).ToArray();

        Gizmos.DrawLine(pts[0], pts[1]);
        Gizmos.DrawLine(pts[2], pts[3]);

        DrawBezier(pts);

        Vector3 curveTangent = GetTangent(t, pts);
        cam.transform.position = GetPoint(t, pts);
        cam.transform.rotation = Quaternion.LookRotation(curveTangent, Vector3.up);
    }

    static Vector3 GetTangent(float t, Vector3[] pts)
    {
        Vector3 a = Vector3.Lerp(pts[0], pts[1], t);
        Vector3 b = Vector3.Lerp(pts[1], pts[2], t);
        Vector3 c = Vector3.Lerp(pts[2], pts[3], t);
        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);
        return (e - d).normalized;
    }

    static Vector3 GetPoint(float t, Vector3[] pts)
    {
        Vector3 a = Vector3.Lerp(pts[0], pts[1], t);
        Vector3 b = Vector3.Lerp(pts[1], pts[2], t);
        Vector3 c = Vector3.Lerp(pts[2], pts[3], t);
        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(d, e, t);
    }

    void DrawBezier(Vector3[] pts)
    {
        const int DETAIL = 32;
        Vector3[] drawPts = new Vector3[DETAIL];
        for (int i = 0; i < DETAIL; i++)
        {
            float tDraw = i / (DETAIL - 1f);
            drawPts[i] = GetPoint(tDraw, pts);
        }
        Handles.DrawAAPolyLine(drawPts);
    }


}
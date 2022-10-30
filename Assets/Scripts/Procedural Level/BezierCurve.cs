using System.Collections;

using UnityEngine;
using UnityEditor;

public class BezierCurve : MonoBehaviour {
    public Vector3[] points;

    public void Reset() {
        points = new Vector3[] {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f)
        };
    }

    public Vector3 GetPoint(float t) {
        return transform.TransformPoint(GetPoint(points[0], points[1], points[2], t));
    }

    public Vector3 GetVelocity(float t) {
        return transform.TransformPoint(GetFirstDerivative(points[0], points[1], points[2], t)) - transform.position;
    }

    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
        return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
        return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
    }
}
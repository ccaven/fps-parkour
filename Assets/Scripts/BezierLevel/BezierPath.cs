using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierPath : MonoBehaviour {

    private struct PathPoint {
        public Vector3 position { get; set; }
        public Vector3 tangent { get; set; }

        public PathPoint(Vector3 pos, Vector3 dir) {
            position = pos;
            tangent = dir;
        }
    }

    private List<PathPoint> pathPoints = new List<PathPoint>();

    private PathPoint GetPointAtTime(float t) {
        t = Mathf.Clamp(t, 0, pathPoints.Count - 1);

        int t0 = Mathf.FloorToInt(t);
        int t1 = Mathf.CeilToInt(t);

        float k = t - t0;
        float ik = 1 - k;

        PathPoint start = pathPoints[t0];
        PathPoint end = pathPoints[t1];

        float l = (end.position - start.position).magnitude;

        Vector3 p0 = start.position;
        Vector3 p1 = start.position + start.tangent * l / 2;
        Vector3 p2 = end.position - end.tangent * l / 2;
        Vector3 p3 = end.position;

        Vector3 r = p0 * ik * ik * ik + 3 * p1 * ik * ik * k + 3 * p2 * ik * k * k + p3 * k * k * k;

        Vector3 tangent = 3 * ik * ik * (p1 - p0) + 6 * ik * k * (p2 - p1) + 3 * k * k * (p3 - p2);

        return new PathPoint(r, tangent);
    }

    private Vector3 GetPointAroundCircleAtTime(float t, float r, float ang) {
        PathPoint bezierPoint = GetPointAtTime(t);
        Vector3 pointOnCircle = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0) * r;

        Quaternion rot = Quaternion.LookRotation(bezierPoint.tangent);
        
        return rot * pointOnCircle + bezierPoint.position;
    }

    private void AddPathPoint(Vector3 position, Vector3 direction) {
        this.pathPoints.Add(new PathPoint(position, direction));
    }

    private void Start() {

        pathPoints.Clear();

        // Sample point
        Vector3 currentPoint = Vector3.zero;

        AddPathPoint(currentPoint, Vector3.forward);

        currentPoint += Vector3.forward * 5 + Vector3.up * 4;

        AddPathPoint(currentPoint, Vector3.right);

        currentPoint += Vector3.right * 5;

        AddPathPoint(currentPoint, -Vector3.forward);

        currentPoint -= Vector3.forward * 5;
        
        AddPathPoint(currentPoint, -Vector3.up);

        currentPoint -= Vector3.up * 5;

        AddPathPoint(currentPoint, -Vector3.right);

    }

    private void DrawRings() {
        float timeIncrement = 0.1f;

        float angleIncrementRing = Mathf.PI / 20;
        float angleIncrement = Mathf.PI / 5;

        for (float t = 0; t < pathPoints.Count; t += timeIncrement) {
            for (float a = 0; a < Mathf.PI * 2; a += angleIncrementRing) {
                Vector3 p1 = GetPointAroundCircleAtTime(t, 0.1f, a);
                Vector3 p2 = GetPointAroundCircleAtTime(t, 0.1f, a + angleIncrementRing);
                Debug.DrawLine(p1, p2, Color.red);
            }
        }

        for (float a = 0; a < Mathf.PI * 2; a += angleIncrement) {

            for (float t = 0; t < pathPoints.Count; t += timeIncrement) {

                Vector3 p1 = GetPointAroundCircleAtTime(t, 0.1f, a);
                Vector3 p2 = GetPointAroundCircleAtTime(t + timeIncrement, 0.1f, a);

                Debug.DrawLine(p1, p2);

            }

        }
    }

    private void DrawTangents() {
        float increment = 0.1f;

        for (float t = 0; t < pathPoints.Count; t += increment) {
            PathPoint pathPoint = GetPointAtTime(t);
            Debug.DrawLine(pathPoint.position, pathPoint.position + pathPoint.tangent, Color.green);
        }
    }

    private void DrawPath() {
        float increment = 0.1f;

        for (float t = 0; t < pathPoints.Count; t += increment) {
            PathPoint pathPoint = GetPointAtTime(t);
            PathPoint pathPoint2 = GetPointAtTime(t + increment);
            Debug.DrawLine(pathPoint.position, pathPoint2.position, Color.white);
        }        
    }

    private void Update() {
        DrawPath();
        DrawRings();
    }
}

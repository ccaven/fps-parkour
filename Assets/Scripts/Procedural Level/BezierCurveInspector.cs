
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurveInspector : Editor {

    private BezierCurve curve;
    private Transform handleTransform;
    private Quaternion handleRotation;


    private void OnSceneGUI() {
        
        curve = target as BezierCurve;

        handleTransform = curve.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        Vector3 p0 = ShowPoint(0),
                p1 = ShowPoint(1),
                p2 = ShowPoint(2);

        Handles.color = Color.white;

        int lineSteps = 10;

        Vector3 lineStart = curve.GetPoint(0f);
        for (int i = 1; i <= lineSteps; i++) {
            Vector3 lineEnd = curve.GetPoint(i / (float)lineSteps);
            Handles.DrawLine(lineStart, lineEnd);
            lineStart = lineEnd;
        }


    }



    private Vector3 ShowPoint(int index) {

        Vector3 point = handleTransform.TransformPoint(curve.points[index]);

        EditorGUI.BeginChangeCheck();

        point = Handles.DoPositionHandle(point, handleRotation);

        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(curve, "Move Point");
            
            
            EditorUtility.SetDirty(curve);

            curve.points[index] = handleTransform.InverseTransformPoint(point);
        }

        return point;
    }

}


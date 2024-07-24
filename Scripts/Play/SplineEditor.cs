using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Spline))]
public class SplineEditor : Editor
{
    private Spline spline;

    private void OnSceneGUI()
    {
        spline = target as Spline;

        for (int i = 0; i < spline.ControlPoints.Count; i++)
        {
            ShowPoint(i);
        }

        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 150, 100));
        if (GUILayout.Button("Add Point"))
        {
            Undo.RecordObject(spline, "Add Point");
            Vector3 newPoint = spline.ControlPoints.Count > 0
                ? spline.transform.TransformPoint(spline.ControlPoints[spline.ControlPoints.Count - 1]) + Vector3.right
                : spline.transform.position;
            spline.AddControlPoint(spline.transform.InverseTransformPoint(newPoint));
        }
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    private void ShowPoint(int index)
    {
        Vector3 point = spline.transform.TransformPoint(spline.ControlPoints[index]);
        EditorGUI.BeginChangeCheck();
        point = Handles.DoPositionHandle(point, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Move Point");
            spline.ControlPoints[index] = spline.transform.InverseTransformPoint(point);
            EditorUtility.SetDirty(spline);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

public class Spline : MonoBehaviour
{
    [SerializeField] private List<Vector3> controlPoints = new List<Vector3>();

    public List<Vector3> ControlPoints => controlPoints;

    public void AddControlPoint(Vector3 point)
    {
        controlPoints.Add(transform.InverseTransformPoint(point));
    }

    public int GetControlPointsCount()
    {
        return controlPoints.Count;
    }

    public Vector3 GetPoint(float t)
    {
        if (controlPoints.Count < 4)
        {
            Debug.LogWarning("Spline requires at least 4 control points");
            return Vector3.zero;
        }

        int count = controlPoints.Count - 3;
        int p0 = Mathf.Clamp(Mathf.FloorToInt(t * count), 0, count - 1);
        int p1 = p0 + 1;
        int p2 = p1 + 1;
        int p3 = p2 + 1;

        t = t * count - p0;

        return CatmullRom(controlPoints[p0], controlPoints[p1], controlPoints[p2], controlPoints[p3], t);
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * ((2.0f * p1) +
                      (-p0 + p2) * t +
                      (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 +
                      (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t3);
    }

    private void OnDrawGizmos()
    {
        if (controlPoints.Count >= 4)
        {
            Gizmos.color = Color.yellow;
            Vector3 previousPoint = transform.TransformPoint(controlPoints[0]);
            for (int i = 1; i <= 100; i++)
            {
                float t = i / 100f;
                Vector3 point = transform.TransformPoint(GetPoint(t));
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }

            for (int i = 0; i < controlPoints.Count; i++)
            {
                if (i == 0)
                {
                    Gizmos.color = Color.green; // First control point
                }
                else if (i == controlPoints.Count - 1)
                {
                    Gizmos.color = Color.red; // Last control point
                }
                else
                {
                    Gizmos.color = Color.yellow; // Intermediate control points
                }

                Gizmos.DrawSphere(transform.TransformPoint(controlPoints[i]), 0.1f);
            }
        }
    }

    private void Start()
    {
        if (controlPoints.Count == 0)
        {
            AddControlPoint(transform.position);
        }
    }
}
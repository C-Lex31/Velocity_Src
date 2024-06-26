using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLimiters : MonoBehaviour
{
    [SerializeField] private Transform start;
    [SerializeField] private Transform end;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(start.position, new Vector2(start.position.x , start.position.y + 1000));
        Gizmos.DrawLine(start.position, new Vector2(start.position.x , start.position.y - 1000));
        
        Gizmos.DrawLine(end.position, new Vector2(end.position.x, end.position.y + 1000));
        Gizmos.DrawLine(end.position, new Vector2(end.position.x ,end.position.y - 1000));
    }
}

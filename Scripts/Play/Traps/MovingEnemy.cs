using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEnemy: TrapBase
{
   [SerializeField]  private float moveSpeed = 5f; // Speed of the enemy
    [SerializeField]private float moveDistance = 10f; // Distance the enemy travels in one direction
    [SerializeField]private float pauseDuration = 1f; // Time the enemy pauses at each end

    private Vector3 startPos;
    private Vector3 endPos;
    private bool movingToEnd = true;
    private bool isPaused = false;

    protected override  void Start()
    {
        base.Start();
        startPos = transform.position;
        endPos = startPos + Vector3.right * moveDistance;
    }

    private void Update()
    {
        if (!isPaused)
        {
            MoveDrone();
        }
    }

    private void MoveDrone()
    {
        if (movingToEnd)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, endPos) < 0.1f)
            {
                StartCoroutine(PauseAtEnd());
                movingToEnd = false;
                Flip();
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, startPos) < 0.1f)
            {
                StartCoroutine(PauseAtEnd());
                movingToEnd = true;
                Flip();
            }
        }
    }

    private IEnumerator PauseAtEnd()
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseDuration);
        isPaused = false;
    }

    private void Flip()
    {
        // Flip  by rotating around the Y-axis by 180 degrees
        Vector3 rotation = transform.eulerAngles;
        rotation.y += 180;
        transform.eulerAngles = rotation;
    }
  void OnTriggerEnter(Collider col)
    {
        if(!(col.gameObject.tag=="Player"))return;

        Debug.Log("BOOM");
       // GetComponentInChildren<Collider>().

    }

  void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawSphere(startPos, 0.2f);
            Gizmos.DrawSphere(endPos, 0.2f);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * moveDistance);
            Gizmos.DrawSphere(transform.position, 0.2f);
            Gizmos.DrawSphere(transform.position + Vector3.right * moveDistance, 0.2f);
        }
    }
}

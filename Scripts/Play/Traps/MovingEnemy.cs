using System.Collections;
using UnityEngine;

public class MovingEnemy : TrapBase
{
    public enum MovementDirection
    {
        Horizontal,
        Vertical,
        OneWay // New movement direction
    }

    [SerializeField] private float moveSpeed = 5f; // Speed of the enemy
    [SerializeField] private float moveDistance = 10f; // Distance the enemy travels in one direction
    [SerializeField] private float pauseDuration = 1f; // Time the enemy pauses at each end
    [SerializeField] private MovementDirection movementDirection = MovementDirection.Horizontal; // Direction of movement

    private Vector3 startPos;
    private Vector3 endPos;
    private bool movingToEnd = true;
    private bool isPaused = false;

    protected override void Start()
    {
        base.Start();
        startPos = transform.position;
        if (movementDirection == MovementDirection.Horizontal)
        {
            endPos = startPos + Vector3.right * moveDistance;
        }
        else if (movementDirection == MovementDirection.Vertical)
        {
            endPos = startPos + Vector3.up * moveDistance;
        }
        else if (movementDirection == MovementDirection.OneWay)
        {
            endPos = startPos + Vector3.left * moveDistance; // Moving right to left
        }
    }

    private void Update()
    {
        if (!isPaused)
        {
            MoveEnemy();
        }
    }

    private void MoveEnemy()
    {
        if (movementDirection == MovementDirection.OneWay)
        {
            // One-way movement logic
            transform.position = Vector3.MoveTowards(transform.position, endPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, endPos) < 0.1f)
            {
                Destroy(gameObject); // Destroy the enemy when it reaches the end position
            }
        }
        else
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
    }

    private IEnumerator PauseAtEnd()
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseDuration);
        isPaused = false;
    }

    private void Flip()
    {
        // Flip by rotating around the Y-axis by 180 degrees for horizontal or X-axis for vertical
        Vector3 rotation = transform.eulerAngles;
        if (movementDirection == MovementDirection.Horizontal)
        {
            rotation.y += 180;
        }
        else if (movementDirection == MovementDirection.Vertical)
        {
            rotation.x += 180;
        }
        transform.eulerAngles = rotation;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag != "Player") return;

        Debug.Log("BOOM");
        // Add your logic here
    }

    private void OnDrawGizmos()
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
            Vector3 gizmoEndPos = transform.position;
            if (movementDirection == MovementDirection.Horizontal)
            {
                gizmoEndPos += Vector3.right * moveDistance;
            }
            else if (movementDirection == MovementDirection.Vertical)
            {
                gizmoEndPos += Vector3.up * moveDistance;
            }
            else if (movementDirection == MovementDirection.OneWay)
            {
                gizmoEndPos += Vector3.left * moveDistance;
            }
            Gizmos.DrawLine(transform.position, gizmoEndPos);
            Gizmos.DrawSphere(transform.position, 0.2f);
            Gizmos.DrawSphere(gizmoEndPos, 0.2f);
        }
    }
}

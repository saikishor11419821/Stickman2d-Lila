using UnityEngine;

public class MoveForwardBackward2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardDistance = 3f;
    public float backwardDistance = 2f;
    public float moveSpeed = 2f;

    private Vector3 startPosition;
    private Vector3 forwardPosition;
    private Vector3 backwardPosition;

    private int movementState = 0;

    void Start()
    {
        startPosition = transform.position;

        // Forward = right on X axis
        forwardPosition = startPosition + Vector3.right * forwardDistance;

        // Backward = left on X axis
        backwardPosition = startPosition - Vector3.right * backwardDistance;
    }

    void Update()
    {
        Vector3 targetPosition;

        if (movementState == 0)
        {
            // Move forward
            targetPosition = forwardPosition;
        }
        else
        {
            // Move backward
            targetPosition = backwardPosition;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Reached target
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            movementState++;

            if (movementState > 1)
            {
                movementState = 0;
            }
        }
    }
}
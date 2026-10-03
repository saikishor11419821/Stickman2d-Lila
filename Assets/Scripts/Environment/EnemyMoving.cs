using UnityEngine;

public class EnemyMoving : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardDistance = 3f;
    public float backwardDistance = 2f;
    public float moveSpeed = 2f;

    [Header("Character Facing")]
    public bool facesRightInitially = true;

    private Vector3 startPosition;
    private Vector3 forwardPosition;
    private Vector3 backwardPosition;

    private int movementState = 0;

    private void Start()
    {
        startPosition = transform.position;

        // Forward = right on X axis
        forwardPosition = startPosition + Vector3.right * forwardDistance;

        // Backward = left on X axis
        backwardPosition = startPosition - Vector3.right * backwardDistance;

        // Start facing right
        SetFacing(true);
    }

    private void Update()
    {
        Vector3 targetPosition;

        if (movementState == 0)
        {
            // Move forward
            targetPosition = forwardPosition;

            // Face right
            SetFacing(true);
        }
        else
        {
            // Move backward
            targetPosition = backwardPosition;

            // Face left
            SetFacing(false);
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

    private void SetFacing(bool facingRight)
    {
        Vector3 scale = transform.localScale;

        float absX = Mathf.Abs(scale.x);

        if (facingRight)
        {
            scale.x = facesRightInitially ? absX : -absX;
        }
        else
        {
            scale.x = facesRightInitially ? -absX : absX;
        }

        transform.localScale = scale;
    }
}
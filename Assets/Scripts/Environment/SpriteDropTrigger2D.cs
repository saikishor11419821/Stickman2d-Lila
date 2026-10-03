using UnityEngine;

public class SpriteDropTrigger2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardDistance = 3f;
    public float backwardDistance = 2f;
    public float moveSpeed = 2f;

    [Header("Character Facing")]
    public bool facesRightInitially = true;

    [Header("Sprite Sorting")]
    public SpriteRenderer spriteRenderer;
    public int initialSortingOrder = 3;
    public int triggeredSortingOrder = 1;

    [Header("Drop Settings")]
    public float moveDownDistance = 2f;
    public float dropSpeed = 3f;

    private Vector3 startPosition;
    private Vector3 forwardPosition;
    private Vector3 backwardPosition;
    private Vector3 dropPosition;

    private int movementState = 0;

    private bool triggered = false;
    private bool isDropping = false;

    private void Start()
    {
        startPosition = transform.position;

        // Forward = right
        forwardPosition = startPosition + Vector3.right * forwardDistance;

        // Backward = left
        backwardPosition = startPosition - Vector3.right * backwardDistance;

        // Sorting order initially 3
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = initialSortingOrder;
        }

        // Start facing right
        SetFacing(true);
    }

    private void Update()
    {
        // Once triggered, stop normal movement
        if (triggered)
        {
            HandleDrop();
            return;
        }

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

        // Reached movement target
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;

            movementState++;

            if (movementState > 1)
            {
                movementState = 0;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only Player can activate the trigger
        if (!other.CompareTag("Player"))
            return;

        // Prevent triggering again
        if (triggered)
            return;

        triggered = true;
        isDropping = true;

        // Change sorting order
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = triggeredSortingOrder;
        }

        // Calculate downward position from current position
        dropPosition = transform.position + Vector3.down * moveDownDistance;
    }

    private void HandleDrop()
    {
        if (!isDropping)
            return;

        // Move downward
        transform.position = Vector3.MoveTowards(
            transform.position,
            dropPosition,
            dropSpeed * Time.deltaTime
        );

        // Reached bottom
        if (Vector3.Distance(transform.position, dropPosition) < 0.01f)
        {
            transform.position = dropPosition;

            // Freeze permanently
            isDropping = false;
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
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Follow Settings")]
    public float smoothSpeed = 5f;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("Camera X Boundaries")]
    public float minX = -10f;
    public float maxX = 50f;

    [Header("Jump Zoom (Orthographic Size)")]
    public float normalSize = 5f;
    public float zoomedOutSize = 7f;
    public float zoomSpeed = 4f;

    [Tooltip("Seconds the player must be in the air before the camera zooms out (avoids flicker on tiny hops / slopes).")]
    public float airTimeBeforeZoom = 0.05f;

    private Camera cam;
    private PlayerController2D playerController;
    private float airTime;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = normalSize;
    }

    private void Start()
    {
        if (player != null)
            playerController = player.GetComponent<PlayerController2D>();
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        if (playerController == null)
            playerController = player.GetComponent<PlayerController2D>();

        FollowPlayer();
        UpdateZoom();
    }

    // ---------------------------------------------------------
    // FOLLOW
    // ---------------------------------------------------------

    private void FollowPlayer()
    {
        Vector3 targetPosition = transform.position;

        targetPosition.x = player.position.x + offset.x;
        targetPosition.y = player.position.y + offset.y;
        targetPosition.z = offset.z;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    // ---------------------------------------------------------
    // ZOOM
    // Zoomed out (7) while jumping / in the air, or standing on SkyGround.
    // Back to normal (5) once the player lands on normal ground.
    // ---------------------------------------------------------

    private void UpdateZoom()
    {
        if (playerController == null)
            return;

        if (playerController.IsGrounded)
            airTime = 0f;
        else
            airTime += Time.deltaTime;

        bool inAir = airTime > airTimeBeforeZoom;
        bool onSkyGround = playerController.IsOnSkyGround;

        float targetSize = (inAir || onSkyGround) ? zoomedOutSize : normalSize;

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetSize,
            zoomSpeed * Time.deltaTime
        );
    }
}
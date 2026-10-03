using UnityEngine;
using System.Collections;

public class PlayerTeleportTrigger : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform teleportPoint;

    [Header("Slow Motion Settings")]
    [Range(0.01f, 1f)]
    public float slowMotionSpeed = 0.2f;

    [Tooltip("Slow motion time before teleport")]
    public float slowMotionBeforeTeleport = 0.5f;

    [Tooltip("Slow motion time after teleport")]
    public float slowMotionAfterTeleport = 1.5f;

    private bool used = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used)
            return;

        if (other.CompareTag("Player"))
        {
            used = true;

            StartCoroutine(TeleportSequence(other.transform));
        }
    }

    private IEnumerator TeleportSequence(Transform player)
    {
        // Start slow motion
        Time.timeScale = slowMotionSpeed;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Slow motion BEFORE teleport
        yield return new WaitForSecondsRealtime(slowMotionBeforeTeleport);

        // Teleport player
        if (teleportPoint != null)
        {
            player.position = teleportPoint.position;
        }

        // Continue slow motion AFTER teleport
        yield return new WaitForSecondsRealtime(slowMotionAfterTeleport);

        // Return game to normal speed
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}

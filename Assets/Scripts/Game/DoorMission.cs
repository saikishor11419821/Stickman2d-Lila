using System.Collections;
using UnityEngine;

public class DoorMission : MonoBehaviour
{
    [Header("Door Animation")]
    public Animator doorAnimator;
    public string openAnimationTrigger = "Door";

    [Header("Result Canvas")]
    public LevelResultUI levelResultUI;

    [Header("Result Delay")]
    public float resultCanvasDelay = 2f;

    private bool doorOpened = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryOpenDoor(other);
    }

    // Also check while the player stands in the door,
    // so collecting the last coin near the door still works
    private void OnTriggerStay2D(Collider2D other)
    {
        TryOpenDoor(other);
    }

    private void TryOpenDoor(Collider2D other)
    {
        if (doorOpened)
            return;

        if (!other.CompareTag("Player"))
            return;

        CoinCollector coinCollector = other.GetComponent<CoinCollector>();

        if (coinCollector == null)
        {
            Debug.LogWarning("Player does not have CoinCollector script!");
            return;
        }

        int requiredCoins = coinCollector.GetRequiredCoins();
        int collectedCoins = coinCollector.GetCollectedCoins();

        if (collectedCoins < requiredCoins)
            return; // door stays locked

        StartCoroutine(OpenDoorSequence());
    }

    private IEnumerator OpenDoorSequence()
    {
        doorOpened = true;

        if (doorAnimator != null)
            doorAnimator.SetTrigger(openAnimationTrigger);

        // Realtime so it never depends on Time.timeScale
        yield return new WaitForSecondsRealtime(resultCanvasDelay);

        if (levelResultUI != null)
        {
            levelResultUI.ShowResult();
        }
        else if (GameUIManager.Instance != null)
        {
            // Fallback: go through the manager directly
            GameUIManager.Instance.ShowResult();
        }
        else
        {
            Debug.LogError("LevelResultUI / GameUIManager not found!");
        }
    }
}
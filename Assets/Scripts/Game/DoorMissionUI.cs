using UnityEngine;
using TMPro;

public class DoorMissionUI : MonoBehaviour
{
    [Header("World Space Canvas")]
    public GameObject doorCanvas;

    [Header("Coin UI")]
    public TMP_Text coinText;

    [Header("Lock Icons")]
    public GameObject lockedIcon;
    public GameObject unlockedIcon;

    private CoinCollector coinCollector;
    private bool playerInside = false;

    private void Start()
    {
        // Hide door canvas when game starts
        if (doorCanvas != null)
            doorCanvas.SetActive(false);

        // Start locked
        SetLockedState();
    }

    private void Update()
    {
        if (!playerInside || coinCollector == null)
            return;

        UpdateUI();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        coinCollector = other.GetComponent<CoinCollector>();

        if (coinCollector == null)
        {
            Debug.LogWarning(
                "Player does not have CoinCollector script!"
            );

            return;
        }

        playerInside = true;

        // Show door canvas
        if (doorCanvas != null)
            doorCanvas.SetActive(true);

        UpdateUI();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        coinCollector = null;

        // Hide door canvas
        if (doorCanvas != null)
            doorCanvas.SetActive(false);
    }

    private void UpdateUI()
    {
        if (coinCollector == null)
            return;

        // Get collected coins
        int collected =
            coinCollector.GetCollectedCoins();

        // Get required coins from LevelSettingsManager
        int required;

        if (LevelSettingsManager.Instance != null)
        {
            required =
                LevelSettingsManager.Instance.requiredCoins;
        }
        else
        {
            required =
                coinCollector.GetRequiredCoins();
        }

        // Update coin text
        if (coinText != null)
        {
            coinText.text =
                collected + " / " + required;
        }

        // Check mission
        bool missionCompleted =
            collected >= required;

        if (missionCompleted)
        {
            SetUnlockedState();
        }
        else
        {
            SetLockedState();
        }
    }

    private void SetLockedState()
    {
        if (lockedIcon != null)
            lockedIcon.SetActive(true);

        if (unlockedIcon != null)
            unlockedIcon.SetActive(false);
    }

    private void SetUnlockedState()
    {
        if (lockedIcon != null)
            lockedIcon.SetActive(false);

        if (unlockedIcon != null)
            unlockedIcon.SetActive(true);
    }
}
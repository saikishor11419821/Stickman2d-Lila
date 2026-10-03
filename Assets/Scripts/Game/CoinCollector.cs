using UnityEngine;

public class CoinCollector : MonoBehaviour
{
    [Header("Progress")]
    public int collectedCoins = 0;

    public int RequiredCoins
    {
        get
        {
            if (LevelSettingsManager.Instance != null)
                return LevelSettingsManager.Instance.requiredCoins;

            return 10;
        }
    }

    public bool MissionCompleted
    {
        get
        {
            return collectedCoins >= RequiredCoins;
        }
    }

    private void Start()
    {
        collectedCoins = 0;

        Debug.Log(
            "Coin Target From Settings: " +
            RequiredCoins
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Coin"))
            return;

        collectedCoins++;

        Debug.Log(
            "Coins: " +
            collectedCoins +
            " / " +
            RequiredCoins
        );

        Destroy(other.gameObject);

        if (MissionCompleted)
        {
            Debug.Log(
                "Coin Mission Completed! Door is now unlocked."
            );
        }
    }

    public int GetCollectedCoins()
    {
        return collectedCoins;
    }

    public int GetRequiredCoins()
    {
        return RequiredCoins;
    }
}
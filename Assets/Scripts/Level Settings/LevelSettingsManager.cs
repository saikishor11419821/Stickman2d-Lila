using UnityEngine;

public class LevelSettingsManager : MonoBehaviour
{
    public static LevelSettingsManager Instance;

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    [Header("Current Settings")]
    public int requiredCoins = 10;
    public Difficulty difficulty = Difficulty.Easy;

    public float EnemyDamageMultiplier
    {
        get
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    return 0.5f;

                case Difficulty.Normal:
                    return 1f;

                case Difficulty.Hard:
                    return 2f;
            }

            return 1f;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCoinRequirement(int coins)
    {
        requiredCoins = coins;

        Debug.Log(
            "Coin Requirement Changed To: " +
            requiredCoins
        );
    }

    public void SetDifficulty(Difficulty newDifficulty)
    {
        difficulty = newDifficulty;

        Debug.Log(
            "Difficulty Changed To: " +
            difficulty
        );
    }

    public string GetDifficultyName()
    {
        return difficulty.ToString();
    }
}
using UnityEngine;

/// <summary>
/// Persistent singleton that tracks level progress and saves/loads it
/// using PlayerPrefs. Place this on an empty GameObject in your VERY FIRST
/// scene (or in every scene — it will destroy duplicates automatically).
/// Levels are identified by simple integers: 1, 2, 3, ...
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private const string UNLOCKED_LEVEL_KEY = "HighestUnlockedLevel";

    [Tooltip("Which level number is unlocked from the very start (usually 1).")]
    [SerializeField] private int firstUnlockedLevel = 1;

    // The highest level number the player has unlocked so far.
    public int HighestUnlockedLevel { get; private set; }

    private void Awake()
    {
        // Singleton pattern: keep one instance alive across all scenes.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadProgress();
    }

    /// <summary>Reads saved progress from PlayerPrefs.</summary>
    private void LoadProgress()
    {
        HighestUnlockedLevel = PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, firstUnlockedLevel);
    }

    /// <summary>Writes current progress to disk immediately.</summary>
    private void SaveProgress()
    {
        PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, HighestUnlockedLevel);
        PlayerPrefs.Save();
    }

    /// <summary>Is this level number allowed to be played?</summary>
    public bool IsLevelUnlocked(int levelNumber)
    {
        return levelNumber <= HighestUnlockedLevel;
    }

    /// <summary>Unlocks a specific level number (and saves), if not already unlocked.</summary>
    public void UnlockLevel(int levelNumber)
    {
        if (levelNumber > HighestUnlockedLevel)
        {
            HighestUnlockedLevel = levelNumber;
            SaveProgress();
        }
    }

    /// <summary>
    /// Call this when the player finishes a level. It unlocks the next one.
    /// e.g. CompleteLevel(3) unlocks level 4.
    /// </summary>
    public void CompleteLevel(int completedLevelNumber)
    {
        UnlockLevel(completedLevelNumber + 1);
    }

    /// <summary>Optional: wipe all progress (handy for testing / a "Reset" button).</summary>
    public void ResetProgress()
    {
        HighestUnlockedLevel = firstUnlockedLevel;
        PlayerPrefs.DeleteKey(UNLOCKED_LEVEL_KEY);
        PlayerPrefs.Save();
    }
}

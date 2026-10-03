using UnityEngine;
using TMPro;

public class LevelResultUI : MonoBehaviour
{
    [Header("Result Canvas")]
    public GameObject resultCanvas;

    [Header("Result Text")]
    public TMP_Text levelTypeText;
    public TMP_Text coinsText;
    public TMP_Text timeText;
    public TMP_Text bestTimeText;
    public TMP_Text newBestText;

    private bool resultShown = false;

    private void Start()
    {
        if (resultCanvas != null)
            resultCanvas.SetActive(false);

        if (newBestText != null)
            newBestText.gameObject.SetActive(false);
    }

    public void ShowResult()
    {
        // Prevent showing result twice
        if (resultShown)
            return;

        if (LevelSettingsManager.Instance == null)
        {
            Debug.LogError(
                "LevelSettingsManager not found!"
            );

            return;
        }

        if (LevelTimer.Instance == null)
        {
            Debug.LogError(
                "LevelTimer not found!"
            );

            return;
        }

        resultShown = true;

        // =========================================
        // STOP TIMER
        // =========================================

        LevelTimer.Instance.StopTimer();

        float currentTime =
            LevelTimer.Instance.ElapsedTime;

        // =========================================
        // GET SETTINGS
        // =========================================

        LevelSettingsManager.Difficulty difficulty =
            LevelSettingsManager.Instance.difficulty;

        int requiredCoins =
            LevelSettingsManager.Instance.requiredCoins;

        // =========================================
        // GET OLD BEST TIME
        // =========================================

        float oldBest =
            LevelBestTimeManager.GetBestTime(
                difficulty
            );

        bool isNewBest =
            oldBest < 0f ||
            currentTime < oldBest;

        // =========================================
        // SAVE NEW BEST
        // =========================================

        if (isNewBest)
        {
            LevelBestTimeManager.SaveBestTime(
                difficulty,
                currentTime
            );
        }

        // =========================================
        // DIFFICULTY
        // =========================================

        if (levelTypeText != null)
        {
            levelTypeText.text =
                "Level Type: " +
                difficulty.ToString();
        }

        // =========================================
        // COINS
        // =========================================

        if (coinsText != null)
        {
            coinsText.text =
                "Coins Required: " +
                requiredCoins;
        }

        // =========================================
        // CURRENT TIME
        // =========================================

        if (timeText != null)
        {
            timeText.text =
                "Time: " +
                FormatTime(currentTime);
        }

        // =========================================
        // BEST TIME
        // =========================================

        float bestTime =
            LevelBestTimeManager.GetBestTime(
                difficulty
            );

        if (bestTimeText != null)
        {
            bestTimeText.text =
                "Best Time: " +
                FormatTime(bestTime);
        }

        // =========================================
        // NEW BEST
        // =========================================

        if (newBestText != null)
        {
            newBestText.gameObject.SetActive(
                isNewBest
            );

            if (isNewBest)
            {
                newBestText.text =
                    "NEW BEST TIME!";
            }
        }

        // =========================================
        // SHOW RESULT THROUGH GAME UI MANAGER
        // =========================================

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowResult();
        }
        else
        {
            // Fallback
            if (resultCanvas != null)
                resultCanvas.SetActive(true);

            Time.timeScale = 0f;
        }

        Debug.Log(
            "Level completed! " +
            "Difficulty: " +
            difficulty +
            " | Time: " +
            FormatTime(currentTime)
        );
    }

    private string FormatTime(float time)
    {
        int minutes =
            Mathf.FloorToInt(time / 60f);

        int seconds =
            Mathf.FloorToInt(time % 60f);

        return string.Format(
            "{0:00}:{1:00}",
            minutes,
            seconds
        );
    }
}
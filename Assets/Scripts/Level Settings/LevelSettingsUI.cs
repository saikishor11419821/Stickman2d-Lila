using UnityEngine;
using TMPro;

public class LevelSettingsUI : MonoBehaviour
{
    [Header("Settings Canvas")]
    public GameObject settingsCanvas;

    [Header("Navigation Canvases")]
    public GameObject homeCanvas;
    public GameObject resultCanvas;

    [Header("Selected Settings Text")]
    public TMP_Text coinsText;
    public TMP_Text difficultyText;

    private void Start()
    {
        if (LevelSettingsManager.Instance == null)
        {
            Debug.LogError(
                "LevelSettingsManager not found!"
            );

            return;
        }

        // Hide settings when scene starts
        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);

        UpdateUI();
    }

    // ==================================================
    // OPEN SETTINGS
    // ==================================================

    // Use this for the Settings button on Home Canvas
    public void OpenSettingsFromHome()
    {
        // Hide Home
        if (homeCanvas != null)
            homeCanvas.SetActive(false);

        // Hide Result just in case
        if (resultCanvas != null)
            resultCanvas.SetActive(false);

        // Show Settings
        if (settingsCanvas != null)
            settingsCanvas.SetActive(true);

        UpdateUI();
    }

    // Use this for the Settings button on Result Canvas
    public void OpenSettingsFromResult()
    {
        // Hide Home
        if (homeCanvas != null)
            homeCanvas.SetActive(false);

        // Hide Result
        if (resultCanvas != null)
            resultCanvas.SetActive(false);

        // Show Settings
        if (settingsCanvas != null)
            settingsCanvas.SetActive(true);

        UpdateUI();
    }

    // ==================================================
    // CLOSE SETTINGS
    // ==================================================

    // Return to Home
    public void CloseSettingsToHome()
    {
        // Hide Settings
        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);

        // Show Home
        if (homeCanvas != null)
            homeCanvas.SetActive(true);

        // Make sure Result is hidden
        if (resultCanvas != null)
            resultCanvas.SetActive(false);
    }

    // Return to Result
    public void CloseSettingsToResult()
    {
        // Hide Settings
        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);

        // Show Result
        if (resultCanvas != null)
            resultCanvas.SetActive(true);

        // Make sure Home is hidden
        if (homeCanvas != null)
            homeCanvas.SetActive(false);
    }

    // ==================================================
    // COIN SETTINGS
    // ==================================================

    public void Set10Coins()
    {
        if (LevelSettingsManager.Instance == null)
            return;

        LevelSettingsManager.Instance.SetCoinRequirement(10);

        UpdateUI();

        Debug.Log(
            "Coin requirement set to 10"
        );
    }

    public void Set20Coins()
    {
        if (LevelSettingsManager.Instance == null)
            return;

        LevelSettingsManager.Instance.SetCoinRequirement(20);

        UpdateUI();

        Debug.Log(
            "Coin requirement set to 20"
        );
    }

    public void Set30Coins()
    {
        if (LevelSettingsManager.Instance == null)
            return;

        LevelSettingsManager.Instance.SetCoinRequirement(30);

        UpdateUI();

        Debug.Log(
            "Coin requirement set to 30"
        );
    }

    // ==================================================
    // DIFFICULTY SETTINGS
    // ==================================================

    public void SetEasy()
    {
        if (LevelSettingsManager.Instance == null)
            return;

        LevelSettingsManager.Instance.SetDifficulty(
            LevelSettingsManager.Difficulty.Easy
        );

        UpdateUI();

        Debug.Log(
            "Difficulty set to Easy"
        );
    }

    public void SetNormal()
    {
        if (LevelSettingsManager.Instance == null)
            return;

        LevelSettingsManager.Instance.SetDifficulty(
            LevelSettingsManager.Difficulty.Normal
        );

        UpdateUI();

        Debug.Log(
            "Difficulty set to Normal"
        );
    }

    public void SetHard()
    {
        if (LevelSettingsManager.Instance == null)
            return;

        LevelSettingsManager.Instance.SetDifficulty(
            LevelSettingsManager.Difficulty.Hard
        );

        UpdateUI();

        Debug.Log(
            "Difficulty set to Hard"
        );
    }

    // ==================================================
    // UPDATE SETTINGS DISPLAY
    // ==================================================

    private void UpdateUI()
    {
        if (LevelSettingsManager.Instance == null)
            return;

        // Coins
        if (coinsText != null)
        {
            coinsText.text =
                "Coins Required: " +
                LevelSettingsManager.Instance.requiredCoins;
        }

        // Difficulty
        if (difficultyText != null)
        {
            difficultyText.text =
                "Difficulty: " +
                LevelSettingsManager.Instance.GetDifficultyName();
        }
    }
}
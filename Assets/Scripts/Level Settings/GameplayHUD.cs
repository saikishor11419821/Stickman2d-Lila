using UnityEngine;
using TMPro;

public class GameplayHUD : MonoBehaviour
{
    [Header("HUD Text")]
    public TMP_Text coinText;
    public TMP_Text timerText;

    [Header("Player")]
    public CoinCollector coinCollector;

    private void Start()
    {
        // Automatically find CoinCollector if not assigned
        if (coinCollector == null)
        {
            coinCollector =
                FindObjectOfType<CoinCollector>();
        }

        UpdateCoinUI();
        UpdateTimerUI();
    }

    private void Update()
    {
        UpdateCoinUI();
        UpdateTimerUI();
    }

    private void UpdateCoinUI()
    {
        if (coinText == null || coinCollector == null)
            return;

        int collected =
            coinCollector.GetCollectedCoins();

        int required =
            coinCollector.GetRequiredCoins();

        coinText.text =
            collected + " / " + required;
    }

    private void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        if (LevelTimer.Instance == null)
            return;

        timerText.text =
            LevelTimer.Instance.GetFormattedTime();
    }
}
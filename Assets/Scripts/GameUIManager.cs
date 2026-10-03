using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Main Canvases")]
    public GameObject homeCanvas;
    public GameObject gameCanvas;
    public GameObject resultCanvas;
    public GameObject levelSettingsCanvas;

    // Survives scene reloads (static), so Restart can jump straight into the game
    private static bool autoStartAfterReload = false;

    // Where to return when Level Settings is closed (Home or Result)
    private GameObject canvasBeforeSettings;

    public bool GameInProgress { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        // PlayerHealth sets this PlayerPrefs flag when the player dies
        bool restartFromDeath = PlayerPrefs.GetInt("RestartGame", 0) == 1;

        if (restartFromDeath)
        {
            PlayerPrefs.SetInt("RestartGame", 0);
            PlayerPrefs.Save();
        }

        if (autoStartAfterReload || restartFromDeath)
        {
            autoStartAfterReload = false;
            PlayGame();
        }
        else
        {
            ShowHome();
        }
    }

    // =========================================
    // CORE: show exactly one canvas
    // =========================================

    private void ShowOnly(GameObject target)
    {
        bool showSettings = levelSettingsCanvas != null && target == levelSettingsCanvas;

        // If Level Settings is a CHILD of the Home canvas, the parent must stay
        // active or the child can never be visible
        bool settingsIsChildOfHome =
            levelSettingsCanvas != null &&
            homeCanvas != null &&
            levelSettingsCanvas.transform.IsChildOf(homeCanvas.transform);

        SetActiveSafe(homeCanvas, homeCanvas == target || (showSettings && settingsIsChildOfHome));
        SetActiveSafe(gameCanvas, gameCanvas == target);
        SetActiveSafe(resultCanvas, resultCanvas == target);
        SetActiveSafe(levelSettingsCanvas, showSettings);
    }

    private void SetActiveSafe(GameObject obj, bool active)
    {
        if (obj != null)
            obj.SetActive(active);
    }

    // =========================================
    // PLAY GAME  (Home -> Game)
    // =========================================

    public void PlayGame()
    {
        ShowGame();

        if (LevelTimer.Instance != null)
            LevelTimer.Instance.StartTimer();
    }

    // =========================================
    // SHOW HOME
    // =========================================

    public void ShowHome()
    {
        ShowOnly(homeCanvas);
        Time.timeScale = 0f;
        GameInProgress = false;
    }

    // =========================================
    // SHOW GAME
    // =========================================

    public void ShowGame()
    {
        ShowOnly(gameCanvas);
        Time.timeScale = 1f;
        GameInProgress = true;
    }

    // =========================================
    // SHOW RESULT
    // =========================================

    public void ShowResult()
    {
        ShowOnly(resultCanvas);
        Time.timeScale = 0f;
        GameInProgress = false;
    }

    // =========================================
    // LEVEL SETTINGS  (works from Home or Result)
    // =========================================

    public void OpenLevelSettings()
    {
        if (levelSettingsCanvas == null)
        {
            Debug.LogError("Level Settings Canvas is not assigned in GameUIManager!");
            return;
        }

        // Remember which canvas we came from
        if (resultCanvas != null && resultCanvas.activeSelf)
            canvasBeforeSettings = resultCanvas;
        else
            canvasBeforeSettings = homeCanvas;

        ShowOnly(levelSettingsCanvas);
        Time.timeScale = 0f;
        GameInProgress = false;
    }

    public void CloseLevelSettings()
    {
        ShowOnly(canvasBeforeSettings != null ? canvasBeforeSettings : homeCanvas);
        Time.timeScale = 0f;
    }

    // =========================================
    // HOME BUTTON  (reload scene so everything is reset,
    // then land on Home)
    // =========================================

    public void GoHome()
    {
        autoStartAfterReload = false;
        ReloadScene();
    }

    // =========================================
    // RESTART  (reload scene and start playing immediately)
    // =========================================

    public void RestartGame()
    {
        autoStartAfterReload = true;
        ReloadScene();
    }

    private void ReloadScene()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================
    // QUIT
    // =========================================

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer Instance;

    private float elapsedTime;
    private bool timerRunning = false;

    public float ElapsedTime
    {
        get { return elapsedTime; }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartTimer();
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        timerRunning = true;
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        elapsedTime += Time.deltaTime;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        return string.Format(
            "{0:00}:{1:00}",
            minutes,
            seconds
        );
    }
}
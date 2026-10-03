using UnityEngine;

public static class LevelBestTimeManager
{
    public static float GetBestTime(
        LevelSettingsManager.Difficulty difficulty)
    {
        string key = GetKey(difficulty);

        return PlayerPrefs.GetFloat(key, -1f);
    }

    public static bool SaveBestTime(
        LevelSettingsManager.Difficulty difficulty,
        float time)
    {
        string key = GetKey(difficulty);

        float oldBest = PlayerPrefs.GetFloat(key, -1f);

        // No previous record
        if (oldBest < 0f)
        {
            PlayerPrefs.SetFloat(key, time);
            PlayerPrefs.Save();

            return true;
        }

        // New faster record
        if (time < oldBest)
        {
            PlayerPrefs.SetFloat(key, time);
            PlayerPrefs.Save();

            return true;
        }

        return false;
    }

    private static string GetKey(
        LevelSettingsManager.Difficulty difficulty)
    {
        return "BestTime_" + difficulty.ToString();
    }
}
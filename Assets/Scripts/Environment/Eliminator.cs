using UnityEngine;
using UnityEngine.SceneManagement;

public class Eliminator : MonoBehaviour
{
    private bool isRestarting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isRestarting)
        {
            isRestarting = true;

            // Make sure the game is running
            Time.timeScale = 1f;

            // Tell GameUIManager to skip Home Canvas
            PlayerPrefs.SetInt("RestartGame", 1);
            PlayerPrefs.Save();

            // Restart the current scene
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }
    }
}
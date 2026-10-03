using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;

    private int currentHealth;
    private bool isDead;

    [Header("Hit Protection")]
    [Tooltip("Seconds of invincibility after taking a hit (stops instant multi-hit deaths).")]
    public float invincibilityTime = 0.5f;

    private float invincibleUntil;

    [Header("Player Controller")]
    public PlayerController2D playerController;

    [Header("Death")]
    public bool disablePlayerOnDeath = true;
    public string deathAnimationName = "Death";
    public float destroyDelay = 2f;

    [Header("Restart")]
    public float restartDelay = 0f;

    private Animator animator;
    private bool isRestarting;

    private void Awake()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();

        if (playerController == null)
            playerController = GetComponent<PlayerController2D>();
    }

    // =========================================================
    // TAKE DAMAGE
    // =========================================================

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position);
    }

    public void TakeDamage(int damage, Vector2 sourcePosition)
    {
        if (isDead || isRestarting)
            return;

        if (Time.time < invincibleUntil)
            return;

        invincibleUntil = Time.time + invincibilityTime;

        currentHealth = Mathf.Max(currentHealth - damage, 0);

        Debug.Log("Player took " + damage +
                  " damage. Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (playerController != null)
            playerController.OnHurt(sourcePosition);
    }

    // =========================================================
    // DEATH
    // =========================================================

    private void Die()
    {
        if (isDead || isRestarting)
            return;

        isDead = true;

        Debug.Log("Player Died! Restarting Game...");

        // Disable player controls
        if (disablePlayerOnDeath && playerController != null)
            playerController.OnDeath();

        // Play death animation if available
        if (animator != null &&
            animator.HasState(
                0,
                Animator.StringToHash(deathAnimationName)))
        {
            animator.Play(deathAnimationName);
        }

        // Restart the game
        Invoke(nameof(RestartGame), restartDelay);
    }

    // =========================================================
    // RESTART GAME
    // =========================================================

    private void RestartGame()
    {
        if (isRestarting)
            return;

        isRestarting = true;

        // Make sure game is running
        Time.timeScale = 1f;

        // Tell GameUIManager to skip Home Canvas
        PlayerPrefs.SetInt("RestartGame", 1);
        PlayerPrefs.Save();

        // Reload current scene
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================================
    // HEAL
    // =========================================================

    public void Heal(int amount)
    {
        if (isDead || isRestarting)
            return;

        currentHealth = Mathf.Min(
            currentHealth + amount,
            maxHealth
        );

        Debug.Log(
            "Player healed. Health: " + currentHealth
        );
    }

    // =========================================================
    // HEALTH INFORMATION
    // =========================================================

    public int GetHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    // =========================================================
    // RESET HEALTH
    // =========================================================

    public void ResetHealth()
    {
        isDead = false;
        isRestarting = false;

        currentHealth = maxHealth;
        invincibleUntil = 0f;

        if (playerController != null)
            playerController.Revive();
    }
}
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public float destroyDelay = 1f;

    [Header("UI")]
    public EnemyHealthBar healthBar;

    private int currentHealth;
    private bool isDead;

    private EnemyAI2D ai;

    private void Awake()
    {
        currentHealth = maxHealth;
        ai = GetComponentInParent<EnemyAI2D>();

        if (healthBar == null)
            healthBar = GetComponentInChildren<EnemyHealthBar>(true);
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position);
    }

    public void TakeDamage(int damage, Vector2 sourcePosition)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log(gameObject.name + " took " + damage + " damage. HP: " + currentHealth);

        if (healthBar != null)
            healthBar.SetHealth(Mathf.Max(currentHealth, 0), maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (ai != null)
            ai.OnHurt(sourcePosition);
    }

    private void Die()
    {
        isDead = true;
        currentHealth = 0;

        Debug.Log(gameObject.name + " Died!");

        if (healthBar != null)
            healthBar.Hide();

        if (ai != null)
            ai.OnDeath();

        Destroy(gameObject, destroyDelay);
    }

    public bool IsDead()
    {
        return isDead;
    }
}
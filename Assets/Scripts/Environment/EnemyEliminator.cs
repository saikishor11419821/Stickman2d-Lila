using UnityEngine;

public class EnemyEliminator : MonoBehaviour
{
    [Header("Normal Damage")]
    public int damageAmount = 50;

    private bool hasDamagedPlayer = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (hasDamagedPlayer)
            return;

        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            int finalDamage = GetDifficultyDamage();

            playerHealth.TakeDamage(finalDamage);

            Debug.Log(
                "Eliminator damaged player for " +
                finalDamage +
                " damage."
            );

            hasDamagedPlayer = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        hasDamagedPlayer = false;
    }

    private int GetDifficultyDamage()
    {
        if (LevelSettingsManager.Instance == null)
            return damageAmount;

        return Mathf.RoundToInt(
            damageAmount *
            LevelSettingsManager.Instance.EnemyDamageMultiplier
        );
    }
}
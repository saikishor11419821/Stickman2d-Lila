using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("Attack Hitbox")]
    public BoxCollider2D attackCollider;

    [Header("Normal Damage")]
    public int damage = 10;

    private bool attackActive;
    private bool playerAlreadyHit;

    private ContactFilter2D filter;
    private readonly Collider2D[] results = new Collider2D[8];
    private Transform attacker;

    private void Awake()
    {
        filter = new ContactFilter2D();
        filter.NoFilter();
        filter.useTriggers = false;

        EnemyAI2D ai = GetComponentInParent<EnemyAI2D>();
        attacker = ai != null ? ai.transform : transform;

        if (attackCollider != null)
            attackCollider.isTrigger = true;

        StopAttack();
    }

    public void StartAttack()
    {
        attackActive = true;
        playerAlreadyHit = false;

        if (attackCollider != null)
            attackCollider.enabled = true;
    }

    public void StopAttack()
    {
        attackActive = false;

        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    private void FixedUpdate()
    {
        if (!attackActive || playerAlreadyHit || attackCollider == null)
            return;

        int count = attackCollider.OverlapCollider(filter, results);

        for (int i = 0; i < count; i++)
        {
            PlayerHealth playerHealth =
                results[i].GetComponentInParent<PlayerHealth>();

            if (playerHealth == null || playerHealth.IsDead())
                continue;

            playerAlreadyHit = true;

            int finalDamage = GetDifficultyDamage();

            playerHealth.TakeDamage(
                finalDamage,
                attacker.position
            );

            Debug.Log(
                "Enemy hit player for " +
                finalDamage +
                " damage."
            );

            break;
        }
    }

    private int GetDifficultyDamage()
    {
        if (LevelSettingsManager.Instance == null)
            return damage;

        return Mathf.RoundToInt(
            damage *
            LevelSettingsManager.Instance.EnemyDamageMultiplier
        );
    }
}
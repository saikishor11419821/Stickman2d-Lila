using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Attack Hitboxes")]
    public BoxCollider2D attackCollider1;
    public BoxCollider2D attackCollider2;

    [Header("Damage")]
    public int damage = 10;

    [Header("Timing")]
    [Tooltip("Delay after pressing attack before the hit can land (matches the sword swing).")]
    public float hitDelay = 0.1f;

    [Tooltip("How long the hitbox stays active.")]
    public float hitboxDuration = 0.25f;

    private bool attackActive;     // whole attack in progress
    private bool hitWindowOpen;    // hitbox currently able to damage

    private readonly HashSet<EnemyHealth> enemiesHit = new HashSet<EnemyHealth>();
    private readonly Collider2D[] results = new Collider2D[16];

    private ContactFilter2D filter;
    private Transform attacker;

    private void Awake()
    {
        // Only solid enemy bodies count. This ignores the enemy's big
        // detection trigger, which used to get hit from far away.
        filter = new ContactFilter2D();
        filter.NoFilter();
        filter.useTriggers = false;

        PlayerController2D pc = GetComponentInParent<PlayerController2D>();
        attacker = pc != null ? pc.transform : transform;

        if (attackCollider1 != null) attackCollider1.isTrigger = true;
        if (attackCollider2 != null) attackCollider2.isTrigger = true;

        DisableHitboxes();
    }

    // Call this when player attacks
    public void StartAttackHitbox()
    {
        if (attackActive)
            return;

        attackActive = true;
        hitWindowOpen = false;

        enemiesHit.Clear();

        CancelInvoke(nameof(OpenHitWindow));
        CancelInvoke(nameof(StopAttackHitbox));

        Invoke(nameof(OpenHitWindow), hitDelay);
        Invoke(nameof(StopAttackHitbox), hitDelay + hitboxDuration);
    }

    private void OpenHitWindow()
    {
        if (!attackActive)
            return;

        hitWindowOpen = true;
        EnableHitboxes();
    }

    public void StopAttackHitbox()
    {
        attackActive = false;
        hitWindowOpen = false;

        CancelInvoke(nameof(OpenHitWindow));
        CancelInvoke(nameof(StopAttackHitbox));

        DisableHitboxes();
    }

    private void FixedUpdate()
    {
        if (!hitWindowOpen)
            return;

        CheckCollider(attackCollider1);
        CheckCollider(attackCollider2);
    }

    private void CheckCollider(BoxCollider2D box)
    {
        if (box == null || !box.enabled)
            return;

        int count = box.OverlapCollider(filter, results);

        for (int i = 0; i < count; i++)
        {
            EnemyHealth enemy = results[i].GetComponentInParent<EnemyHealth>();

            if (enemy == null || enemy.IsDead())
                continue;

            // Don't hit the same enemy twice during one attack
            if (!enemiesHit.Add(enemy))
                continue;

            enemy.TakeDamage(damage, attacker.position);

            Debug.Log("Hit enemy for " + damage + " damage");
        }
    }

    private void EnableHitboxes()
    {
        if (attackCollider1 != null) attackCollider1.enabled = true;
        if (attackCollider2 != null) attackCollider2.enabled = true;
    }

    private void DisableHitboxes()
    {
        if (attackCollider1 != null) attackCollider1.enabled = false;
        if (attackCollider2 != null) attackCollider2.enabled = false;
    }
}
using System.Collections;
using UnityEngine;

public class EnemyAI2D : MonoBehaviour
{
    private enum State { Free, Attacking, Hurt, Dead }

    [Header("Player Detection")]
    public Transform player;

    [Tooltip("Optional. If assigned, its bounds are the detection zone. If empty, Detection Range is used.")]
    public BoxCollider2D detectionCollider;
    public float detectionRange = 8f;

    [Tooltip("Extra room once the enemy has spotted you, so it doesn't flicker between chase/patrol at the edge.")]
    public float loseSightExtra = 2f;

    [Tooltip("Max height difference for detection (range mode) and for landing attacks.")]
    public float verticalTolerance = 2f;

    [Header("Patrol")]
    public float patrolSpeed = 2f;
    public Transform leftPoint;
    public Transform rightPoint;
    public float patrolWaitTime = 1f;

    [Header("Chase")]
    public float chaseSpeed = 3.5f;

    [Header("Melee Attack (close range)")]
    public float attackDistance = 1.5f;
    public float attackCooldown = 1f;

    [Tooltip("When (0-1 of the animation) the hitbox turns ON. This is the enemy's windup - the player can dodge before it.")]
    [Range(0f, 1f)] public float hitStartNormalized = 0.4f;

    [Tooltip("When (0-1 of the animation) the hitbox turns OFF.")]
    [Range(0f, 1f)] public float hitEndNormalized = 0.7f;

    [Header("Gun (mid range)")]
    public GameObject bulletPrefab;
    [Tooltip("Child object at the gun muzzle. If empty, the bullet spawns at the enemy position.")]
    public Transform firePoint;

    [Tooltip("The enemy shoots when the player is farther than Min Shoot Distance but within this range.")]
    public float shootRange = 6f;

    [Tooltip("Enemy will NOT shoot if the player is closer than this. Keep it >= Attack Distance.")]
    public float minShootDistance = 3f;

    public float shootCooldown = 2f;

    [Tooltip("When (0-1 of the fire animation) the bullet leaves the gun. This is the windup the player can react to.")]
    [Range(0f, 1f)] public float fireSpawnNormalized = 0.5f;

    [Header("Attack Animations (melee - one is picked at random)")]
    public AnimationClip attack1Animation;
    public AnimationClip attack2Animation;

    [Header("Gun Animation")]
    public AnimationClip fireAnimation;

    [Header("Other Animations")]
    public AnimationClip walkAnimation;
    public AnimationClip idleAnimation;
    public AnimationClip hurtAnimation;   // optional
    public AnimationClip deathAnimation;  // optional

    [Header("Attack Hitbox")]
    public EnemyAttackHitbox attackHitbox;

    [Header("Hurt")]
    public float hurtStunTime = 0.35f;
    public float knockbackSpeed = 4f;
    public float knockbackDecay = 12f;

    [Header("Sprite")]
    [Tooltip("Untick if your enemy artwork faces LEFT by default.")]
    public bool spriteFacesRight = true;

    private Animator animator;
    private Rigidbody2D rb;
    private PlayerHealth playerHealth;

    private State state = State.Free;
    private Coroutine actionRoutine;

    private bool movingRight = true;
    private bool isChasing;

    private float attackTimer;
    private float shootTimer;
    private float patrolWaitTimer;
    private float knockbackX;
    private float findPlayerTimer;

    private bool UsesPhysics
    {
        get { return rb != null && rb.bodyType == RigidbodyType2D.Dynamic; }
    }

    private bool CanShoot
    {
        get { return fireAnimation != null && bulletPrefab != null; }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.freezeRotation = true;

        if (detectionCollider != null)
            detectionCollider.isTrigger = true;
    }

    private void Start()
    {
        AcquirePlayer();

        if (attackHitbox != null)
            attackHitbox.StopAttack();
    }

    private void Update()
    {
        if (state == State.Dead)
            return;

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (shootTimer > 0f)
            shootTimer -= Time.deltaTime;

        // Player may have respawned / been destroyed
        if (player == null)
        {
            findPlayerTimer -= Time.deltaTime;

            if (findPlayerTimer <= 0f)
            {
                findPlayerTimer = 0.5f;
                AcquirePlayer();
            }
        }

        switch (state)
        {
            case State.Attacking:
                StopMoving();
                return;

            case State.Hurt:
                ApplyKnockback();
                return;
        }

        if (CanSeePlayer())
        {
            isChasing = true;
            HandleCombat();
        }
        else
        {
            isChasing = false;
            HandlePatrol();
        }
    }

    // =========================================================
    // PLAYER DETECTION
    // =========================================================

    private void AcquirePlayer()
    {
        if (player == null)
        {
            GameObject go = GameObject.FindWithTag("Player");

            if (go != null)
                player = go.transform;
        }

        if (player != null && playerHealth == null)
            playerHealth = player.GetComponentInParent<PlayerHealth>();
    }

    private bool CanSeePlayer()
    {
        if (player == null)
            return false;

        if (playerHealth != null && playerHealth.IsDead())
            return false;

        if (detectionCollider != null)
        {
            Bounds b = detectionCollider.bounds;

            if (isChasing)
                b.Expand(loseSightExtra);

            Vector3 p = player.position;
            p.z = b.center.z;

            return b.Contains(p);
        }

        float range = isChasing ? detectionRange + loseSightExtra : detectionRange;

        Vector2 d = player.position - transform.position;

        return Mathf.Abs(d.x) <= range && Mathf.Abs(d.y) <= verticalTolerance;
    }

    // =========================================================
    // COMBAT
    // =========================================================

    private void HandleCombat()
    {
        float dx = player.position.x - transform.position.x;
        float dy = Mathf.Abs(player.position.y - transform.position.y);
        float dist = Mathf.Abs(dx);

        Face(dx);

        bool sameHeight = dy <= verticalTolerance;

        // ---- Close range: melee (random attack) ----
        if (dist <= attackDistance && sameHeight)
        {
            StopMoving();

            if (attackTimer <= 0f)
                actionRoutine = StartCoroutine(AttackRoutine());
            else
                PlayAnimation(idleAnimation);

            return;
        }

        // ---- Mid range: stop and shoot (only when the player is NOT close) ----
        if (CanShoot && sameHeight && dist >= minShootDistance && dist <= shootRange && shootTimer <= 0f)
        {
            StopMoving();
            actionRoutine = StartCoroutine(ShootRoutine());
            return;
        }

        // ---- Otherwise walk toward the player ----
        if (dist > 0.1f)
        {
            Move(Mathf.Sign(dx), chaseSpeed);
            PlayAnimation(walkAnimation);
        }
        else
        {
            StopMoving();
            PlayAnimation(idleAnimation);
        }
    }

    private IEnumerator AttackRoutine()
    {
        state = State.Attacking;

        StopMoving();

        if (player != null)
            Face(player.position.x - transform.position.x);

        AnimationClip clip = PickAttack();

        if (clip == null)
        {
            Debug.LogWarning("Enemy melee attack animation is not assigned!");
            attackTimer = attackCooldown;
            state = State.Free;
            actionRoutine = null;
            yield break;
        }

        animator.Play(clip.name, 0, 0f);

        float length = clip.length;
        float hitStart = length * Mathf.Min(hitStartNormalized, hitEndNormalized);
        float hitEnd = length * Mathf.Max(hitStartNormalized, hitEndNormalized);

        // 1) Windup (harmless, player can dodge / hit the enemy)
        yield return new WaitForSeconds(hitStart);

        // 2) Active frames
        if (attackHitbox != null)
            attackHitbox.StartAttack();

        yield return new WaitForSeconds(hitEnd - hitStart);

        if (attackHitbox != null)
            attackHitbox.StopAttack();

        // 3) Recovery
        yield return new WaitForSeconds(length - hitEnd);

        attackTimer = attackCooldown;   // cooldown starts AFTER the attack ends
        state = State.Free;
        actionRoutine = null;
    }

    private IEnumerator ShootRoutine()
    {
        state = State.Attacking;

        StopMoving();

        if (player != null)
            Face(player.position.x - transform.position.x);

        animator.Play(fireAnimation.name, 0, 0f);

        float length = fireAnimation.length;
        float spawnTime = length * fireSpawnNormalized;

        // 1) Windup (player can react)
        yield return new WaitForSeconds(spawnTime);

        // 2) Bullet leaves the gun (skip it if the player got too close during the windup)
        bool tooClose = player != null &&
                        Mathf.Abs(player.position.x - transform.position.x) < minShootDistance;

        if (!tooClose)
            SpawnBullet();

        // 3) Recovery
        yield return new WaitForSeconds(Mathf.Max(0f, length - spawnTime));

        shootTimer = shootCooldown;                        // cooldown starts AFTER the shot ends
        attackTimer = Mathf.Max(attackTimer, 0.3f);        // tiny gap before a melee follow-up
        state = State.Free;
        actionRoutine = null;
    }

    private void SpawnBullet()
    {
        Vector3 pos = firePoint != null ? firePoint.position : transform.position;

        GameObject go = Instantiate(bulletPrefab, pos, Quaternion.identity);

        Bullet2D bullet = go.GetComponent<Bullet2D>();

        if (bullet != null)
            bullet.Init(FacingDirection(), false);
    }

    private AnimationClip PickAttack()
    {
        AnimationClip[] clips = { attack1Animation, attack2Animation };

        int start = Random.Range(0, clips.Length);

        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip c = clips[(start + i) % clips.Length];

            if (c != null)
                return c;
        }

        return null;
    }

    // =========================================================
    // HURT / DEATH (called by EnemyHealth)
    // =========================================================

    public void OnHurt(Vector2 sourcePosition)
    {
        if (state == State.Dead)
            return;

        CancelAction();

        state = State.Hurt;
        isChasing = true;   // it knows where you are now

        float dir;

        if (Mathf.Approximately(sourcePosition.x, transform.position.x))
            dir = -FacingDirection();
        else
            dir = transform.position.x >= sourcePosition.x ? 1f : -1f;

        knockbackX = dir * knockbackSpeed;

        AnimationClip clip = hurtAnimation != null ? hurtAnimation : idleAnimation;

        if (clip != null)
            animator.Play(clip.name, 0, 0f);

        actionRoutine = StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        yield return new WaitForSeconds(hurtStunTime);

        knockbackX = 0f;
        StopMoving();

        attackTimer = Mathf.Max(attackTimer, 0.3f);   // small breathing room for the player
        shootTimer = Mathf.Max(shootTimer, 0.5f);

        state = State.Free;
        actionRoutine = null;
    }

    private void ApplyKnockback()
    {
        knockbackX = Mathf.MoveTowards(knockbackX, 0f, knockbackDecay * Time.deltaTime);
        SetVelocityX(knockbackX);
    }

    public void OnDeath()
    {
        if (state == State.Dead)
            return;

        CancelAction();

        state = State.Dead;

        StopMoving();

        if (deathAnimation != null)
            animator.Play(deathAnimation.name, 0, 0f);

        foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }
    }

    private void CancelAction()
    {
        if (actionRoutine != null)
        {
            StopCoroutine(actionRoutine);
            actionRoutine = null;
        }

        if (attackHitbox != null)
            attackHitbox.StopAttack();
    }

    // =========================================================
    // PATROL
    // =========================================================

    private void HandlePatrol()
    {
        if (leftPoint == null || rightPoint == null)
        {
            StopMoving();
            PlayAnimation(idleAnimation);
            return;
        }

        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.deltaTime;
            StopMoving();
            PlayAnimation(idleAnimation);
            return;
        }

        float x = transform.position.x;

        // Works even if the enemy ends up outside the patrol points after a chase
        if (movingRight && x >= rightPoint.position.x)
        {
            movingRight = false;
            patrolWaitTimer = patrolWaitTime;
            StopMoving();
            return;
        }

        if (!movingRight && x <= leftPoint.position.x)
        {
            movingRight = true;
            patrolWaitTimer = patrolWaitTime;
            StopMoving();
            return;
        }

        float dir = movingRight ? 1f : -1f;

        Move(dir, patrolSpeed);
        Face(dir);
        PlayAnimation(walkAnimation);
    }

    // =========================================================
    // MOVEMENT HELPERS
    // =========================================================

    private void Move(float direction, float speed)
    {
        SetVelocityX(direction * speed);
    }

    private void SetVelocityX(float vx)
    {
        if (UsesPhysics)
            rb.velocity = new Vector2(vx, rb.velocity.y);
        else
            transform.position += Vector3.right * vx * Time.deltaTime;
    }

    private void StopMoving()
    {
        if (UsesPhysics)
            rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    // =========================================================
    // FACING
    // =========================================================

    private void Face(float direction)
    {
        if (Mathf.Abs(direction) < 0.05f)
            return;

        float artSign = spriteFacesRight ? 1f : -1f;
        float dirSign = direction > 0f ? 1f : -1f;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * dirSign * artSign;
        transform.localScale = s;
    }

    // World-space direction the enemy is looking: +1 right, -1 left
    private float FacingDirection()
    {
        float artSign = spriteFacesRight ? 1f : -1f;
        return Mathf.Sign(transform.localScale.x) * artSign;
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    private void PlayAnimation(AnimationClip clip)
    {
        if (clip == null || animator == null)
            return;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        if (info.IsName(clip.name))
            return;

        animator.Play(clip.name, 0, 0f);
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        if (detectionCollider != null)
        {
            Bounds b = detectionCollider.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }
        else
        {
            Gizmos.DrawWireCube(
                transform.position,
                new Vector3(detectionRange * 2f, verticalTolerance * 2f, 0f)
            );
        }

        // Melee range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            transform.position + Vector3.left * attackDistance,
            transform.position + Vector3.right * attackDistance
        );

        // Shoot range (cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            transform.position + Vector3.left * shootRange + Vector3.up * 0.2f,
            transform.position + Vector3.right * shootRange + Vector3.up * 0.2f
        );

        // Min shoot distance (magenta)
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(
            transform.position + Vector3.left * minShootDistance + Vector3.up * 0.4f,
            transform.position + Vector3.right * minShootDistance + Vector3.up * 0.4f
        );
    }
}
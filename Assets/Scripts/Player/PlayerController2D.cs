using System.Collections;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 30f;
    public float deceleration = 40f;
    // Read by FireCooldownUI
    public float FireCooldownRemaining => fireTimer;
    public float FireCooldownTotal => fireCooldown;
    public bool IsFiring => fireRoutine != null;

    [Header("Jump")]
    public float jumpForce = 10f;
    public float gravity = 25f;
    public float maxFallSpeed = 20f;

    [Header("Double Jump")]
    [Tooltip("How many extra jumps are allowed in the air (1 = double jump, 2 = triple jump).")]
    public int extraJumps = 1;
    [Tooltip("Force of the air jump. Set to 0 to use the normal Jump Force.")]
    public float doubleJumpForce = 0f;
    [Tooltip("Optional. If empty, the normal jump animation is replayed.")]
    public AnimationClip doubleJumpAnimation;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Tooltip("Layer for high platforms (e.g. SkyGround). The player can stand and jump here, and the camera stays zoomed out.")]
    public LayerMask skyGroundLayer;

    [Header("Animations")]
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip jumpAnimation;
    public AnimationClip hurtAnimation;   // optional

    [Header("Melee Animations (one is picked at random)")]
    public AnimationClip attack1Animation;
    public AnimationClip attack2Animation;

    [Header("Melee Settings")]
    public PlayerAttackHitbox attackHitbox;
    public float attackCooldown = 0.1f;
    public KeyCode attackKey = KeyCode.J;

    [Header("Gun")]
    public AnimationClip fireAnimation;
    public GameObject bulletPrefab;
    [Tooltip("Child object at the gun muzzle. If empty, the bullet spawns at the player position.")]
    public Transform firePoint;
    public KeyCode fireKey = KeyCode.K;
    public float fireCooldown = 0.5f;

    [Tooltip("When (0-1 of the fire animation) the bullet leaves the gun.")]
    [Range(0f, 1f)] public float fireSpawnNormalized = 0.4f;

    [Tooltip("Untick if your player artwork faces LEFT by default.")]
    public bool spriteFacesRight = true;

    [Header("Hurt")]
    public float hurtStunTime = 0.3f;
    public float knockbackSpeed = 6f;

    [Header("Mobile Input")]
    public bool leftPressed;
    public bool rightPressed;

    private Rigidbody2D rb;
    private Animator animator;

    private float horizontalInput;

    private bool isGrounded;
    private bool isOnSkyGround;

    // Read by CameraFollow2D
    public bool IsGrounded => isGrounded;
    public bool IsOnSkyGround => isOnSkyGround;
    private bool isAttacking;   // true during melee AND gun animations
    private bool isDead;

    private float attackTimer;
    private float fireTimer;
    private float hurtTimer;

    private int extraJumpsRemaining;
    private bool usedAirJump;

    private Coroutine fireRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // We use custom gravity
        rb.gravityScale = 0f;

        // Prevent player from rotating
        rb.freezeRotation = true;

        // Smooth visuals / less camera jitter
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        extraJumpsRemaining = extraJumps;
    }

    private void Update()
    {
        DetectGround();

        if (isDead)
            return;

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (fireTimer > 0f)
            fireTimer -= Time.deltaTime;

        if (hurtTimer > 0f)
            hurtTimer -= Time.deltaTime;

        GetInput();
        HandleJumpInput();
        HandleAttackInput();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            SlowDownHorizontally(deceleration);
            HandleGravity();
            return;
        }

        HandleMovement();
        HandleGravity();
    }

    // =========================================================
    // INPUT
    // =========================================================

    private void GetInput()
    {
        float keyboardInput = Input.GetAxisRaw("Horizontal");

        if (leftPressed && !rightPressed)
            horizontalInput = -1f;
        else if (rightPressed && !leftPressed)
            horizontalInput = 1f;
        else
            horizontalInput = keyboardInput;
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void HandleMovement()
    {
        // Being hit: keep the knockback, just fade it out
        if (hurtTimer > 0f)
        {
            SlowDownHorizontally(deceleration * 0.5f);
            return;
        }

        // Can't move while attacking / firing
        if (isAttacking)
        {
            SlowDownHorizontally(deceleration);
            return;
        }

        float targetSpeed = horizontalInput * moveSpeed;

        float rate = Mathf.Abs(horizontalInput) > 0.01f
            ? acceleration
            : deceleration;

        float newX = Mathf.MoveTowards(
            rb.velocity.x,
            targetSpeed,
            rate * Time.fixedDeltaTime
        );

        rb.velocity = new Vector2(newX, rb.velocity.y);

        FlipPlayer();
    }

    private void SlowDownHorizontally(float rate)
    {
        float x = Mathf.MoveTowards(
            rb.velocity.x,
            0f,
            rate * Time.fixedDeltaTime
        );

        rb.velocity = new Vector2(x, rb.velocity.y);
    }

    private void FlipPlayer()
    {
        float artSign = spriteFacesRight ? 1f : -1f;

        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x) * artSign,
                transform.localScale.y,
                transform.localScale.z
            );
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x) * artSign,
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    // Direction the player is looking in world space: +1 right, -1 left
    private float FacingDirection()
    {
        float artSign = spriteFacesRight ? 1f : -1f;
        return Mathf.Sign(transform.localScale.x) * artSign;
    }

    // =========================================================
    // GRAVITY
    // =========================================================

    private void HandleGravity()
    {
        if (!isGrounded)
        {
            float newY = rb.velocity.y - gravity * Time.fixedDeltaTime;
            newY = Mathf.Max(newY, -maxFallSpeed);

            rb.velocity = new Vector2(rb.velocity.x, newY);
        }
        else if (rb.velocity.y < 0f)
        {
            // Keep player slightly grounded
            rb.velocity = new Vector2(rb.velocity.x, -1f);
        }
    }

    // =========================================================
    // GROUND DETECTION
    // =========================================================

    private void DetectGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            isOnSkyGround = false;
            return;
        }

        // Both normal ground and sky ground count as "ground" (so jumping works on both)
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer | skyGroundLayer
        );

        // Standing specifically on the sky ground layer
        isOnSkyGround = isGrounded && Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            skyGroundLayer
        );

        // Refill air jumps when standing on the ground.
        // The velocity check stops the refill on the frames right after
        // take-off, when the ground check may still overlap the ground.
        if (isGrounded && rb.velocity.y <= 0.1f)
        {
            extraJumpsRemaining = extraJumps;
            usedAirJump = false;
        }
    }

    // =========================================================
    // JUMP
    // =========================================================

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Jump();
    }

    public void Jump()
    {
        if (isDead || isAttacking || hurtTimer > 0f)
            return;

        // Normal jump from the ground
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            return;
        }

        // Extra jump in the air
        if (extraJumpsRemaining > 0)
        {
            extraJumpsRemaining--;
            usedAirJump = true;

            float force = doubleJumpForce > 0f ? doubleJumpForce : jumpForce;
            rb.velocity = new Vector2(rb.velocity.x, force);

            // Restart the animation so the second jump is visible
            AnimationClip clip = doubleJumpAnimation != null ? doubleJumpAnimation : jumpAnimation;
            if (clip != null)
                animator.Play(clip.name, 0, 0f);
        }
    }

    // =========================================================
    // ATTACK INPUT
    // =========================================================

    private void HandleAttackInput()
    {
        if (Input.GetKeyDown(attackKey))
            Attack();

        if (Input.GetKeyDown(fireKey))
            Fire();
    }

    // =========================================================
    // MELEE
    // =========================================================

    public void Attack()
    {
        if (isDead || isAttacking || hurtTimer > 0f)
            return;

        // Don't attack while airborne
        if (!isGrounded)
            return;

        if (attackTimer > 0f)
            return;

        // Randomly choose one of the two melee attacks (skips unassigned clips)
        AnimationClip[] clips = { attack1Animation, attack2Animation };

        int start = Random.Range(0, clips.Length);

        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip c = clips[(start + i) % clips.Length];

            if (c != null)
            {
                PlayAttack(c);
                return;
            }
        }

        Debug.LogWarning("Melee attack animation is not assigned!");
    }

    private void PlayAttack(AnimationClip attackClip)
    {
        isAttacking = true;

        // Stop horizontal movement
        rb.velocity = new Vector2(0f, rb.velocity.y);

        animator.Play(attackClip.name, 0, 0f);

        attackTimer = attackCooldown;

        if (attackHitbox != null)
            attackHitbox.StartAttackHitbox();

        CancelInvoke(nameof(FinishAttack));
        Invoke(nameof(FinishAttack), attackClip.length);
    }

    private void FinishAttack()
    {
        isAttacking = false;

        if (attackHitbox != null)
            attackHitbox.StopAttackHitbox();

        if (!isDead)
            UpdateAnimations();
    }

    // =========================================================
    // GUN
    // =========================================================

    public void Fire()
    {
        if (isDead || isAttacking || hurtTimer > 0f)
            return;

        if (!isGrounded)
            return;

        if (fireTimer > 0f)
            return;

        if (fireAnimation == null || bulletPrefab == null)
        {
            Debug.LogWarning("Fire animation or bullet prefab is not assigned!");
            return;
        }

        fireRoutine = StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        isAttacking = true;

        rb.velocity = new Vector2(0f, rb.velocity.y);

        animator.Play(fireAnimation.name, 0, 0f);

        float length = fireAnimation.length;
        float spawnTime = length * fireSpawnNormalized;

        yield return new WaitForSeconds(spawnTime);

        SpawnBullet();

        yield return new WaitForSeconds(Mathf.Max(0f, length - spawnTime));

        fireTimer = fireCooldown;
        isAttacking = false;
        fireRoutine = null;

        if (!isDead)
            UpdateAnimations();
    }

    private void SpawnBullet()
    {
        Vector3 pos = firePoint != null ? firePoint.position : transform.position;

        GameObject go = Instantiate(bulletPrefab, pos, Quaternion.identity);

        Bullet2D bullet = go.GetComponent<Bullet2D>();

        if (bullet != null)
            bullet.Init(FacingDirection(), true);
    }

    private void CancelFire()
    {
        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;
        }
    }

    // =========================================================
    // HURT / DEATH (called by PlayerHealth)
    // =========================================================

    public void OnHurt(Vector2 sourcePosition)
    {
        if (isDead)
            return;

        // Getting hit interrupts your melee attack or gun shot
        CancelInvoke(nameof(FinishAttack));
        CancelFire();
        isAttacking = false;

        if (attackHitbox != null)
            attackHitbox.StopAttackHitbox();

        hurtTimer = hurtStunTime;

        float dir;

        if (Mathf.Approximately(sourcePosition.x, transform.position.x))
            dir = -FacingDirection();
        else
            dir = transform.position.x >= sourcePosition.x ? 1f : -1f;

        rb.velocity = new Vector2(dir * knockbackSpeed, rb.velocity.y);

        if (hurtAnimation != null)
            animator.Play(hurtAnimation.name, 0, 0f);
    }

    public void OnDeath()
    {
        isDead = true;
        isAttacking = false;
        horizontalInput = 0f;

        CancelInvoke(nameof(FinishAttack));
        CancelFire();

        if (attackHitbox != null)
            attackHitbox.StopAttackHitbox();
    }

    public void Revive()
    {
        isDead = false;
        isAttacking = false;
        hurtTimer = 0f;
        fireTimer = 0f;
        extraJumpsRemaining = extraJumps;
        usedAirJump = false;
    }

    // =========================================================
    // ANIMATIONS
    // =========================================================

    private void UpdateAnimations()
    {
        // Never override attack / fire / hurt animation
        if (isAttacking)
            return;

        if (hurtTimer > 0f && hurtAnimation != null)
            return;

        if (!isGrounded)
        {
            AnimationClip airClip = (usedAirJump && doubleJumpAnimation != null)
                ? doubleJumpAnimation
                : jumpAnimation;

            PlayAnimation(airClip);
            return;
        }

        if (Mathf.Abs(horizontalInput) > 0.01f)
            PlayAnimation(walkAnimation);
        else
            PlayAnimation(idleAnimation);
    }

    private void PlayAnimation(AnimationClip clip)
    {
        if (clip == null)
            return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(clip.name))
            return;

        animator.Play(clip.name, 0, 0f);
    }

    // =========================================================
    // MOBILE BUTTONS
    // =========================================================

    public void LeftDown()   { leftPressed = true; }
    public void LeftUp()     { leftPressed = false; }
    public void RightDown()  { rightPressed = true; }
    public void RightUp()    { rightPressed = false; }
    public void JumpButton() { Jump(); }
    public void AttackButton() { Attack(); }
    public void FireButton() { Fire(); }

    // =========================================================
    // DEBUG GROUND CHECK
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
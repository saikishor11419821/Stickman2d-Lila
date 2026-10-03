using UnityEngine;

public class Bullet2D : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 10;
    public float lifetime = 3f;

    [Tooltip("Ground/wall layers that stop the bullet.")]
    public LayerMask obstacleLayers;

    private float direction = 1f;
    private bool firedByPlayer;
    private bool initialized;

    // Called by whoever spawns the bullet
    public void Init(float dir, bool fromPlayer)
    {
        direction = dir >= 0f ? 1f : -1f;
        firedByPlayer = fromPlayer;
        initialized = true;

        // Flip the sprite so it faces its travel direction
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * direction;
        transform.localScale = s;

        Destroy(gameObject, lifetime);
    }

    private void Awake()
    {
        // Trigger events need a Rigidbody2D on one side; a kinematic one keeps it self-contained
        Rigidbody2D body = GetComponent<Rigidbody2D>();

        if (body == null)
            body = gameObject.AddComponent<Rigidbody2D>();

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
            col.isTrigger = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        transform.position += Vector3.right * direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized)
            return;

        if (firedByPlayer)
        {
            EnemyHealth eh = other.GetComponentInParent<EnemyHealth>();

            if (eh != null)
            {
                // >>> Adjust this call to match your EnemyHealth's method signature <<<
                eh.TakeDamage(damage, transform.position);
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();

            if (ph != null)
            {
                // >>> Adjust this call to match your PlayerHealth's method signature <<<
                ph.TakeDamage(damage, transform.position);
                Destroy(gameObject);
                return;
            }
        }

        // Hit a wall / ground
        if (((1 << other.gameObject.layer) & obstacleLayers.value) != 0)
            Destroy(gameObject);
    }
}
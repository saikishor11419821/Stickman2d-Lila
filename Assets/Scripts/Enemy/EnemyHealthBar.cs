using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;

    [Tooltip("Optional. A slider BEHIND the health slider that trails down to show recent damage.")]
    public Slider damageSlider;

    public CanvasGroup canvasGroup;

    [Header("Popup")]
    [Tooltip("How long the bar stays visible after the last hit.")]
    public float visibleTime = 2f;
    public float fadeSpeed = 4f;
    public bool alwaysVisible = false;

    [Header("Damage Trail")]
    public float trailDelay = 0.4f;
    public float trailSpeed = 1.5f;   // slider units (0-1) per second

    private float targetValue = 1f;
    private float visibleTimer;
    private float trailTimer;
    private bool hidden;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }

        if (damageSlider != null)
        {
            damageSlider.minValue = 0f;
            damageSlider.maxValue = 1f;
            damageSlider.value = 1f;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = alwaysVisible ? 1f : 0f;
    }

    // Called by EnemyHealth every time damage is taken
    public void SetHealth(int current, int max)
    {
        targetValue = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

        if (healthSlider != null)
            healthSlider.value = targetValue;

        trailTimer = trailDelay;
        visibleTimer = visibleTime;
        hidden = false;
    }

    public void Hide()
    {
        hidden = true;
        visibleTimer = 0f;
    }

    private void Update()
    {
        // Damage trail: waits a moment, then slides down to the real value
        if (damageSlider != null)
        {
            if (trailTimer > 0f)
                trailTimer -= Time.deltaTime;
            else
                damageSlider.value = Mathf.MoveTowards(damageSlider.value, targetValue, trailSpeed * Time.deltaTime);
        }

        // Popup fade
        if (canvasGroup != null)
        {
            if (visibleTimer > 0f)
                visibleTimer -= Time.deltaTime;

            bool show = !hidden && (alwaysVisible || visibleTimer > 0f);
            float targetAlpha = show ? 1f : 0f;

            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }
    }

    // Enemy flips localScale.x to face the player - cancel that so the bar never mirrors
    private void LateUpdate()
    {
        if (transform.parent == null)
            return;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * Mathf.Sign(transform.parent.lossyScale.x);
        transform.localScale = s;
    }
}
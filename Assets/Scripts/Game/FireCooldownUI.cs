using UnityEngine;
using TMPro;

public class FireCooldownUI : MonoBehaviour
{
    [Header("References")]
    public PlayerController2D player;
    public TMP_Text cooldownText;

    [Header("Settings")]
    [Tooltip("Text format. 0.0 shows one decimal, 0 shows whole seconds.")]
    public string format = "0.0";
    public string suffix = "s";

    private void Awake()
    {
        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (player == null || cooldownText == null)
            return;

        float remaining = player.FireCooldownRemaining;

        // While the fire animation is playing, show the full cooldown value.
        // After it ends, count down to 0.
        bool show = player.IsFiring || remaining > 0f;

        if (cooldownText.gameObject.activeSelf != show)
            cooldownText.gameObject.SetActive(show);

        if (!show)
            return;

        float value = player.IsFiring ? player.FireCooldownTotal : remaining;
        cooldownText.text = value.ToString(format) + suffix;
    }
}
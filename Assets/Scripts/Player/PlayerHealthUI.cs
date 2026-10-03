using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Slider healthSlider;

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        UpdateHealthBar();
    }

    private void Update()
    {
        if (playerHealth == null || healthSlider == null)
            return;

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthSlider.value = playerHealth.GetHealthPercentage();
    }
}
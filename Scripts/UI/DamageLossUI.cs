using JacobHomanics.HealthSystem;
using JacobHomanics.HealthSystem.UI;
using JacobHomanics.TrickedOutUI;
using UnityEngine;

public class DamageLossUI : MonoBehaviour
{
    public AnimatedImageFill animatedImageFill;
    public Health health;

    [Header("Damage Animation Settings")]
    [Tooltip("Duration for damage animation when health decreases")]
    public float damageDuration = 1f;
    [Tooltip("Delay before damage animation starts when health decreases")]
    public float damageDelay = 0.5f;

    private float previousHealth;
    private float maxHealthReached;

    void Start()
    {
        if (health == null)
            health = GetComponent<Health>();

        if (health != null)
        {
            previousHealth = health.Current;
            maxHealthReached = health.Current;
        }
    }

    void Update()
    {
        if (health == null || animatedImageFill == null)
            return;

        float currentHealth = health.Current;

        // Update max health reached
        if (currentHealth > maxHealthReached)
        {
            maxHealthReached = currentHealth;
        }

        // Check if health changed
        if (currentHealth != previousHealth)
        {
            if (currentHealth > previousHealth)
            {
                // Health went up - only set duration and delay to 0 if we've reached or exceeded max health
                if (currentHealth >= maxHealthReached)
                {
                    animatedImageFill.properties.animationDuration = 0f;
                    animatedImageFill.properties.delay = 0f;
                }
                // If health went up but is still below maxHealthReached, keep damage animation settings
            }
            else if (currentHealth < previousHealth)
            {
                // Health went down - set duration and delay to configured values
                animatedImageFill.properties.animationDuration = damageDuration;
                animatedImageFill.properties.delay = damageDelay;
            }

            previousHealth = currentHealth;
        }
    }
}

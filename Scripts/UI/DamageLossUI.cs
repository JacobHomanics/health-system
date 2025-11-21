using JacobHomanics.TrickedOutUI;
using UnityEngine;

public class DamageLossUI : MonoBehaviour
{
    public AnimatedImageFill animatedImageFill;
    public BaseVector2Adapter adapter;

    [Header("Damage Animation Settings")]
    [Tooltip("Duration for damage animation when health decreases")]
    public float damageDuration = 1f;
    [Tooltip("Delay before damage animation starts when health decreases")]
    public float damageDelay = 0.5f;

    private float previous;
    private float maxReached;

    void Start()
    {
        previous = adapter.X;
        maxReached = adapter.X;
    }

    void Update()
    {
        float current = adapter.X;

        // Update max health reached
        if (current > maxReached)
        {
            maxReached = current;
        }

        // Check if health changed
        if (current != previous)
        {
            if (current > previous)
            {
                // Health went up - only set duration and delay to 0 if we've reached or exceeded max health
                if (current >= maxReached)
                {
                    animatedImageFill.properties.animationDuration = 0f;
                    animatedImageFill.properties.delay = 0f;
                }
                // If health went up but is still below maxHealthReached, keep damage animation settings
            }
            else if (current < previous)
            {
                // Health went down - set duration and delay to configured values
                animatedImageFill.properties.animationDuration = damageDuration;
                animatedImageFill.properties.delay = damageDelay;
            }

            previous = current;
        }
    }
}

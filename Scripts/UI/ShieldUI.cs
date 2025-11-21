using JacobHomanics.HealthSystem.UI;
using UnityEngine;
using UnityEngine.UI;

public class ShieldUI : MonoBehaviour
{
    public HealthAdapter healthAdapter;
    public Image image;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //   float totalValue = health.Max + health.ShieldCurrent;
        //         float shieldPercent = totalValue > 0 ? health.ShieldCurrent / totalValue : 0;
        //         shieldPercent = Mathf.Clamp01(shieldPercent);

        float totalMax = healthAdapter.Y + healthAdapter.health.Shield;
        float shieldPercent = totalMax > 0 ? healthAdapter.health.Shield / totalMax : 0;
        shieldPercent = Mathf.Clamp01(shieldPercent);
        image.fillAmount = shieldPercent;
    }
}

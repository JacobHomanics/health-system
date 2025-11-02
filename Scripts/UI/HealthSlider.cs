using JacobHomanics.HealthSystem;
using UnityEngine;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour
{
    public Health health;
    public Slider slider;

    void Update()
    {
        slider.value = health.Current;
        slider.maxValue = health.Max;
    }
}

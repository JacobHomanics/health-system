using System.Collections.Generic;
using JacobHomanics.HealthSystem;
using UnityEngine;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour
{
    [SerializeField] private Health health;

    public Health Health
    {
        get => health;
    }

    [SerializeField] private Slider slider;

    public Slider Slider
    {
        get => slider;
    }

    [SerializeReference]
    private List<FeatureToggle> featureToggles = new List<FeatureToggle>
    {
        new TextDisplayFeature(),
        new ColorGradientFeature(),
        new BackgroundFillFeature(),
        new FlashingFeature()
    };

    public float CurrentNum => Health.Current;
    public float MaxNum => Health.Max;

    private float previousValue;

    private void Awake()
    {
        previousValue = CurrentNum;
    }

    void Update()
    {
        Slider.value = CurrentNum;
        Slider.maxValue = MaxNum;

        // Check if value has changed and handle background fill animation
        UIToolkit.HandleValueChange(CurrentNum, UIToolkit.GetFeature<BackgroundFillFeature>(featureToggles), ref previousValue, MaxNum);

        TextFeatureCommand();

        UIToolkit.ColorGradientFeatureCommand(UIToolkit.GetFeature<ColorGradientFeature>(featureToggles), Slider.fillRect.GetComponent<Image>(), CurrentNum, MaxNum);

        UIToolkit.FlashingFeatureCommand(UIToolkit.GetFeature<FlashingFeature>(featureToggles), CurrentNum, MaxNum);

        UIToolkit.UpdateBackgroundFillAnimation(UIToolkit.GetFeature<BackgroundFillFeature>(featureToggles), MaxNum);
    }

    private void TextFeatureCommand()
    {
        var textFeature = UIToolkit.GetFeature<TextDisplayFeature>(featureToggles);
        UIToolkit.Display(textFeature.textCurrent, CurrentNum, MaxNum);
        UIToolkit.Display(textFeature.textMax, CurrentNum, MaxNum);
    }
}

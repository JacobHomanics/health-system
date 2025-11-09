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
        new BackgroundFillFeature()
    };

    public float CurrentNum => Health.Current;
    public float MaxNum => Health.Max;

    private float previousValue;

    private void Awake()
    {
        previousValue = slider.value;
        slider.onValueChanged.AddListener(OnValueChangedInternal);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnValueChangedInternal);
    }

    void Update()
    {
        Slider.value = CurrentNum;
        Slider.maxValue = MaxNum;

        TextFeatureCommand();

        UIToolkit.ColorGradientFeatureCommand(UIToolkit.GetFeature<ColorGradientFeature>(featureToggles), Slider.fillRect.GetComponent<Image>(), CurrentNum, MaxNum);

        UIToolkit.FlashingFeatureCommand(UIToolkit.GetFeature<FlashingFeature>(featureToggles), CurrentNum, MaxNum);

        UIToolkit.UpdateBackgroundFillAnimation(UIToolkit.GetFeature<BackgroundFillFeature>(featureToggles), MaxNum);
    }

    private void OnValueChangedInternal(float newValue)
    {
        var bgFeature = UIToolkit.GetFeature<BackgroundFillFeature>(featureToggles);
        if (bgFeature == null || bgFeature.backgroundFill == null)
        {
            previousValue = newValue;
            return;
        }

        // Get the starting value based on whether we want to keep size consistent
        float startValue;
        if (bgFeature.keepSizeConsistent)
        {
            // Use current background fill position (continues from where it is)
            startValue = UIToolkit.GetBackgroundFillValue(bgFeature, MaxNum);
        }
        else
        {
            // Reset to previous value (starts from previous slider value)
            startValue = previousValue;
            UIToolkit.SetBackgroundFillAmount(bgFeature, previousValue, MaxNum);
        }

        // If new value is greater than start position, immediately snap to it
        if (newValue > startValue)
        {
            // Stop any ongoing animation
            bgFeature.isAnimating = false;
            // Immediately set to new value
            UIToolkit.SetBackgroundFillAmount(bgFeature, newValue, MaxNum);
        }
        else
        {
            // HP goes down or stays same - animate from start position
            // Set up animation state
            UIToolkit.StartBackgroundFillAnimation(startValue, newValue, bgFeature, MaxNum);
        }

        previousValue = newValue;
    }

    // Main Health Slider 


    // Min/Max Health Feature
    private void TextFeatureCommand()
    {
        var textFeature = UIToolkit.GetFeature<TextDisplayFeature>(featureToggles);
        UIToolkit.Display(textFeature.textCurrent, CurrentNum, MaxNum);
        UIToolkit.Display(textFeature.textMax, CurrentNum, MaxNum);
    }
}

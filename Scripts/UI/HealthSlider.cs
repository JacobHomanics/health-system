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
        previousValue = CurrentNum;
    }

    void Update()
    {
        Slider.value = CurrentNum;
        Slider.maxValue = MaxNum;

        // Check if value has changed and handle background fill animation
        HandleValueChange(CurrentNum, UIToolkit.GetFeature<BackgroundFillFeature>(featureToggles), ref previousValue, MaxNum);

        TextFeatureCommand();

        UIToolkit.ColorGradientFeatureCommand(UIToolkit.GetFeature<ColorGradientFeature>(featureToggles), Slider.fillRect.GetComponent<Image>(), CurrentNum, MaxNum);

        UIToolkit.FlashingFeatureCommand(UIToolkit.GetFeature<FlashingFeature>(featureToggles), CurrentNum, MaxNum);

        UIToolkit.UpdateBackgroundFillAnimation(UIToolkit.GetFeature<BackgroundFillFeature>(featureToggles), MaxNum);
    }

    public static void HandleValueChange(float newValue, BackgroundFillFeature bgFeature, ref float previousValue, float max)
    {
        if (Mathf.Abs(newValue - previousValue) < 0.001f)
            return;

        if (bgFeature == null || bgFeature.backgroundFill == null)
        {
            previousValue = newValue;
            return;
        }

        // Get the current background fill value
        float currentFillValue = UIToolkit.GetBackgroundFillValue(bgFeature, max);

        // Check if background fill needs initialization (is at or near 0, indicating uninitialized)
        // Only initialize if it's truly uninitialized, not just different
        if (currentFillValue < 0.01f * max)
        {
            // Background fill appears uninitialized, initialize it to previousValue
            UIToolkit.SetBackgroundFillAmount(bgFeature, previousValue, max);
            currentFillValue = previousValue;
        }

        // Get the starting value based on whether we want to keep size consistent
        float startValue;
        if (bgFeature.keepSizeConsistent)
        {
            // Use current background fill position (continues from where it is)
            startValue = currentFillValue;
        }
        else
        {
            // Reset to previous value (starts from previous slider value)
            startValue = previousValue;
            UIToolkit.SetBackgroundFillAmount(bgFeature, previousValue, max);
        }

        // If new value is greater than start position, immediately snap to it
        if (newValue > startValue)
        {
            // Stop any ongoing animation
            bgFeature.isAnimating = false;
            // Immediately set to new value
            UIToolkit.SetBackgroundFillAmount(bgFeature, newValue, max);
        }
        else
        {
            // HP goes down or stays same - animate from start position
            // Set up animation state
            UIToolkit.StartBackgroundFillAnimation(startValue, newValue, bgFeature, max);
        }

        previousValue = newValue;
    }

    private void TextFeatureCommand()
    {
        var textFeature = UIToolkit.GetFeature<TextDisplayFeature>(featureToggles);
        UIToolkit.Display(textFeature.textCurrent, CurrentNum, MaxNum);
        UIToolkit.Display(textFeature.textMax, CurrentNum, MaxNum);
    }
}

using System.Collections.Generic;
using JacobHomanics.HealthSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthImage : MonoBehaviour
{
    [SerializeField] private Health health;

    public Health Health
    {
        get => health;
    }

    [SerializeField] private Image image;

    public Image Image
    {
        get => image;
    }

    [SerializeReference]
    private List<FeatureToggle> featureToggles = new List<FeatureToggle>
    {
        new TextDisplayFeature2(),
        new ColorGradientFeature(),
        new BackgroundFillFeature(),
        new FlashingFeature()
    };

    public float CurrentNum => Health.Current;
    public float MaxNum => Health.Max;

    // Main Health Slider 
    void Update()
    {
        image.fillAmount = CurrentNum / MaxNum;

        UIToolkit.Display(UIToolkit.GetFeature<TextDisplayFeature2>(featureToggles), CurrentNum, MaxNum);

        UIToolkit.ColorGradientFeatureCommand(UIToolkit.GetFeature<ColorGradientFeature>(featureToggles), image, CurrentNum, MaxNum);

        UIToolkit.FlashingFeatureCommand(UIToolkit.GetFeature<FlashingFeature>(featureToggles), CurrentNum, MaxNum);

        UIToolkit.UpdateBackgroundFillAnimation(UIToolkit.GetFeature<BackgroundFillFeature>(featureToggles), MaxNum);
    }

    private float previousValue;

    private void Awake()
    {
        previousValue = image.fillAmount;
        health.onCurrentChange.AddListener(OnValueChangedInternal);
    }

    private void OnDestroy()
    {
        health.onCurrentChange.RemoveListener(OnValueChangedInternal);
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
}

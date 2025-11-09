using System.Collections.Generic;
using JacobHomanics.HealthSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthImage : MonoBehaviour
{
    [System.Serializable]
    public class TextDisplayFeature2 : FeatureToggle
    {
        public TMP_Text text;

        public enum DisplayType
        {
            Current, Max
        }

        public DisplayType displayType;
        public string format = "#,##0";

        public TextDisplayFeature2()
        {
        }

        public TextDisplayFeature2(DisplayType displayType)
        {
            this.displayType = displayType;
        }
    }

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

    private T GetFeature<T>() where T : FeatureToggle
    {
        return featureToggles.Find(f => f is T) as T;
    }

    public float CurrentNum => Health.Current;
    public float MaxNum => Health.Max;

    // Main Health Slider 
    void Update()
    {
        image.fillAmount = CurrentNum / MaxNum;

        HealthSlider.Display(GetFeature<TextDisplayFeature2>(), CurrentNum, MaxNum);

        HealthSlider.ColorGradientFeatureCommand(GetFeature<ColorGradientFeature>(), image, CurrentNum, MaxNum);

        HealthSlider.FlashingFeatureCommand(GetFeature<FlashingFeature>(), CurrentNum, MaxNum);

        HealthSlider.UpdateBackgroundFillAnimation(GetFeature<BackgroundFillFeature>(), MaxNum);
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
        var bgFeature = GetFeature<BackgroundFillFeature>();
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
            startValue = HealthSlider.GetBackgroundFillValue(bgFeature, MaxNum);
        }
        else
        {
            // Reset to previous value (starts from previous slider value)
            startValue = previousValue;
            HealthSlider.SetBackgroundFillAmount(bgFeature, previousValue, MaxNum);
        }

        // If new value is greater than start position, immediately snap to it
        if (newValue > startValue)
        {
            // Stop any ongoing animation
            bgFeature.isAnimating = false;
            // Immediately set to new value
            HealthSlider.SetBackgroundFillAmount(bgFeature, newValue, MaxNum);
        }
        else
        {
            // HP goes down or stays same - animate from start position
            // Set up animation state
            HealthSlider.StartBackgroundFillAnimation(startValue, newValue, bgFeature, MaxNum);
        }

        previousValue = newValue;
    }
}

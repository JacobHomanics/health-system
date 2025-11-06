using System.Collections.Generic;
using JacobHomanics.HealthSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public abstract class FeatureToggle
{
}

[System.Serializable]
public class TextDisplayFeature : FeatureToggle
{
    public TMP_Text textCurrent;
    public TMP_Text textMax;

}

[System.Serializable]
public class ColorGradientFeature : FeatureToggle
{
    public Color colorAtMin = Color.red;
    public Color colorAtHalfway = Color.yellow;
    public Color colorAtMax = Color.green;

}

[System.Serializable]
public class BackgroundFillFeature : FeatureToggle
{
    public Image backgroundFill;
    public bool keepSizeConsistent = true;
    public float animationSpeed = 10;
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 16f);
    public float delay = 1f;

    public bool isAnimating = false;
    public float animationFromValue;
    public float animationToValue;
    public float animationElapsed;
    public float animationDuration;
    public float animationDelayRemaining;

}

[System.Serializable]
public class FlashingFeature : FeatureToggle
{
    public float thresholdPercent = 0.2f;
    public Color flashColor1 = Color.red;
    public Color flashColor2 = Color.white;
    public float flashSpeed = 15f;
}

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

    private T GetFeature<T>() where T : FeatureToggle
    {
        return featureToggles.Find(f => f is T) as T;
    }

    private float previousValue;

    // Animation state for background fill


    void Update()
    {
        Slider.value = Health.Current;
        Slider.maxValue = Health.Max;

        TextFeatureCommand();

        ColorGradientFeatureCommand();

        FlashingFeatureCommand();

        UpdateBackgroundFillAnimation();
    }

    private void TextFeatureCommand()
    {
        var textFeature = GetFeature<TextDisplayFeature>();
        if (textFeature != null && textFeature.textCurrent != null && textFeature.textMax != null)
        {
            textFeature.textCurrent.text = Health.Current.ToString();
            textFeature.textMax.text = Health.Max.ToString();
        }
    }

    private void ColorGradientFeatureCommand()
    {
        float healthPercent = Slider.maxValue > 0 ? Slider.value / Slider.maxValue : 0;
        healthPercent = Mathf.Clamp01(healthPercent);

        // Color gradient: green (high) -> yellow (mid) -> red (low)
        var colorFeature = GetFeature<ColorGradientFeature>();
        var flashingFeature = GetFeature<FlashingFeature>();

        // If flashing feature is active and health is below threshold, let flashing handle the color
        if (flashingFeature != null && healthPercent < flashingFeature.thresholdPercent)
        {
            // Flashing feature will handle the color
            return;
        }

        if (colorFeature != null)
        {
            Color healthColor;
            if (healthPercent > 0.5f)
            {
                // Green to yellow
                float t = (healthPercent - 0.5f) * 2f;
                healthColor = Color.Lerp(colorFeature.colorAtHalfway, colorFeature.colorAtMax, t);
            }
            else
            {
                // Yellow to red
                float t = healthPercent * 2f;
                healthColor = Color.Lerp(colorFeature.colorAtMin, colorFeature.colorAtHalfway, t);
            }

            slider.fillRect.GetComponent<Image>().color = healthColor;
        }
    }

    private void FlashingFeatureCommand()
    {
        float healthPercent = Slider.maxValue > 0 ? Slider.value / Slider.maxValue : 0;
        healthPercent = Mathf.Clamp01(healthPercent);

        var flashingFeature = GetFeature<FlashingFeature>();
        if (flashingFeature != null && healthPercent < flashingFeature.thresholdPercent)
        {
            // Calculate flashing based on time
            float flashValue = Mathf.Sin(Time.time * flashingFeature.flashSpeed) * 0.5f + 0.5f;
            Color flashColor = Color.Lerp(flashingFeature.flashColor1, flashingFeature.flashColor2, flashValue);
            slider.fillRect.GetComponent<Image>().color = flashColor;
        }
    }

    private void Awake()
    {
        previousValue = slider.value;
        slider.onValueChanged.AddListener(OnValueChangedInternal);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnValueChangedInternal);
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
            startValue = GetBackgroundFillValue();
        }
        else
        {
            // Reset to previous value (starts from previous slider value)
            startValue = previousValue;
            SetBackgroundFillAmount(previousValue);
        }

        // If new value is greater than start position, immediately snap to it
        if (newValue > startValue)
        {
            // Stop any ongoing animation
            bgFeature.isAnimating = false;
            // Immediately set to new value
            SetBackgroundFillAmount(newValue);
        }
        else
        {
            // HP goes down or stays same - animate from start position
            // Set up animation state
            StartBackgroundFillAnimation(startValue, newValue, bgFeature);
        }

        previousValue = newValue;
    }

    private void StartBackgroundFillAnimation(float fromValue, float toValue, BackgroundFillFeature bgFeature)
    {
        float valueDifference = Mathf.Abs(fromValue - toValue);
        if (valueDifference < 0.001f)
        {
            SetBackgroundFillAmount(toValue);
            bgFeature.isAnimating = false;
            return;
        }

        // Initialize animation state
        bgFeature.isAnimating = true;
        bgFeature.animationFromValue = fromValue;
        bgFeature.animationToValue = toValue;
        bgFeature.animationElapsed = 0f;
        bgFeature.animationDelayRemaining = bgFeature.delay;

        // Calculate dynamic animation speed based on difference
        float normalizedDifference = slider.maxValue > 0 ? valueDifference / slider.maxValue : 0;
        float speedMultiplier = bgFeature.speedCurve.Evaluate(normalizedDifference);
        float dynamicSpeed = bgFeature.animationSpeed * speedMultiplier;
        bgFeature.animationDuration = valueDifference / dynamicSpeed;
    }

    private void UpdateBackgroundFillAnimation()
    {
        var bgFeature = GetFeature<BackgroundFillFeature>();


        if (!bgFeature.isAnimating)
            return;

        // Handle delay before animation starts
        if (bgFeature.animationDelayRemaining > 0f)
        {
            bgFeature.animationDelayRemaining -= Time.deltaTime;
            return;
        }

        // Update animation
        bgFeature.animationElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(bgFeature.animationElapsed / bgFeature.animationDuration);
        float currentValue = Mathf.Lerp(bgFeature.animationFromValue, bgFeature.animationToValue, t);
        SetBackgroundFillAmount(currentValue);

        // Check if animation is complete
        if (bgFeature.animationElapsed >= bgFeature.animationDuration)
        {
            SetBackgroundFillAmount(bgFeature.animationToValue);
            bgFeature.isAnimating = false;
        }
    }

    private void SetBackgroundFillAmount(float amount)
    {
        var bgFeature = GetFeature<BackgroundFillFeature>();
        if (bgFeature != null && bgFeature.backgroundFill != null)
        {
            bgFeature.backgroundFill.fillAmount = amount / slider.maxValue;
        }
    }

    private float GetBackgroundFillValue()
    {
        var bgFeature = GetFeature<BackgroundFillFeature>();
        if (bgFeature != null && bgFeature.backgroundFill != null)
        {
            return bgFeature.backgroundFill.fillAmount * slider.maxValue;
        }
        return 0;
    }


    private void OnEnable()
    {
        SetBackgroundFillAmount(slider.value);
    }
}

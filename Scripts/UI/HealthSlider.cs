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
    public HealthImage.TextDisplayFeature2 textCurrent = new(HealthImage.TextDisplayFeature2.DisplayType.Current);
    public HealthImage.TextDisplayFeature2 textMax = new(HealthImage.TextDisplayFeature2.DisplayType.Max);
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
    public Image flashImage;
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

    public float CurrentNum => Health.Current;
    public float MaxNum => Health.Max;

    // Main Health Slider 
    void Update()
    {
        Slider.value = CurrentNum;
        Slider.maxValue = MaxNum;

        TextFeatureCommand();

        ColorGradientFeatureCommand(GetFeature<ColorGradientFeature>(), Slider.fillRect.GetComponent<Image>(), CurrentNum, MaxNum);

        FlashingFeatureCommand(GetFeature<FlashingFeature>(), CurrentNum, MaxNum);

        UpdateBackgroundFillAnimation(GetFeature<BackgroundFillFeature>(), MaxNum);
    }

    // Min/Max Health Feature
    private void TextFeatureCommand()
    {
        var textFeature = GetFeature<TextDisplayFeature>();
        Display(textFeature.textCurrent, CurrentNum, MaxNum);
        Display(textFeature.textMax, CurrentNum, MaxNum);
    }

    public static void Display(HealthImage.TextDisplayFeature2 textDisplayFeature, float current, float max)
    {
        if (textDisplayFeature.displayType == HealthImage.TextDisplayFeature2.DisplayType.Current)
            Display(textDisplayFeature.text, current, textDisplayFeature.format);
        if (textDisplayFeature.displayType == HealthImage.TextDisplayFeature2.DisplayType.Max)
            Display(textDisplayFeature.text, max, textDisplayFeature.format);

    }

    public static void Display(TMP_Text text, float num, string format)
    {
        text.text = num.ToString(format);
    }

    // Flashing Fill Rect Feature
    public static void FlashingFeatureCommand(FlashingFeature flashingFeature, float current, float max)
    {
        float healthPercent = current / max;
        healthPercent = Mathf.Clamp01(healthPercent);

        flashingFeature.flashImage.gameObject.SetActive(healthPercent < flashingFeature.thresholdPercent);
        if (flashingFeature != null && healthPercent < flashingFeature.thresholdPercent)
        {
            // Calculate flashing based on time
            float flashValue = Mathf.Sin(Time.time * flashingFeature.flashSpeed) * 0.5f + 0.5f;
            Color flashColor = Color.Lerp(flashingFeature.flashColor1, flashingFeature.flashColor2, flashValue);
            flashingFeature.flashImage.color = flashColor;
            flashingFeature.flashImage.fillAmount = healthPercent;
        }
    }


    // Color Gradient Health Feature
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

    public static void ColorGradientFeatureCommand(ColorGradientFeature colorFeature, Image image, float current, float max)
    {
        float healthPercent = current / max;
        healthPercent = Mathf.Clamp01(healthPercent);

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

            image.color = healthColor;
        }
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
            startValue = GetBackgroundFillValue(bgFeature, MaxNum);
        }
        else
        {
            // Reset to previous value (starts from previous slider value)
            startValue = previousValue;
            SetBackgroundFillAmount(bgFeature, previousValue, MaxNum);
        }

        // If new value is greater than start position, immediately snap to it
        if (newValue > startValue)
        {
            // Stop any ongoing animation
            bgFeature.isAnimating = false;
            // Immediately set to new value
            SetBackgroundFillAmount(bgFeature, newValue, MaxNum);
        }
        else
        {
            // HP goes down or stays same - animate from start position
            // Set up animation state
            StartBackgroundFillAnimation(startValue, newValue, bgFeature, MaxNum);
        }

        previousValue = newValue;
    }

    public static void StartBackgroundFillAnimation(float fromValue, float toValue, BackgroundFillFeature bgFeature, float max)
    {
        float valueDifference = Mathf.Abs(fromValue - toValue);
        if (valueDifference < 0.001f)
        {
            SetBackgroundFillAmount(bgFeature, toValue, max);
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
        float normalizedDifference = valueDifference / max;
        float speedMultiplier = bgFeature.speedCurve.Evaluate(normalizedDifference);
        float dynamicSpeed = bgFeature.animationSpeed * speedMultiplier;
        bgFeature.animationDuration = valueDifference / dynamicSpeed;
    }

    public static void UpdateBackgroundFillAnimation(BackgroundFillFeature bgFeature, float max)
    {
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
        SetBackgroundFillAmount(bgFeature, currentValue, max);

        // Check if animation is complete
        if (bgFeature.animationElapsed >= bgFeature.animationDuration)
        {
            SetBackgroundFillAmount(bgFeature, bgFeature.animationToValue, max);
            bgFeature.isAnimating = false;
        }
    }

    public static void SetBackgroundFillAmount(BackgroundFillFeature bgFeature, float amount, float max)
    {
        if (bgFeature != null && bgFeature.backgroundFill != null)
        {
            bgFeature.backgroundFill.fillAmount = amount / max;
        }
    }

    public static float GetBackgroundFillValue(BackgroundFillFeature bgFeature, float max)
    {
        return bgFeature.backgroundFill.fillAmount * max;
    }
}

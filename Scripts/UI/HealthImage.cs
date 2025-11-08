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

        // ColorGradientFeatureCommand();

        HealthSlider.FlashingFeatureCommand(GetFeature<FlashingFeature>(), CurrentNum, MaxNum);

        HealthSlider.UpdateBackgroundFillAnimation(GetFeature<BackgroundFillFeature>(), MaxNum);


        // UpdateBackgroundFillAnimation();
    }

    // Min/Max Health Feature


    // private void Display(TextDisplayFeature2 textDisplayFeature)
    // {
    //     if (textDisplayFeature.displayType == TextDisplayFeature2.DisplayType.Current)
    //         Display(textDisplayFeature.text, CurrentNum, textDisplayFeature.format);
    //     if (textDisplayFeature.displayType == TextDisplayFeature2.DisplayType.Max)
    //         Display(textDisplayFeature.text, MaxNum, textDisplayFeature.format);

    // }

    // private void Display(TMP_Text text, float num, string format)
    // {
    //     text.text = num.ToString(format);
    // }

    // Flashing Fill Rect Feature
    // private void FlashingFeatureCommand()
    // {
    //     float healthPercent = 1 > 0 ? Image.fillAmount / 1 : 0;
    //     healthPercent = Mathf.Clamp01(healthPercent);

    //     var flashingFeature = GetFeature<FlashingFeature>();
    //     flashingFeature.flashImage.gameObject.SetActive(healthPercent < flashingFeature.thresholdPercent);
    //     if (flashingFeature != null && healthPercent < flashingFeature.thresholdPercent)
    //     {
    //         // Calculate flashing based on time
    //         float flashValue = Mathf.Sin(Time.time * flashingFeature.flashSpeed) * 0.5f + 0.5f;
    //         Color flashColor = Color.Lerp(flashingFeature.flashColor1, flashingFeature.flashColor2, flashValue);
    //         flashingFeature.flashImage.color = flashColor;
    //         flashingFeature.flashImage.fillAmount = healthPercent;
    //     }
    // }


    // Color Gradient Health Feature
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

    // private void ColorGradientFeatureCommand()
    // {
    //     float healthPercent = 1 > 0 ? Image.fillAmount / 1 : 0;
    //     healthPercent = Mathf.Clamp01(healthPercent);

    //     // Color gradient: green (high) -> yellow (mid) -> red (low)
    //     var colorFeature = GetFeature<ColorGradientFeature>();
    //     var flashingFeature = GetFeature<FlashingFeature>();

    //     // If flashing feature is active and health is below threshold, let flashing handle the color
    //     if (flashingFeature != null && healthPercent < flashingFeature.thresholdPercent)
    //     {
    //         // Flashing feature will handle the color
    //         return;
    //     }

    //     if (colorFeature != null)
    //     {
    //         Color healthColor;
    //         if (healthPercent > 0.5f)
    //         {
    //             // Green to yellow
    //             float t = (healthPercent - 0.5f) * 2f;
    //             healthColor = Color.Lerp(colorFeature.colorAtHalfway, colorFeature.colorAtMax, t);
    //         }
    //         else
    //         {
    //             // Yellow to red
    //             float t = healthPercent * 2f;
    //             healthColor = Color.Lerp(colorFeature.colorAtMin, colorFeature.colorAtHalfway, t);
    //         }

    //         image.color = healthColor;
    //     }
    // }

    private void OnValueChangedInternal()
    {
        float newValue = CurrentNum;
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
        float normalizedDifference = 1 > 0 ? valueDifference / MaxNum : 0;
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
            bgFeature.backgroundFill.fillAmount = amount / MaxNum;
        }
    }

    private float GetBackgroundFillValue()
    {
        var bgFeature = GetFeature<BackgroundFillFeature>();
        if (bgFeature != null && bgFeature.backgroundFill != null)
        {
            return bgFeature.backgroundFill.fillAmount * MaxNum;
        }
        return 0;
    }
}

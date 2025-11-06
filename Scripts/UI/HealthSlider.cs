using System.Collections;
using System.Collections.Generic;
using JacobHomanics.HealthSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class FeatureToggle
{
    public string featureName;
    public bool enabled;

    // Text Display fields
    public TMP_Text textCurrent;
    public TMP_Text textMax;

    // Color Gradient fields
    public Color colorAtMin = Color.red;
    public Color colorAtHalfway = Color.yellow;
    public Color colorAtMax = Color.green;

    // Background Fill fields
    public Image backgroundFill;
    public bool keepSizeConsistent = true;
    public float animationSpeed = 10;
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 8.5f);
    public float delay = 1f;

    public FeatureToggle(string name)
    {
        featureName = name;
    }
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

    [SerializeField]
    private List<FeatureToggle> featureToggles = new List<FeatureToggle>
    {
        new FeatureToggle("Text Display"),
        new FeatureToggle("Color Gradient"),
        new FeatureToggle("Background Fill")
    };

    // Legacy fields for backward compatibility (kept for migration)
    // [SerializeField] private bool showText = false;
    // [SerializeField] private TMP_Text textCurrent;
    // [SerializeField] private TMP_Text textMax;
    // [SerializeField] private bool showColorGradient = false;
    // [SerializeField] private Color colorAtMin = Color.red;
    // [SerializeField] private Color colorAtHalfway = Color.yellow;
    // [SerializeField] private Color colorAtMax = Color.green;
    // [SerializeField] private bool showBackgroundFill = false;
    // [SerializeField] private Image backgroundFill;
    // [SerializeField] private bool keepSizeConsistent = true;
    // [SerializeField] private float animationSpeed = 10;
    // [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 8.5f);

    // Helper properties to get feature data

    private FeatureToggle GetFeature(string featureName)
    {
        return featureToggles.Find(f => f.featureName == featureName);
    }


    private float previousValue;
    private Coroutine animationCoroutine;

    void Update()
    {
        Slider.value = Health.Current;
        Slider.maxValue = Health.Max;

        var textFeature = GetFeature("Text Display");
        if (textFeature != null)
        {
            textFeature.textCurrent.text = Health.Current.ToString();
            textFeature.textMax.text = Health.Max.ToString();
        }

        float healthPercent = Slider.maxValue > 0 ? Slider.value / Slider.maxValue : 0;
        healthPercent = Mathf.Clamp01(healthPercent);

        // Color gradient: green (high) -> yellow (mid) -> red (low)
        var colorFeature = GetFeature("Color Gradient");

        Debug.Log(colorFeature);

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
        else
        {
            // Fallback to legacy colors
            Color healthColor;
            if (healthPercent > 0.5f)
            {
                float t = (healthPercent - 0.5f) * 2f;
                healthColor = Color.Lerp(colorFeature.colorAtHalfway, colorFeature.colorAtMax, t);
            }
            else
            {
                float t = healthPercent * 2f;
                healthColor = Color.Lerp(colorFeature.colorAtMin, colorFeature.colorAtHalfway, t);
            }
            slider.fillRect.GetComponent<Image>().color = healthColor;
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
        var bgFeature = GetFeature("Background Fill");
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
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            // Immediately set to new value
            SetBackgroundFillAmount(newValue);
        }
        else
        {
            // HP goes down or stays same - animate from start position
            // Animate the background fill from start position to the new value
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateBackgroundFill(startValue, newValue, bgFeature));
        }

        previousValue = newValue;
    }

    private void SetBackgroundFillAmount(float amount)
    {
        var bgFeature = GetFeature("Background Fill");
        Image fillImage = bgFeature.backgroundFill;
        if (fillImage != null)
        {
            fillImage.fillAmount = amount / slider.maxValue;
        }
    }

    private float GetBackgroundFillValue()
    {
        var bgFeature = GetFeature("Background Fill");
        Image fillImage = bgFeature.backgroundFill;
        if (fillImage != null)
        {
            return fillImage.fillAmount * slider.maxValue;
        }
        return 0;
    }

    private IEnumerator AnimateBackgroundFill(float fromValue, float toValue, FeatureToggle bgFeature)
    {
        float valueDifference = Mathf.Abs(fromValue - toValue);
        if (valueDifference < 0.001f)
        {
            SetBackgroundFillAmount(toValue);
            yield break;
        }

        yield return new WaitForSeconds(bgFeature.delay);

        // Calculate dynamic animation speed based on difference
        // Higher difference = faster animation, smaller differences animate slower
        float normalizedDifference = slider.maxValue > 0 ? valueDifference / slider.maxValue : 0;
        // Evaluate the speed curve to get the speed multiplier
        // X-axis (0-1): normalized difference, Y-axis: speed multiplier
        float speedMultiplier = bgFeature.speedCurve.Evaluate(normalizedDifference);
        float dynamicSpeed = bgFeature.animationSpeed * speedMultiplier;

        float elapsed = 0f;
        float duration = valueDifference / dynamicSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentValue = Mathf.Lerp(fromValue, toValue, t);
            SetBackgroundFillAmount(currentValue);
            yield return null;
        }

        SetBackgroundFillAmount(toValue);
        animationCoroutine = null;
    }

    private void OnEnable()
    {
        SetBackgroundFillAmount(slider.value);
    }
}

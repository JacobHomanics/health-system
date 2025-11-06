using System.Collections;
using JacobHomanics.HealthSystem;
using TMPro;
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

    [SerializeField] private TMP_Text textCurrent;
    public TMP_Text TextCurrent
    {
        get => textCurrent;
    }

    [SerializeField] private TMP_Text textMax;
    public TMP_Text TextMax
    {
        get => textMax;
    }

    [SerializeField] private Image backgroundFill;
    public Image BackgroundFill
    {
        get => backgroundFill;
    }

    [SerializeField] private bool keepSizeConsistent = true;

    [SerializeField] private Color colorAtMin = Color.red;
    [SerializeField] private Color colorAtHalfway = Color.yellow;
    [SerializeField] private Color colorAtMax = Color.green;

    [SerializeField] private float animationSpeed = 10;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 8.5f);

    private float previousValue;
    private Coroutine animationCoroutine;

    void Update()
    {
        Slider.value = Health.Current;
        Slider.maxValue = Health.Max;

        if (TextCurrent != null)
            TextCurrent.text = Health.Current.ToString();
        if (TextMax != null)
            TextMax.text = Health.Max.ToString();



        float healthPercent = Slider.maxValue > 0 ? Slider.value / Slider.maxValue : 0;
        healthPercent = Mathf.Clamp01(healthPercent);

        // Color gradient: green (high) -> yellow (mid) -> red (low)
        Color healthColor;
        if (healthPercent > 0.5f)
        {
            // Green to yellow
            float t = (healthPercent - 0.5f) * 2f;
            healthColor = Color.Lerp(colorAtHalfway, colorAtMax, t);
        }
        else
        {
            // Yellow to red
            float t = healthPercent * 2f;
            healthColor = Color.Lerp(colorAtMin, colorAtHalfway, t);
        }

        slider.fillRect.GetComponent<Image>().color = healthColor;
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
        // Get the starting value based on whether we want to keep size consistent
        float startValue;
        if (keepSizeConsistent)
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
            animationCoroutine = StartCoroutine(AnimateBackgroundFill(startValue, newValue));
        }

        previousValue = newValue;
    }

    private void SetBackgroundFillAmount(float amount)
    {
        backgroundFill.fillAmount = amount / slider.maxValue;
    }

    private float GetBackgroundFillValue()
    {
        return backgroundFill.fillAmount * slider.maxValue;
    }

    public float delay = 3f;

    private IEnumerator AnimateBackgroundFill(float fromValue, float toValue)
    {
        float valueDifference = Mathf.Abs(fromValue - toValue);
        if (valueDifference < 0.001f)
        {
            SetBackgroundFillAmount(toValue);
            yield break;
        }

        yield return new WaitForSeconds(delay);

        // Calculate dynamic animation speed based on difference
        // Higher difference = faster animation, smaller differences animate slower
        float normalizedDifference = slider.maxValue > 0 ? valueDifference / slider.maxValue : 0;
        // Evaluate the speed curve to get the speed multiplier
        // X-axis (0-1): normalized difference, Y-axis: speed multiplier
        float speedMultiplier = speedCurve.Evaluate(normalizedDifference);
        float dynamicSpeed = animationSpeed * speedMultiplier;

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

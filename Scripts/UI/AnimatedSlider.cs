using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimatedSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image backgroundFill;
    [SerializeField] private bool keepSizeConsistent = true;
    [SerializeField] private float animationSpeed = 10;
    [SerializeField] private Color colorAtMin = Color.red;
    [SerializeField] private Color colorAtMax = Color.green;

    private float previousValue;
    private Coroutine animationCoroutine;

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

    void Update()
    {
        // Normalize the value between min and max (0 to 1)
        float normalizedValue = Mathf.Clamp01((slider.value - slider.minValue) / (slider.maxValue - slider.minValue));
        // Interpolate between the two colors based on normalized value
        slider.fillRect.GetComponent<Image>().color = Color.Lerp(colorAtMin, colorAtMax, normalizedValue);
    }


    private float GetBackgroundFillValue()
    {
        return backgroundFill.fillAmount * slider.maxValue;
    }

    private IEnumerator AnimateBackgroundFill(float fromValue, float toValue)
    {
        float valueDifference = Mathf.Abs(fromValue - toValue);
        if (valueDifference < 0.001f)
        {
            SetBackgroundFillAmount(toValue);
            yield break;
        }

        float elapsed = 0f;
        float duration = valueDifference / animationSpeed;

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

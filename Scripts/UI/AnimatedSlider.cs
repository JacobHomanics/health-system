using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimatedSlider : Slider
{
    [SerializeField] private Image backgroundFill;
    private float animationSpeed = 10;

    private float previousValue;
    private Coroutine animationCoroutine;

    protected override void Awake()
    {
        backgroundFill = transform.Find("Background Fill").GetComponent<Image>();
        base.Awake();
        previousValue = value;
        onValueChanged.AddListener(OnValueChangedInternal);
    }

    protected override void OnDestroy()
    {
        onValueChanged.RemoveListener(OnValueChangedInternal);
        base.OnDestroy();
    }

    private void OnValueChangedInternal(float newValue)
    {
        if (backgroundFill != null)
        {
            // Get the current background fill value (where it is now, even if animating)
            float currentBackgroundValue = GetBackgroundFillValue();

            // If new value is greater than current background fill position, immediately snap to it
            if (newValue > currentBackgroundValue)
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
                // HP goes down or stays same - animate from current position
                // Animate the background fill from its current position to the new value
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }
                animationCoroutine = StartCoroutine(AnimateBackgroundFill(currentBackgroundValue, newValue));
            }
        }

        previousValue = newValue;
    }

    private void SetBackgroundFillAmount(float amount)
    {
        if (backgroundFill != null)
        {
            backgroundFill.fillAmount = amount / maxValue;
        }
    }

    private float GetBackgroundFillValue()
    {
        if (backgroundFill != null)
        {
            return backgroundFill.fillAmount * maxValue;
        }
        return previousValue;
    }

    private IEnumerator AnimateBackgroundFill(float fromValue, float toValue)
    {
        if (backgroundFill == null) yield break;

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

    protected override void OnEnable()
    {
        base.OnEnable();
        if (backgroundFill != null)
        {
            SetBackgroundFillAmount(value);
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        if (backgroundFill != null && Application.isPlaying)
        {
            SetBackgroundFillAmount(previousValue);
        }
    }
}

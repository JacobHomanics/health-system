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
        Debug.Log(animationSpeed);

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
            // Set the background fill to show the previous value
            SetBackgroundFillAmount(previousValue);

            // Animate the background fill to the new value
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateBackgroundFill(previousValue, newValue));
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

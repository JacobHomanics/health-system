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

    private float previousValue;

    private void Awake()
    {
        previousValue = CurrentNum;
    }

    // Main Health Slider 
    void Update()
    {
        image.fillAmount = CurrentNum / MaxNum;

        UIToolkit.HandleValueChange(CurrentNum, UIToolkit.GetFeature<BackgroundFillFeature>(featureToggles), ref previousValue, MaxNum);

        UIToolkit.Display(UIToolkit.GetFeature<TextDisplayFeature2>(featureToggles), CurrentNum, MaxNum);

        UIToolkit.ColorGradientFeatureCommand(UIToolkit.GetFeature<ColorGradientFeature>(featureToggles), image, CurrentNum, MaxNum);

        UIToolkit.FlashingFeatureCommand(UIToolkit.GetFeature<FlashingFeature>(featureToggles), CurrentNum, MaxNum);

        UIToolkit.UpdateBackgroundFillAnimation(UIToolkit.GetFeature<BackgroundFillFeature>(featureToggles), MaxNum);
    }

}

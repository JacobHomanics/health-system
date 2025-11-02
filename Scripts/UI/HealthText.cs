using JacobHomanics.HealthSystem;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthText : MonoBehaviour
{
    public Health health;
    public TMP_Text text;
    public enum DisplayType
    {
        Current, Max, Difference
    }
    public DisplayType displayType;

    void Update()
    {
        if (displayType == DisplayType.Current)
            text.text = health.Current.ToString();

        if (displayType == DisplayType.Max)
            text.text = health.Max.ToString();


        if (displayType == DisplayType.Difference)
            text.text = (health.Max - health.Current).ToString();

    }
}

using UnityEngine;
using UnityEngine.UI;

namespace JacobHomanics.TrickedOutUI
{
    /// <summary>
    /// MonoBehaviour component that handles animated background fill based on value changes.
    /// </summary>
    public class AnimatedImageFill : BaseAnimatedFill
    {
        public Image image;


        void Start()
        {
            image.fillAmount = X / Y;
        }

        void Update()
        {
            image.fillAmount = HandleValueChange(X, image.fillAmount, ref previousValue, Y, properties.delay, properties.animationDuration);
            image.fillAmount = UpdateAnimation(image.fillAmount, Y);
        }

        public void Reset()
        {
            if (!image)
                image = GetComponent<Image>();
        }
    }
}


using UnityEngine;
using UnityEngine.UI;

public class ConsumableUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Slider durationSlider;

    private float originalDuration;

    public void Initialize(Sprite icon, float duration)
    {
        iconImage.sprite = icon;
        originalDuration = duration;
        durationSlider.maxValue = duration;
        durationSlider.value = duration;
    }

    public void UpdateSlider(float timeRemaining)
    {
        durationSlider.value = timeRemaining;
    }

    public void ResetDuration(float duration)
    {
        originalDuration = duration;
        durationSlider.maxValue = duration;
        durationSlider.value = duration;
    }
}

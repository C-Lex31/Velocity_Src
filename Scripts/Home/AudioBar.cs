using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class AudioBar : MonoBehaviour, IDragHandler
{
    public enum SliderType
    {
        Effect,
        Music
    }
    public SliderType sliderType;
    Slider slider;

    public Color colorMute;
    public Color colorOn;
    public Image audioIcon;
    public Image iconMude;
    PopupPause popupPause;
    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.value = Value;
        //SetIcon();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (slider.value < 0)
        {

            slider.value = 0;
        }
        else if (slider.value > 1)
        {
            slider.value = 1;
        }

        Value = slider.value;
        if (sliderType == SliderType.Music)
        {
            SoundManager.Instance.SetBgmVolume();
        }
        else
        {
            SoundManager.Instance.SetEffectVolume();
        }

        SetIcon();
    }
    void SetIcon()
    {
        if (slider.value <= 0)
        {
            switch (sliderType)
            {
                case SliderType.Effect:
                    Data.PreviousVolumeEffect = 0.5f;
                    break;
                case SliderType.Music:
                    Data.PreviousVolumeMusic = 0.5f;
                    break;
            }

        }

    }
    float Value
    {
        get
        {
            if (sliderType == SliderType.Music)
            {
                return Data.VolumeMusic;
            }
            else
            {
                return Data.VolumeEffect;
            }
        }
        set
        {
            if (sliderType == SliderType.Music)
            {
                Data.VolumeMusic = value;
            }
            else
            {
                Data.VolumeEffect = value;
                Debug.Log(Data.VolumeEffect);
            }
        }
    }
}


using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
public class PopupPause : MonoBehaviour
{

    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textBest;
    [SerializeField] private Button sfxButton;
    [SerializeField] private Button musicButton;
    public bool isOn = false, bMuted = false;
    float prevEffectVol;
    Sprite sound, music, soundMute, musicMute;
    public void UIReset()
    {

        canvasGroup.DOFade(0f, 0f).SetUpdate(true);

        sound = Resources.Load<Sprite>($"{Data.path_UI_sprites}icon_audio");
        soundMute = Resources.Load<Sprite>($"{Data.path_UI_sprites}icon_audio_mute");
        music = Resources.Load<Sprite>($"{Data.path_UI_sprites}icon_music");
        musicMute = Resources.Load<Sprite>($"{Data.path_UI_sprites}icon_music_mute");
        if (Data.VolumeEffect > 0)
        {
            Data.PreviousVolumeEffect = Data.VolumeEffect;
            sfxButton.GetComponent<Image>().sprite = sound;
        }
        else
            sfxButton.GetComponent<Image>().sprite = soundMute;

        if (Data.VolumeMusic > 0)
        {
            Data.PreviousVolumeMusic = Data.VolumeMusic;
            musicButton.GetComponent<Image>().sprite = this.music;
        }
        else
            musicButton.GetComponent<Image>().sprite = this.musicMute;


    }

    private int c = 1;
    public void Open()
    {
        if (isOn) return;
        isOn = true;
        Time.timeScale = 0;



        textBest.text = GlobalGameData.BestScore.ToString();

        this.gameObject.SetActive(true);
        canvasGroup.DOKill();
        canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic).SetUpdate(true);

    }
    public void Close()
    {
        if (!isOn) return;
        isOn = false;
        SoundManager.Instance.PlayEffect(SoundList.sound_common_btn_in);

        Time.timeScale = 1;
        canvasGroup.DOKill();
        canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            this.gameObject.SetActive(false);

        }).SetUpdate(true);
    }

    public void Click_Home()
    {
        SoundManager.Instance.PlayEffect(SoundList.sound_common_btn_in);
        SoundManager.Instance.StopBGM();
        PlayManager.instance.GameOver();

    }
    public void Click_SFX()
    {

        if (Data.VolumeEffect > 0)
        {
            sfxButton.GetComponent<Image>().sprite = soundMute;
            Data.PreviousVolumeEffect = Data.VolumeEffect;
            Data.VolumeEffect = 0;
            SoundManager.Instance.SetEffectVolume();
        }
        else
        {
            sfxButton.GetComponent<Image>().sprite = sound;
            Data.VolumeEffect = Data.PreviousVolumeEffect;
            SoundManager.Instance.SetEffectVolume();

        }

    }


    public void Click_Music()
    {
        if (Data.VolumeMusic > 0)
        {
            musicButton.GetComponent<Image>().sprite = musicMute;
            Data.PreviousVolumeMusic = Data.VolumeMusic;
            Data.VolumeMusic = 0;
            SoundManager.Instance.SetBgmVolume();
        }
        else
        {
            musicButton.GetComponent<Image>().sprite = music;
            Data.VolumeMusic = Data.PreviousVolumeMusic;
            SoundManager.Instance.SetBgmVolume();
        }

    }

}

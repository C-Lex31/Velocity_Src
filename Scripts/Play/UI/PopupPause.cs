
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
public class PopupPause : MonoBehaviour
{

    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textBest;
    public bool isOn = false;
    public void UIReset()
    {
        canvasGroup.DOFade(0f, 0f).SetUpdate(true);

    }

    private int c = 1;
    public void Open()
    {
        if (isOn) return;
        isOn = true;
        Time.timeScale = 0;


       // textBest.text = Utility.ChangeThousandsSeparator(GameData.BestScore);
        textBest.text = GlobalGameData.BestScore.ToString();

        this.gameObject.SetActive(true);
        canvasGroup.DOKill();
        canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic).SetUpdate(true);
   
    }
     public void Close () {
        if (!isOn) return;
        isOn = false;
        SoundManager.Instance.PlayEffect(SoundList.sound_common_btn_in);

        Time.timeScale = 1;
        canvasGroup.DOKill();
        canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.OutCubic).OnComplete(() => {
            this.gameObject.SetActive(false);

        }).SetUpdate(true);
    }


}

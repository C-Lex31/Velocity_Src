using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class PanelBase: MonoBehaviour
{
        [HideInInspector] public bool isOpen = false;
    public CanvasGroup canvasGroup;

    /// <summary>
    /// Initialize all UI.
    /// </summary>
    public virtual void UIReset()
    {
        canvasGroup.DOFade(0f, 0f);
        canvasGroup.transform.DOScale(0.95f, 0f);
    }

    /// <summary>
    /// Data settings
    /// </summary>
    public virtual void SetData()
    {
    }
     /// <summary>
    /// Open panel
    /// </summary>
    public virtual void Open()
    {
        if (isOpen) return;
        isOpen = true;

     //   GameManager.Instance.panelBase = this;

        SoundManager.Instance.PlayEffect(SoundList.sound_common_btn_in);
        HomeManager.instance.SetHomeUI(false, canvasGroup);
    }
     /// <summary>
    /// Close panel
    /// </summary>
    public virtual void Close()
    {

        if (!isOpen) return;
        isOpen = false;

      //  PlayManager.Instance.panelBase = null;
        SoundManager.Instance.PlayEffect(SoundList.sound_common_btn_close);
         HomeManager.instance.SetHomeUI(true, canvasGroup);
    }
}

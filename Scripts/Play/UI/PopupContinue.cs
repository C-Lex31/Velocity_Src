using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class PopupContinue : MonoBehaviour
{


    public TextMeshProUGUI textScore;
    public CanvasGroup canvasGroup;
    public GameObject btnNoThanks;
     public GameObject buttonContinue;
    public GameObject popupBg;

    public GameObject iconAds;
    public TextMeshProUGUI textBest;

    /// <summary>
    /// Initialize all UI.
    /// </summary>
    public void UIReset()
    {
        canvasGroup.DOFade(0f, 0f);
        btnNoThanks.transform.DOScale(0f, 0f);
        popupBg.transform.DOScale(0f, 0f);

    }
      public void Open()
    {
        SoundManager.Instance.PlayEffect(SoundList.sound_continue_sfx_default);
       // iconAds.SetActive(false);

        PlayManager.instance.bIsGameOver = true;

        // textBest.text = World.ChangeCountFormat(RankingManager.Instance.rankingMyScore.score);
        textScore.text = 0.ToString();
        this.gameObject.SetActive(true);
        canvasGroup.DOKill();
        canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic);
        popupBg.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        StartCoroutine(ScoreAnimCo());
    }
     IEnumerator ScoreAnimCo()
    {
        int score = 0;
        int targetScore = GameManager.Instance.score;
        textBest.text= GlobalGameData.BestScore.ToString();
        float time = 0.5f;
        DOTween.To(() => score, x => score = x, targetScore, time).SetEase(Ease.Linear);

       // SoundManager.Instance.PlayEffectLoop(SoundList.sound_result_sfx_score);

        while (time > 0)
        {
            time -= Time.deltaTime;
            textScore.text = score.ToString();
            yield return null;
        }

       // SoundManager.Instance.StopEffectLoop();
        textScore.text = targetScore.ToString();
        
        yield return new WaitForSeconds(1f);

        btnNoThanks.transform.DOScale(1f, 0.25f).SetEase(Ease.OutCubic);


    }
}

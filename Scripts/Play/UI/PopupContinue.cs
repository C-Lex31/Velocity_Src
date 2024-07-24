using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
public class PopupContinue : MonoBehaviour
{


    public TextMeshProUGUI textScore;
    public CanvasGroup canvasGroup;
    public GameObject btnNoThanks;
    public GameObject buttonContinue;
    public GameObject popupBg;
    public TextMeshProUGUI keyCountText;
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
        Time.timeScale = 0;
        SoundManager.Instance.PlayEffect(SoundList.sound_continue_sfx_default);
        // iconAds.SetActive(false);

        PlayManager.instance.bIsGameOver = true;

        // textBest.text = World.ChangeCountFormat(RankingManager.Instance.rankingMyScore.score);
        textScore.text = 0.ToString();
        keyCountText.text = GlobalGameData.Key.ToString();
        this.gameObject.SetActive(true);
        canvasGroup.DOKill();

        canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic).SetUpdate(true);
        popupBg.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
        StartCoroutine(ScoreAnimCo());
    }
    IEnumerator ScoreAnimCo()
    {
        int score = 0;
        int targetScore = GameManager.Instance.score;
        textBest.text = GlobalGameData.BestScore.ToString();
        float time = 0.5f;
        DOTween.To(() => score, x => score = x, targetScore, time).SetEase(Ease.Linear).SetUpdate(true);

        // SoundManager.Instance.PlayEffectLoop(SoundList.sound_result_sfx_score);

        while (time > 0)
        {
            time -= Time.unscaledDeltaTime;
            textScore.text = score.ToString();
            yield return null;
        }

        // SoundManager.Instance.StopEffectLoop();
        textScore.text = targetScore.ToString();

        yield return new WaitForSecondsRealtime(1f);

        btnNoThanks.transform.DOScale(1f, 0.25f).SetEase(Ease.OutCubic).SetUpdate(true);


    }

    public void Click_NoThanks()
    {
        PlayManager.instance.LoadResultScene();
        SoundManager.Instance.StopBGM();
    }
    public void Click_Continue()
    {

        if (GlobalGameData.Key >= PlayManager.instance.reviveKeyCost)
        {
            GlobalGameData.Key -= PlayManager.instance.reviveKeyCost;
            PlayManager.instance.reviveKeyCost *= 2;
            PlayManager.instance.Continue();
        }
        else
            buttonContinue.GetComponent<Button>().enabled = false;
    }
    public void Close()
    {
        Time.timeScale = 1;
        canvasGroup.DOKill();
        canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.OutCubic).OnComplete(() => { this.gameObject.SetActive(false); });
    }
}

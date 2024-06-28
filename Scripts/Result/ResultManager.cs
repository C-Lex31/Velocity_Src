using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class ResultManager : MonoBehaviour
{
    public GameObject buttonHome;
    public GameObject buttonPlay;

    public TextMeshProUGUI textScore;
    public TextMeshProUGUI textBestScore;
    public TextMeshProUGUI textCoins;
    public TextMeshProUGUI textDistance;
    public GameObject objRibbon;

    int score = 0;
    int bestScore = 0;
    private bool bIsShow;
    private bool bIsExpAnimation;

    void Awake()
    {
        objRibbon.transform.DOScale(0f, 0f);
         buttonHome.transform.DOScale(0f, 0f);
         buttonPlay.transform.DOScale(0f, 0f);
        //  coinObject.transform.DOScale(0f, 0f);

         bestScore = GlobalGameData.BestScore;
        score = GlobalGameData.Score = GameManager.Instance.score;
         textBestScore.text = bestScore.ToString();
    }
    void Start()
    {
        SoundManager.Instance.PlayEffect(SoundList.sound_gameover_sfx_default);
        this.gameObject.SetActive(true);
        //Show results
        StartCoroutine(ShowResultsCo());
       
    }

    IEnumerator ShowResultsCo()
    {
        bIsShow = true;
        textCoins.text = GameManager.Instance.coins.ToString();
        textDistance.text = GameManager.Instance.distance.ToString();

        float time = 0.5f;
        int playScore = GameManager.Instance.score;
        DOTween.To(() => score, x => score = x, playScore, time).SetEase(Ease.Linear);
        while (time > 0)
        {
            time -= Time.deltaTime;
            textScore.text = score.ToString();
            yield return null;
        }
        textScore.text = playScore.ToString();

        if (playScore > bestScore)
        {

            time = 0.5f;
            objRibbon.SetActive(true);
            objRibbon.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.2f);
            //Score count animation
            DOTween.To(() => bestScore, x => bestScore = x, playScore, time).SetEase(Ease.Linear);
            while (time > 0)
            {
                time -= Time.deltaTime;
                textBestScore.text = bestScore.ToString();
                yield return null;
            }

            GlobalGameData.BestScore = playScore;
            textBestScore.text = playScore.ToString();
        }


        while (bIsExpAnimation)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        buttonHome.transform.DOScale(1f, 0.25f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.1f);
        buttonPlay.transform.DOScale(1f, 0.25f).SetEase(Ease.OutCubic);
    }

    public void Click_Play()
    {
        GameManager.Instance.bIsTouch = false;
        SoundManager.Instance.PlayEffect(SoundList.sound_common_btn_in);
        GameManager.Instance.LoadScene(Data.scene_play);
        SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_in);
    }

}

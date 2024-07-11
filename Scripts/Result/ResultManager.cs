using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
public class ResultManager : MonoBehaviour
{
    public GameObject buttonHome;
    public GameObject buttonPlay;

    public TextMeshProUGUI textScore;
    public TextMeshProUGUI textBestScore;
    public TextMeshProUGUI textCoins;
    public TextMeshProUGUI textBonusCoins;
    public TextMeshProUGUI textTotalCoins;
    public TextMeshProUGUI textDistance;
    public TextMeshProUGUI textTime;
    public GameObject objRibbon;
    public CanvasGroup TotalCoins;
    public AnimationClip animationClip;
    public Animator animator;
    int score = 0, bestScore = 0, coin = 0, distance = 0, performanceBonus = 0, totalCoins = 0;
    float timeRan = 0;
    private bool bIsShow;
    private bool bIsExpAnimation;

    void Awake()
    {
        objRibbon.transform.DOScale(0f, 0f);
        buttonHome.transform.DOScale(0f, 0f);
        buttonPlay.transform.DOScale(0f, 0f);
        TotalCoins.DOFade(0f, 0f);
        //  coinObject.transform.DOScale(0f, 0f);

        bestScore = GlobalGameData.BestScore;
        // score = GlobalGameData.Score = GameManager.Instance.score;
        textBestScore.text = bestScore.ToString();
        textTotalCoins.text=textBonusCoins.text=textDistance.text = textCoins.text = textScore.text = 0.ToString();
        textTime.text = String.Format("{0:00}:{1:00}", 0, 0);
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


        float time = 1f;
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

            time = 1f;
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

            //  GlobalGameData.BestScore = playScore;
            textBestScore.text = playScore.ToString();
        }

        time = 1f;
        DOTween.To(() => distance, x => distance = x, GameManager.Instance.distance, time).SetEase(Ease.Linear);
        while (time > 0)
        {
            time -= Time.deltaTime;
            textDistance.text = distance.ToString();
            yield return null;
        }
        textDistance.text = GameManager.Instance.distance.ToString();

        time = 1f;
        DOTween.To(() => timeRan, x => timeRan = x, GameManager.Instance.TimeRan, time).SetEase(Ease.Linear);
        while (time > 0)
        {
            time -= Time.deltaTime;
            int minutes = Mathf.FloorToInt(timeRan / 60);
            int seconds = Mathf.FloorToInt(timeRan % 60);

            textTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            yield return null;
        }
        textTime.text = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(GameManager.Instance.TimeRan / 60), Mathf.FloorToInt(GameManager.Instance.TimeRan % 60));

        time = 1f;
        DOTween.To(() => coin, x => coin = x, GameManager.Instance.coins, time).SetEase(Ease.Linear);
        while (time > 0)
        {
            time -= Time.deltaTime;
            textCoins.text = coin.ToString();
            yield return null;
        }
        textCoins.text = GameManager.Instance.coins.ToString();

        time = 1f;
        DOTween.To(() => performanceBonus, x => performanceBonus = x, GameManager.Instance.performanceBonus, time).SetEase(Ease.Linear);
        while (time > 0)
        {
            time -= Time.deltaTime;
            textBonusCoins.text = performanceBonus.ToString();
            yield return null;
        }
        textBonusCoins.text = GameManager.Instance.performanceBonus.ToString();

        animator.Play(animationClip.name);

        TotalCoins.DOFade(1, 0.5f);
        time = 1f;
        DOTween.To(() => totalCoins, x => totalCoins = x, GameManager.Instance.totalCoins, time).SetEase(Ease.Linear);
        while (time > 0)
        {
            time -= Time.deltaTime;
            textTotalCoins.text = totalCoins.ToString();
            yield return null;
        }
        textTotalCoins.text = GameManager.Instance.totalCoins.ToString();

        

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

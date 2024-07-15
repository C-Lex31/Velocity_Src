using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class CurrencyUI : MonoBehaviour
{
    public TextMeshProUGUI textCoin;
    public TextMeshProUGUI textCredits;
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }
    public void SetCoin()
    {
        textCoin.text = GlobalGameData.Coin.ToString();
    }
    public void SetCoin(int value)
    {
        StartCoroutine(SetCoinCo(value));
    }
    IEnumerator SetCoinCo(int coinValue)
    {
        int count = GlobalGameData.Coin - coinValue;
        float time = 0.5f;

        DOTween.To(() => count, x => count = x, GlobalGameData.Coin, time).SetEase(Ease.Linear);

        while (time > 0)
        {
            time -= Time.deltaTime;
            textCoin.text = count.ToString();
            yield return null;
        }

        textCoin.text = GlobalGameData.Coin.ToString();
    }
    public void SetCredits()
    {
        textCredits.text = GlobalGameData.Credits.ToString();
    }

    public void SetGem(int value)
    {
        StartCoroutine(SetGemCo(value));
    }

    IEnumerator SetGemCo(int gemValue)
    {
        int count = GlobalGameData.Credits - gemValue;
        float time = 0.5f;

        DOTween.To(() => count, x => count = x, GlobalGameData.Credits, time).SetEase(Ease.Linear);

        while (time > 0)
        {
            time -= Time.deltaTime;
            textCredits.text = count.ToString();
            yield return null;
        }

        textCredits.text =GlobalGameData.Credits.ToString();
    }

}

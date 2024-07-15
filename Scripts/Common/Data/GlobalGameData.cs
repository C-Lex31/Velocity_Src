using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameData : MonoBehaviour
{
    public static int BestScore
    {
        get { return PlayerPrefs.GetInt("BestScore", 0); }
        set { PlayerPrefs.SetInt("BestScore", value); }
    }
    public static int Save_Score
    {
        get { return PlayerPrefs.GetInt("Save_Score", 0); }
        set { PlayerPrefs.SetInt("Save_Score", value); }
    }
    public static int Score
    {
        get { return PlayerPrefs.GetInt("Save_Score", 0); }
        set { PlayerPrefs.SetInt("Save_Score", value); }
    }
    public static int Coin
    {
        get { return PlayerPrefs.GetInt("Coin", 0); }
        set
        {
            PlayerPrefs.SetInt("Coin", value);
            GameManager.Instance.commonUI._CurrencyUI.SetCoin();
        }
    }
    public static int Credits
    {
        get { return PlayerPrefs.GetInt("Credits", 0); }
        set
        {
            PlayerPrefs.SetInt("Credits", value);
            GameManager.Instance.commonUI._CurrencyUI.SetCredits();
        }
    }
    public static int Key
    {
        get { return PlayerPrefs.GetInt("Key", 0); }
        set
        {
            PlayerPrefs.SetInt("Key", value);
            //  GameManager.Instance.commonUI._CurrencyUI.SetCoin();
        }
    }


}

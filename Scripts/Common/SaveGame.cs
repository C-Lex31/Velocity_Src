using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveGame 
{
    public static void SaveCoins()
    {
        GlobalGameData.Coin += GameManager.Instance.savedCoins;

    }
    public static void SaveBestScore()
    {
        if (GameManager.Instance.score > GlobalGameData.BestScore)
        {
            GlobalGameData.BestScore = GameManager.Instance.score;
        }
    }
}

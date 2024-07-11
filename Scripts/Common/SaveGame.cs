using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveGame : MonoBehaviour
{
    public static void SaveProgress()
    {
        GlobalGameData.Coin += GameManager.Instance.totalCoins;
        if(GameManager.Instance.score>GlobalGameData.BestScore)
        {
            GlobalGameData.BestScore = GameManager.Instance.score;
        }
    }
}

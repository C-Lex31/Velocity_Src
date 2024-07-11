using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameData : MonoBehaviour
{
    public static int BestScore {
        get { return PlayerPrefs.GetInt("BestScore", 0); }
        set { PlayerPrefs.SetInt("BestScore", value); }
    }
        public static int Save_Score {
        get { return PlayerPrefs.GetInt("Save_Score", 0); }
        set { PlayerPrefs.SetInt("Save_Score", value); }
    }
        public static int Score {
        get { return PlayerPrefs.GetInt("Save_Score", 0); }
        set { PlayerPrefs.SetInt("Save_Score", value); }
    }
       public static int Coin {
        get { return PlayerPrefs.GetInt("Coin", 0); }
        set {
            PlayerPrefs.SetInt("Coin", value);
         //   PlayManager.Instance.commonUI._CoinGem.SetCoin();
        }
    }
        public static int Key {
        get { return PlayerPrefs.GetInt("Key", 0); }
        set {
            PlayerPrefs.SetInt("Key", value);
            //PlayManager.Instance.commonUI._CoinGem.SetCoin();
        }
    }


}

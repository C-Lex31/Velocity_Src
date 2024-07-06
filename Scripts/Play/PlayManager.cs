
using System.Collections;
//using Unity.VisualScripting;
using UnityEngine;

public class PlayManager : MonoBehaviour
{

    static PlayManager _instance;

    public static PlayManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayManager>();
            }

            return _instance;
        }
    }
    [HideInInspector] public bool bIsStart = false;
    [HideInInspector] public bool bIsGameOver = false;
    [HideInInspector] public bool bIsContinue = false;

    [HideInInspector] public float distance;
    [HideInInspector] public int coins;

    public ObjectPool<Coin> coinPool;

    public Coin coinPrefab; // Reference to the coin prefab
    void Start()
    {
       coinPool= new ObjectPool<Coin>(coinPrefab , 65 ,150);
        //Start game here 
        //Start BGM 
        //
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.instance)
        {
            if (Player.instance.transform.position.x > distance)
                distance = Player.instance.transform.position.x;
        }
    }

    public void Continue()
    {
         if (bIsContinue) return;
        bIsContinue = true;

        //Resume BGM
        //Close Continue popup from UI_Manager 
        StartCoroutine(ContinueCo());
    }
    IEnumerator ContinueCo()
    {
         yield return new WaitForSeconds(0.1f);
         //Continue Player 
    }
    public void GameOver()
    {

        GameManager.Instance.distance = (int)distance;
        GameManager.Instance.coins =coins;
        GameManager.Instance.score= (int)distance *  coins;
        bIsGameOver = true;
        //Pause BGM 
        if (bIsContinue)
        {
            //Load result scene form game manager
        }
        else
        {
            //Open Continue pop-up from UI_Manager
            UI_Manager.instance._PopupContinue.Open();
        }

    }
}

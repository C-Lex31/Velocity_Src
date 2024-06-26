
using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    [HideInInspector] public int score;
    [HideInInspector] public int coins;
    [HideInInspector] public int distance;
    [HideInInspector] public bool isSaveGameStart = false;

    void Start()
    {

    }

    void Update()
    {

    }
}

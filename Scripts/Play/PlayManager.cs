using System.Collections;
using System.Collections.Generic;
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


    [HideInInspector] public int reviveKeyCost = 1; // Initial cost to revive
    public ObjectPool<Coin> coinPool;
    public Coin coinPrefab; // Reference to the coin prefab
    private Dictionary<string, Coroutine> activeConsumables = new Dictionary<string, Coroutine>();
    private Dictionary<string, List<ConsumableBase>> activeConsumableInstances = new Dictionary<string, List<ConsumableBase>>();
    void Start()
    {
        coinPool = new ObjectPool<Coin>(coinPrefab, 65, 150);
        GlobalGameData.Key += 10;
        reviveKeyCost = 1;
        GameManager.Instance.TimeRan = 0f;
        // Start game here
        // Start BGM
        StartCoroutine(SaveProgressPeriodically());
    }

    void Update()
    {
        if (Player.instance)
        {
            GameManager.Instance.TimeRan += Time.deltaTime;
            if (Player.instance.transform.position.x > GameManager.Instance.distance)
                GameManager.Instance.distance = (int)Player.instance.transform.position.x;

        }
    }

    public void Continue()
    {
        //if (bIsContinue) return;
        // bIsContinue = true;
        // Resume BGM
        // Close Continue popup from UI_Manager
        UI_Manager.instance._PopupContinue.Close();

        StartCoroutine(ContinueCo());
    }

    IEnumerator ContinueCo()
    {
        Transform RevivePos = LevelGenerator.instance.SpawnSafeSegment();
        Player.instance.transform.position = RevivePos.position;

        // Continue player from the current state
        // Continue Player
        ParticleManager.instance.PlayParticleEffect(ParticleList.explosion, RevivePos.position, Quaternion.identity);
        yield return null;
        Player.instance.Revive(RevivePos);



    }

    public void GameOver(bool bShowContinuePopup = false)
    {

        GameManager.Instance.Save();

        bIsGameOver = true;

        // Pause BGM
        if (bShowContinuePopup)
        {
            ClearAllIndependentObjects();
            LevelGenerator.instance.ClearOldSegments();
            UI_Manager.instance._PopupContinue.Open();
        }
        else
        {
            GameManager.Instance.LoadScene(Data.scene_result);
        }
    }

    private IEnumerator SaveProgressPeriodically()
    {
        while (Application.IsPlaying(this))
        {
            yield return new WaitForSeconds(1); // Save progress every 30 seconds
            GameManager.Instance.Save();
        }
    }
    private void ClearAllIndependentObjects()
    {
        Coin[] coins = GameObject.FindObjectsOfType<Coin>();
        foreach (Coin coin in coins)
        {
            Coin coinComponent = coin.GetComponent<Coin>();
            if (coinComponent != null)
            {
                coinPool.ReturnObject(coinComponent);
            }
            else
            {
                Destroy(coin);
            }
        }
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            Destroy(item);
        }

    }

    public void ActivateConsumable(string consumableName, float duration, Sprite icon, ConsumableBase consumableInstance)
    {
        if (activeConsumables.ContainsKey(consumableName))
        {
            StopCoroutine(activeConsumables[consumableName]);
            activeConsumables[consumableName] = StartCoroutine(ManageConsumable(consumableName, duration));
        }
        else
        {
            Coroutine coroutine = StartCoroutine(ManageConsumable(consumableName, duration));
            activeConsumables.Add(consumableName, coroutine);
        }

        if (!activeConsumableInstances.ContainsKey(consumableName))
        {
            activeConsumableInstances[consumableName] = new List<ConsumableBase>();
        }
        activeConsumableInstances[consumableName].Add(consumableInstance);

        UI_Manager.instance.AddOrUpdateConsumableUI(consumableName, icon, duration);
    }

    private IEnumerator ManageConsumable(string consumableName, float duration)
    {
        float timeRemaining = duration;
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UI_Manager.instance.UpdateConsumableUI(consumableName, timeRemaining);
            yield return null;
        }

        DeactivateConsumable(consumableName);
        UI_Manager.instance.RemoveConsumableUI(consumableName);
        activeConsumables.Remove(consumableName);
    }

    private void DeactivateConsumable(string consumableName)
    {
        if (activeConsumableInstances.ContainsKey(consumableName))
        {
            foreach (var instance in activeConsumableInstances[consumableName])
            {
                instance.DeactivatePowerUp();
            }
            activeConsumableInstances.Remove(consumableName);
        }
    }
}

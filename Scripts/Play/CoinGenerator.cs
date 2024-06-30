using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoinGenerator : MonoBehaviour
{
    //public Coin coinPrefab; // Reference to the coin prefab
    public int poolSize = 20; // Number of coins to keep in the pool
    public int minCoins = 3; // Minimum number of coins to spawn
    public int maxCoins = 7; // Maximum number of coins to spawn
    public float spacing = 1.0f; // Spacing between coins
    public Vector3 offset = Vector3.zero; // Offset to start spawning coins
    public float destroyDelay = 2.0f; // Time to wait before deactivating the coin after it goes out of view

    private ObjectPool<Coin> coinPool;
    private List<Coin> coins = new List<Coin>();
    private Camera mainCamera;

    private void Start()
    {
        Debug.Log("CoinGenerator Start called for: " + gameObject.name);
        mainCamera = Camera.main;
        
        coinPool = PlayManager.instance.coinPool;
        SpawnCoins();
    }

    private void Update()
    {
        CheckCoinsOutOfView();
    }

    private void SpawnCoins()
    {
        int numberOfCoins = Random.Range(minCoins, maxCoins + 1); // Randomize the number of coins
        Vector3 spawnPosition = transform.position + offset;

        Debug.Log("Positioning" + numberOfCoins + " coins at " + gameObject.name);

        for (int i = 0; i < numberOfCoins; i++)
        {
            Coin coin = coinPool.GetObject();
            coin.gameObject.SetActive(true); // Ensure the coin is active
            coin.transform.position = spawnPosition;
            coins.Add(coin);
            spawnPosition += new Vector3(spacing, 0, 0); // Adjust this for vertical or different spacing
        }
    }

    private void CheckCoinsOutOfView()
    {
        foreach (Coin coin in coins)
        {
            if (coin != null)
            {
                bool isOutOfView = IsInView(coin.transform.position);
               //Debug.Log($"Coin {coin.gameObject.name} is out of view: {isOutOfView}");
                if (isOutOfView)
                {
                    if (!coin.isOutOfView)
                    {
                        coin.isOutOfView = true;
                        StartCoroutine(DeactivateCoinAfterDelay(coin));
                    }
                }
                else
                {
                    coin.isOutOfView = false;
                }
            }
        }
    }

    private bool IsInView(Vector3 position)
    {
       // Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);
       // return viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1 && viewportPoint.z > 0;
   
      Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);

    // Check if the coin is to the left of the camera's view
    bool isPassedCamera = viewportPoint.x < 0;

    return isPassedCamera;
    }

    private IEnumerator DeactivateCoinAfterDelay(Coin coin)
    {
        yield return new WaitForSeconds(destroyDelay);
        if (coin != null && coin.isOutOfView)
        {
            coin.isOutOfView = false;
            coin.gameObject.SetActive(false); // Deactivate the coin
            coinPool.ReturnObject(coin);
            coins.Remove(coin);
        }
    }

    public void ReturnCoin(Coin coin)
    {
        coinPool.ReturnObject(coin);
    }

    // Gizmo helpers to visualize the coin generator in the level editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + offset, 0.5f);

        int numberOfCoins = Mathf.Max(minCoins, maxCoins);
        Vector3 gizmoPosition = transform.position + offset;

        for (int i = 0; i < numberOfCoins; i++)
        {
            Gizmos.DrawWireSphere(gizmoPosition, 0.5f);
            gizmoPosition += new Vector3(spacing, 0, 0); // Adjust this if you want vertical or different spacing
        }
    }
}

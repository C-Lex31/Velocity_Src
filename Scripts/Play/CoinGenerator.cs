using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoinGenerator : MonoBehaviour, IResettable
{
    public int minCoins = 3; // Minimum number of coins to spawn
    public int maxCoins = 7; // Maximum number of coins to spawn
    public float spacing = 1.0f; // Spacing between coins
    public Vector3 offset = Vector3.zero; // Offset to start spawning coins
    public float destroyDelay = 0.8f; // Time to wait before deactivating the coin after it goes out of view
    private float startCheckDelay = 0.5f; // Delay before starting the out-of-view check

    private ObjectPool<Coin> coinPool;
    [SerializeField] private List<Coin> coins = new List<Coin>();
    private Camera mainCamera;
    //public static CoinGenerator Instance { get; private set; }   BadIdea

    private void Start()
    {
        mainCamera = Camera.main;
        coinPool = PlayManager.instance.coinPool;
        SpawnCoins();

    }

    public void ResetState()
    {
        mainCamera = Camera.main;
        coinPool = PlayManager.instance.coinPool;
        SpawnCoins();
    }
    private void SpawnCoins()
    {
        int numberOfCoins = Random.Range(minCoins, maxCoins + 1); // Randomize the number of coins
        Vector3 spawnPosition = transform.position + offset;

        for (int i = 0; i < numberOfCoins; i++)
        {
            Coin coin = coinPool.GetObject();
            coin.gameObject.SetActive(true); // Ensure the coin is active
            coin.transform.position = spawnPosition;
            coin.SetGenerator(this); // Set the generator reference
            coins.Add(coin);
            spawnPosition += new Vector3(spacing, 0, 0); // Adjust this for vertical or different spacing
        }
        StartCoroutine(StartCheckCoinsOutOfViewAfterDelay(startCheckDelay));
    }

    private IEnumerator StartCheckCoinsOutOfViewAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(CheckCoinsOutOfView());

    }

    private IEnumerator CheckCoinsOutOfView()
    {
        while (coins.Count > 0)
        {
            foreach (Coin coin in coins)

            {
                //Coin coin = coins[i];
                if (coin != null && IsOutOfView(coin.transform.position))
                {
                    StartCoroutine(DeactivateCoinAfterDelay(coin));
                }
            }

            yield return new WaitForSeconds(0.1f); // Check every 0.5 seconds
        }
        //   StopCoroutine(CheckCoinsOutOfView());
    }

    private bool IsOutOfView(Vector3 position)
    {
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);

        // Check if the coin is to the left of the camera's view
        return viewportPoint.x < 0;
    }

    private IEnumerator DeactivateCoinAfterDelay(Coin coin)
    {
        yield return new WaitForSeconds(destroyDelay);
        if (coin != null && coin.gameObject.activeSelf)
        {
            ReturnCoin(coin);
        }
    }

    public void ReturnCoin(Coin coin)
    {
        coinPool.ReturnObject(coin);
        coins.Remove(coin);
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

    public void ClearState()
    {
        throw new System.NotImplementedException();
    }
}

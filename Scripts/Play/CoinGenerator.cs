using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CoinGenerator : MonoBehaviour
{
    [Header("Coin Spawn Settings")]
    [Range(1, 20)] public int minCoins = 3; // Minimum number of coins to spawn
    [Range(1, 20)] public int maxCoins = 7; // Maximum number of coins to spawn
    public float spacing = 1.0f; // Spacing between coins
    public Vector3 offset = Vector3.zero; // Offset to start spawning coins
    public Spline spline; // Reference to the spline

    [Header("Coin Deactivation Settings")]
    public float destroyDelay = 0.8f; // Time to wait before deactivating the coin after it goes out of view
    private float startCheckDelay = 0.5f; // Delay before starting the out-of-view check

    private ObjectPool<Coin> coinPool;
    private List<Coin> activeCoins = new List<Coin>();
    private Camera mainCamera;


    private void Start()
    {
        mainCamera = Camera.main;
        coinPool = PlayManager.instance.coinPool;
        SpawnCoins();
    }

    private void SpawnCoins()
    {
        int numberOfCoins = Random.Range(minCoins, maxCoins + 1);
        Vector3 spawnPosition = transform.position + offset;

        for (int i = 0; i < numberOfCoins; i++)
        {
            if (spline)
            {
                float t = i / (float)(numberOfCoins - 1); // Normalized position along the spline
                Vector3 coinPosition = spline.transform.TransformPoint(spline.GetPoint(t));

                Coin coin = coinPool.GetObject();
                coin.gameObject.SetActive(true);
                coin.transform.position = coinPosition;
                coin.SetGenerator(this);
                activeCoins.Add(coin);
            }
            else
            {
                Coin coin = coinPool.GetObject();
                coin.gameObject.SetActive(true);
                coin.transform.position = spawnPosition;
                coin.SetGenerator(this);
                activeCoins.Add(coin);
                spawnPosition += new Vector3(spacing, 0, 0);
            }

        }

        StartCoroutine(CheckCoinsOutOfViewAfterDelay(startCheckDelay));
    }

    private IEnumerator CheckCoinsOutOfViewAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(CheckCoinsOutOfView());
    }

    private IEnumerator CheckCoinsOutOfView()
    {
        while (activeCoins.Count > 0)
        {
            for (int i = activeCoins.Count - 1; i >= 0; i--)
            {
                Coin coin = activeCoins[i];
                if (coin != null && !coin.isOutOfView && IsOutOfView(coin.transform.position))
                {
                    coin.isOutOfView = true;
                    StartCoroutine(DeactivateCoinAfterDelay(coin));
                }
            }

            yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
        }
    }

    private bool IsOutOfView(Vector3 position)
    {
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);
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
        activeCoins.Remove(coin);
        coinPool.ReturnObject(coin);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + offset, 0.5f);
        Vector3 gizmoPosition = transform.position + offset;

        int numberOfCoins = Mathf.Max(minCoins, maxCoins);


        for (int i = 0; i < numberOfCoins; i++)
        {
            if (spline && spline.GetControlPointsCount() >= 4 )
            {
                float t = i / (float)(numberOfCoins - 1);
               Vector3 coinPosition =  spline.transform.TransformPoint(spline.GetPoint(t));
                Gizmos.DrawWireSphere(coinPosition, 0.5f);
            }
            else
            {
                Gizmos.DrawWireSphere(gizmoPosition, 0.5f);
                gizmoPosition += new Vector3(spacing, 0, 0);
            }

        }

    }
}

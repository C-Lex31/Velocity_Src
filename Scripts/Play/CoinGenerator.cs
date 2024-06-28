using UnityEngine;

public class CoinGenerator : MonoBehaviour
{
    public GameObject coinPrefab; // Reference to the coin prefab
    public int minCoins = 3; // Minimum number of coins to spawn
    public int maxCoins = 7; // Maximum number of coins to spawn
    public float spacing = 1.0f; // Spacing between coins
    public Vector3 offset = Vector3.zero; // Offset to start spawning coins

    private void Start()
    {
        SpawnCoins();
    }

    private void SpawnCoins()
    {
        int numberOfCoins = Random.Range(minCoins, maxCoins + 1); // Randomize the number of coins
        Vector3 spawnPosition = transform.position + offset;

        for (int i = 0; i < numberOfCoins; i++)
        {
            Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            spawnPosition += new Vector3(spacing, 0, 0); // Adjust this if you want vertical or different spacing
        }
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

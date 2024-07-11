using System;
using System.Threading;
using UnityEngine;


[System.Serializable]
public class PowerupSpawnInfo
{
    public GameObject powerupPrefab;
    [Range(0f, 1f)]
    public float spawnWeight = 0.5f; // Weight for spawning this power-up
}

[System.Serializable]
public class CollectibleSpawnInfo
{
    public GameObject collectiblePrefab;
    [Range(0f, 1f)]
    public float spawnWeight = 0.5f; // Weight for spawning this collectible
}

public class ItemGenerator : MonoBehaviour
{
    public PowerupSpawnInfo[] powerupSpawnInfos; // Array of power-ups with spawn weights
    public CollectibleSpawnInfo[] collectibleSpawnInfos; // Array of collectibles with spawn weights
    private GameObject spawnedItem;

    void Start()
    {
        SpawnRandomItem();
    }

    // Call this method when you want to spawn a random item
    public void SpawnRandomItem()
    {
        if (powerupSpawnInfos.Length == 0 && collectibleSpawnInfos.Length == 0)
        {
            Debug.LogWarning("No power-up or collectible spawn info assigned!");
            return;
        }

        // Check if power-up info array is empty
        if (powerupSpawnInfos.Length == 0)
        {
            SpawnCollectibleByWeight();
        }
        // Check if collectible info array is empty
        else if (collectibleSpawnInfos.Length == 0)
        {
            SpawnPowerupByWeight();
        }
        else
        {
            float randomValue = UnityEngine.Random.value; // Generate random value between 0 and 1

            // Determine whether to spawn a power-up, collectible, or both based on weights
            if (randomValue < 0.5f) // Example: 50% chance for power-up
            {
                SpawnPowerupByWeight();
            }
            else // Example: 50% chance for collectible
            {
                SpawnCollectibleByWeight();
            }
        }
    }

    private void SpawnPowerupByWeight()
    {
        if (powerupSpawnInfos.Length == 0)
        {
            Debug.LogWarning("No power-up spawn info assigned!");
            return;
        }

        // Calculate total spawn weight for power-ups
        float totalSpawnWeight = 0f;
        foreach (var powerupInfo in powerupSpawnInfos)
        {
            totalSpawnWeight += powerupInfo.spawnWeight;
        }

        // Generate a random value within the total spawn weight range
        float randomValue = UnityEngine.Random.Range(0f, totalSpawnWeight);
        float cumulativeWeight = 0f;

        // Iterate through each power-up spawn info to select one based on weight
        foreach (var powerupInfo in powerupSpawnInfos)
        {
            cumulativeWeight += powerupInfo.spawnWeight;
            if (randomValue < cumulativeWeight)
            {
                GameObject powerupPrefab = powerupInfo.powerupPrefab;
                Instantiate(powerupPrefab, transform.position, Quaternion.identity);
                return; // Exit method after spawning one power-up
            }
        }
    }

    private void SpawnCollectibleByWeight()
    {
        if (collectibleSpawnInfos.Length == 0)
        {
            Debug.LogWarning("No collectible spawn info assigned!");
            return;
        }

        // Calculate total spawn weight for collectibles
        float totalSpawnWeight = 0f;
        foreach (var collectibleInfo in collectibleSpawnInfos)
        {
            totalSpawnWeight += collectibleInfo.spawnWeight;
        }

        // Generate a random value within the total spawn weight range
        float randomValue = UnityEngine.Random.Range(0f, totalSpawnWeight);
        float cumulativeWeight = 0f;

        // Iterate through each collectible spawn info to select one based on weight
        foreach (var collectibleInfo in collectibleSpawnInfos)
        {
            cumulativeWeight += collectibleInfo.spawnWeight;
            if (randomValue < cumulativeWeight)
            {
                GameObject collectiblePrefab = collectibleInfo.collectiblePrefab;
                spawnedItem=Instantiate(collectiblePrefab, transform.position, Quaternion.identity);
                return; // Exit method after spawning one collectible
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }


}

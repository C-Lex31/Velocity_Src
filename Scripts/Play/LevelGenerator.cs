using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private Transform levelStart;
    [SerializeField] private Transform[] easyLevelParts;
    [SerializeField] private Transform[] mediumLevelParts;
    [SerializeField] private Transform[] hardLevelParts;
    [SerializeField] private float distanceToSpawn = 50f;
    [SerializeField] private float distanceToDelete = 60f;
    private Queue<Transform> activePlatforms = new Queue<Transform>();
    public Dictionary<Transform, ObjectPool<Transform>> platformPools = new Dictionary<Transform, ObjectPool<Transform>>();

    private Vector3 nextPartPosition;
    private int levelSpawnCount;

    private List<Transform> shuffledLevelParts;
    private int shuffledIndex = 0;

    private enum LevelDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    private void Start()
    {
        InitializePools();
        nextPartPosition = levelStart.Find("EndPoint").position;
        GenerateInitialPlatforms();
    }

    private void Update()
    {
        DeletePlatform();
        GeneratePlatform();
    }

    private void InitializePools()
    {
        InitializePoolForLevelParts(easyLevelParts);
        InitializePoolForLevelParts(mediumLevelParts);
        InitializePoolForLevelParts(hardLevelParts);

        InitializeShuffledParts(easyLevelParts); // Initialize shuffled parts for the starting difficulty
    }

    private void InitializePoolForLevelParts(Transform[] levelParts)
    {
        foreach (var part in levelParts)
        {
            if (!platformPools.ContainsKey(part))
            {
                platformPools[part] = new ObjectPool<Transform>(part, levelParts.Length);
            }
        }
    }

    private void InitializeShuffledParts(Transform[] levelParts)
    {
        shuffledLevelParts = new List<Transform>(levelParts);
        Shuffle(shuffledLevelParts);
        shuffledIndex = 0;
    }

    private void GenerateInitialPlatforms()
    {
        for (int i = 0; i < easyLevelParts.Length / 2; i++)
        {
            GeneratePlatform();
        }
    }

    private void GeneratePlatform()
    {
        while (Vector3.Distance(Player.instance.transform.position, nextPartPosition) < distanceToSpawn)
        {
            if (shuffledIndex >= shuffledLevelParts.Count)
            {
                Shuffle(shuffledLevelParts);
                shuffledIndex = 0;
            }

            Transform partPrefab = shuffledLevelParts[shuffledIndex];
            shuffledIndex++;

            Transform newPart = platformPools[partPrefab].GetObject();

            Vector3 newPosition = new Vector3(nextPartPosition.x, 0, 0);
            newPart.position = newPosition;
            newPart.SetParent(transform);

            // Call ResetState on all IResettable components
            foreach (var resettable in newPart.GetComponentsInChildren<IResettable>())
            {
                resettable.ResetState();
            }

            activePlatforms.Enqueue(newPart);
            nextPartPosition = newPart.Find("EndPoint").position;

            Debug.Log("New part spawned. New part position: " + newPart.position + ", Next part position: " + nextPartPosition);

            levelSpawnCount++;
        }
    }

    private void DeletePlatform()
    {
        if (activePlatforms.Count > 0)
        {
            Transform partToDelete = activePlatforms.Peek();

            if (Vector3.Distance(Player.instance.transform.position, partToDelete.position) > distanceToDelete)
            {
                activePlatforms.Dequeue();
                foreach (var pool in platformPools)
                {
                    if (pool.Value.ContainsObject(partToDelete))
                    {
                        pool.Value.ReturnObject(partToDelete);
                        Debug.Log("Returned part to pool: " + partToDelete.name);
                        break;
                    }
                }
            }
        }
    }

    private Transform[] GetLevelParts()
    {
        if (levelSpawnCount >= 10) return hardLevelParts;
        if (levelSpawnCount >= 5) return mediumLevelParts;
        return easyLevelParts;
    }

    private void Shuffle(List<Transform> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Transform temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private void SpawnTrapsAndItems()
    {

    }



}

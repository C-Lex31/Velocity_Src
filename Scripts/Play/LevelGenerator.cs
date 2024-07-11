using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    static LevelGenerator _instance;
    public static LevelGenerator instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelGenerator>();
            }
            return _instance;
        }
    }
    [SerializeField] private Transform[] easyLevelSegments;
    [SerializeField] private Transform[] mediumLevelSegments;
    [SerializeField] private Transform[] hardLevelSegments;
    [SerializeField] private Transform specialLevelSegment; // Reference to the special level segment
    [SerializeField] private float distanceToSpawn = 10f;
    [SerializeField] private float distanceToDelete = 20f;

    private int levelSpawnCount = 0;
    private Transform[] levelSegments;
    private Vector3 nextSegmentPosition;
    private Queue<Transform> activeSegments = new Queue<Transform>();
    private List<Transform> shuffledLevelSegments;
    private int shuffledIndex = 0;

    private Transform currentSegment;

    private enum LevelDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    void Start()
    {

    }

    void Update()
    {
        DeleteSegment();
        GenerateSegment();
    }



    private void GenerateSegment()
    {
        while (Vector2.Distance(Player.instance.transform.position, nextSegmentPosition) < distanceToSpawn)
        {
            switch (GetLevelDifficulty())
            {
                case LevelDifficulty.Easy:
                    levelSegments = easyLevelSegments;
                    break;
                case LevelDifficulty.Medium:
                    levelSegments = mediumLevelSegments;
                    break;
                case LevelDifficulty.Hard:
                    levelSegments = hardLevelSegments;
                    break;
                default:
                    levelSegments = easyLevelSegments;
                    break;
            }

            if (shuffledLevelSegments == null || shuffledLevelSegments.Count != levelSegments.Length)
            {
                shuffledLevelSegments = new List<Transform>(levelSegments);
                Shuffle(shuffledLevelSegments);
                shuffledIndex = 0;
            }

            Transform segment = shuffledLevelSegments[shuffledIndex];
            shuffledIndex = (shuffledIndex + 1) % shuffledLevelSegments.Count;

            Vector2 newPosition = new Vector2(nextSegmentPosition.x - segment.Find("StartPoint").position.x, 0);
            Transform newSegment = Instantiate(segment, newPosition, transform.rotation, transform);

            nextSegmentPosition = newSegment.Find("EndPoint").position;
            activeSegments.Enqueue(newSegment);

            levelSpawnCount++;
        }
    }

    private void DeleteSegment()
    {
        if (currentSegment == null && activeSegments.Count > 0)
        {
            currentSegment = activeSegments.Peek();
        }

        if (currentSegment != null)
        {
            Transform endPoint = currentSegment.Find("EndPoint");

            if (Player.instance.transform.position.x > endPoint.position.x)
            {
                while (activeSegments.Count > 0)
                {
                    Transform segmentToDelete = activeSegments.Peek();
                    Transform startPoint = segmentToDelete.Find("StartPoint");

                    if (Player.instance.transform.position.x - startPoint.position.x > distanceToDelete)
                    {
                        activeSegments.Dequeue();
                        Destroy(segmentToDelete.gameObject);
                    }
                    else
                    {
                        currentSegment = segmentToDelete;
                        break;
                    }
                }
            }
        }
    }

    private LevelDifficulty GetLevelDifficulty()
    {
        // if (levelSpawnCount >= 10) return LevelDifficulty.Hard;
        // if (levelSpawnCount >= 5) return LevelDifficulty.Medium;
        return LevelDifficulty.Easy;
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
    public void ClearOldSegments()
    {
        while (activeSegments.Count > 0)
        {
            Transform segmentToDelete = activeSegments.Dequeue();
            Destroy(segmentToDelete.gameObject);
        }


    }
    public Transform SpawnSafeSegment()
    {
        Vector2 newPosition = new Vector2(Player.instance.transform.position.x, 0);
        Transform newSegment = Instantiate(specialLevelSegment, newPosition, transform.rotation, transform);
      //  Debug.Log(RevivePos);
        nextSegmentPosition = newSegment.Find("EndPoint").position;
        activeSegments.Enqueue(newSegment);
        currentSegment = newSegment;

        // Return the child transform used for player positioning
        return newSegment.Find("RevivePosition");
    }
}

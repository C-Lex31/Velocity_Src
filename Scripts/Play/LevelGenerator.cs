using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private Transform[] initialLevelSegments;
    private void Start()
    {
        levelSegments = initialLevelSegments;
        TeleportSystem.instance.OnThemeChanged += OnThemeChanged; // Subscribe to the theme change event
    }

    private void Update()
    {
        DeleteSegment();
        GenerateSegment();
    }

    private void GenerateSegment()
    {
        while (Vector2.Distance(Player.instance.transform.position, nextSegmentPosition) < distanceToSpawn)
        {
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
        nextSegmentPosition = newSegment.Find("EndPoint").position;
        activeSegments.Enqueue(newSegment);
        currentSegment = newSegment;

        // Return the child transform used for player positioning
        return newSegment.Find("RevivePosition");
    }

    private void OnThemeChanged(Theme newTheme , ref Portal po)
    {
        PlayManager.instance.ClearAllIndependentObjects();
        levelSegments = newTheme.levelSegments;
        Debug.Log(po);
        po.gameObject.transform.position = levelSegments[0].Find("PortalStart").position;
        po.gameObject.transform.SetParent(null);
        Player.instance.transform.position =  new Vector2( po.gameObject.transform.position.x+0.5f,po.gameObject.transform.position.y);
       Camera.main.transform.position = new Vector3(Player.instance.transform.position.x ,Player.instance.transform.position.y, Camera.main.transform.position.z) ;
        ClearOldSegments();
        nextSegmentPosition = Vector3.zero; // Reset the next segment position
    }
}

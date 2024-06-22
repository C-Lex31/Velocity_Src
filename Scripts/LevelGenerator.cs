
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    [SerializeField] private Transform levelStart;
    [SerializeField] private Transform[] easyLevelPart;
    [SerializeField] private Transform[] medLevelPart;
    [SerializeField] private Transform[] hardLevelPart;
    [SerializeField] private float distanceToSpawn;
    [SerializeField] private float distanceToDelete;

    private int LevelSpawnCount;
    private Transform[] levelPart;
    private Vector3 nextPartPosition;

    private enum LevelDifficulty{
        Easy,
        Medium,
        Hard
    }
    void Start()
    {
       // nextPartPosition = levelStart.Find("EndPoint").position;
    }

    void Update()
    {
        DeletePlatform();
        GeneratePlatform();
    }
     private void GeneratePlatform()
    {
        while (Vector2.Distance(Player.instance.transform.position,nextPartPosition) < distanceToSpawn)
        {
           
        
            switch(GetLevelDiffiluty())
            {
                case LevelDifficulty.Easy: levelPart = easyLevelPart; break;
                case LevelDifficulty.Medium: levelPart = medLevelPart; break;
                case LevelDifficulty.Hard: levelPart = hardLevelPart; break;
                default: levelPart =easyLevelPart; break;
            }
            

            Transform part = levelPart[Random.Range(0, levelPart.Length)];
            
            Vector2 newPosition = new Vector2(nextPartPosition.x - part.Find("StartPoint").position.x, 0);

            Transform newPart = Instantiate(part, newPosition, transform.rotation, transform);
            
            LevelSpawnCount++;
            nextPartPosition = newPart.Find("EndPoint").position;
        


        }
    }


    private void DeletePlatform()
    {
        if (transform.childCount > 0)
        {
            Transform partToDelete = transform.GetChild(0);

            if (Vector2.Distance(Player.instance.transform.position, partToDelete.transform.position) > distanceToDelete)
                Destroy(partToDelete.gameObject);

        }
    }
    private LevelDifficulty GetLevelDiffiluty()
    {
       // if(LevelSpawnCount>=10) return LevelDifficulty.Hard;
       // if(LevelSpawnCount>=05) return LevelDifficulty.Medium;
        return LevelDifficulty.Easy;
       
    }
}

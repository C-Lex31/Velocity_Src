
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum SceneType
{
    None,
    Home,
    Play,
    Result
}
public class GameManager : Singleton<GameManager>
{

    private SceneType sceneType = SceneType.None;
    [HideInInspector] public int score = 0;
    [HideInInspector] public int coins = 0;
    [HideInInspector] public int totalCoins = 0;
    [HideInInspector] public int savedCoins = 0;
    [HideInInspector] public int performanceBonus = 0;
    [HideInInspector] public int distance = 0;
    [HideInInspector] public float TimeRan = 0;
    [HideInInspector] public bool isSaveGameStart = false;
    [HideInInspector] public CommonUI commonUI;
    [HideInInspector] public CharacterInfo selectedCharacter;
    private bool bLoadScene;
    public bool bIsTouch;
    private Scene scene;
    int lastValue = 0;


    void Awake()
    {
        if (!commonUI)
        {
            GameObject obj = (GameObject)Instantiate(Resources.Load("CommonUI"), Vector3.zero, Quaternion.identity);
            commonUI = obj.GetComponent<CommonUI>();
            commonUI.transform.SetParent(transform, false);
            commonUI.GetComponent<Canvas>().worldCamera = Camera.main;
        }
    }
    void Start()
    {
        string characterName = PlayerPrefs.GetString("SelectedCharacter", "DefaultCharacter");
       selectedCharacter = HomeManager.instance.GetCharacters().Find(c => c.characterName == characterName);
    }

    void Update()
    {

    }
    public void LoadScene(string sceneName, bool isRefresh = false)
    {
        if (bLoadScene) return;
        bLoadScene = true;
        Time.timeScale = 1;
        StartCoroutine(LoadSceneCo(sceneName, isRefresh));
    }
    IEnumerator LoadSceneCo(string sceneName, bool isRefresh = false)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!async.isDone)
        {
            if (async.progress < 0.9f)
            {
                //Loading
                async.allowSceneActivation = false;
            }
            else
            {
                //Loaded
                yield return true;
                async.allowSceneActivation = true;


            }

            yield return null;
        }


        bLoadScene = false;
        Time.timeScale = 1;
        bIsTouch = true;
    }

    /// <summary>
    /// After loading the scene, check which scene is the current scene.
    /// </summary>
    void SceneCheck()
    {
        string scenename = SceneManager.GetActiveScene().name;
        if (scenename == Data.scene_home)
        {
            sceneType = SceneType.Home;
        }
        else if (scenename == Data.scene_play)
        {
            sceneType = SceneType.Play;
        }
        else if (scenename == Data.scene_result)
        {
            sceneType = SceneType.Result;
        }
    }
    /// <summary>
    /// Get the name of the current scene.
    /// </summary>
    public string GetSceneName()
    {
        scene = SceneManager.GetActiveScene();
        return scene.name;
    }

    public void Save()
    {
        if (coins > lastValue)
        {
            SaveFinal();
        }
        SaveGame.SaveBestScore();
    }
    public void CalcPerfomanceBonus()
    {
        performanceBonus = (int)((distance * 0.1f) + (TimeRan * 0.1f));

        // Apply performance modifier to ensure the bonus is always less than coins collected
        performanceBonus = Mathf.FloorToInt(Mathf.Min(performanceBonus, coins * 0.5f));
        totalCoins = coins + Mathf.FloorToInt(performanceBonus);

    }
    public void SaveFinal()
    {
        savedCoins = coins - lastValue + Mathf.FloorToInt(performanceBonus);
        lastValue = coins;//10
        SaveGame.SaveCoins();
        SaveGame.SaveBestScore();
    }
}

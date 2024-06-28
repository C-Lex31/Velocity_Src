
using System.Collections;
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
    [HideInInspector] public int score;
    [HideInInspector] public int coins;
    [HideInInspector] public int distance;
    [HideInInspector] public bool isSaveGameStart = false;
    private bool bLoadScene;
    public bool bIsTouch;
    private Scene scene;

    void Start()
    {

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
}

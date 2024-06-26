using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data : MonoBehaviour
{
    public static float VolumeEffect
    {
        get { return PlayerPrefs.GetFloat("VolumeEffect", 1f); }
        set { PlayerPrefs.SetFloat("VolumeEffect", value); }
    }

    public static float VolumeMusic
    {
        get { return PlayerPrefs.GetFloat("VolumeMusic", 1f); }
        set { PlayerPrefs.SetFloat("VolumeMusic", value); }
    }


    //Sound File Path
    public static string path_sound = "Audio/";

    //SceneLoad
    public static string scene_splash = "0_Splash";
    public static string scene_title = "1_Title";
    public static string scene_home = "2_Home";
    public static string scene_play = "3_Play";
    public static string scene_result = "4_Result";

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

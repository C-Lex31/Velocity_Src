using UnityEngine;
using UnityEditor;

public class VelocityDebug : EditorWindow
{
    private int coinsToAdd = 0;
    private int keysToAdd = 0;
    private int creditsToAdd = 0;

    private float timeScale=1;

    [MenuItem("Tools/Velocity Debug")]
    public static void ShowWindow()
    {
        GetWindow<VelocityDebug>("Velocity Debug");
    }

    private void OnGUI()
    {
        GUILayout.Label("Debug Menu", EditorStyles.boldLabel);

        GUILayout.Space(10);

        coinsToAdd = EditorGUILayout.IntField("Coins to Add", coinsToAdd);
        if (GUILayout.Button("Add Coins"))
        {
            AddCoins(coinsToAdd);
        }

        GUILayout.Space(10);

        keysToAdd = EditorGUILayout.IntField("Keys to Add", keysToAdd);
        if (GUILayout.Button("Add Keys"))
        {
            AddKeys(keysToAdd);
        }

        GUILayout.Space(10);

        creditsToAdd = EditorGUILayout.IntField("Credits to Add", creditsToAdd);
        if (GUILayout.Button("Add Credits"))
        {
           // AddCredits(creditsToAdd);
        }
         GUILayout.Space(10);
        timeScale= EditorGUILayout.FloatField("Time Scale", timeScale);
        if (GUILayout.Button("Scale Time"))
         {
               Time.timeScale=timeScale ;
         }
    }

    private void AddCoins(int amount)
    {
        GlobalGameData.Coin += amount;
        Debug.Log($"{amount} coins added. Total coins: {GlobalGameData.Coin}");
    }

    private void AddKeys(int amount)
    {
        GlobalGameData.Key += amount;
        Debug.Log($"{amount} keys added. Total keys: {GlobalGameData.Key}");
    }

   
}

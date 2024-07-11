using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_Manager : MonoBehaviour
{
    static UI_Manager _instance;
    public static UI_Manager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UI_Manager>();
            }
            return _instance;
        }
    }

    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [SerializeField] private Transform consumablePanel;
    [SerializeField] private GameObject consumableUIPrefab;

    public PopupPause _PopupPause;
    public PopupContinue _PopupContinue;
    private float distance;
    private int coins;
    private int timeRan;

    private Dictionary<string, ConsumableUI> activeConsumableUI = new Dictionary<string, ConsumableUI>();

    void Awake()
    {
        _PopupPause.UIReset();
        _PopupContinue.UIReset();
    }
    void Start()
    {
        InvokeRepeating("UpdateInfo", 0, .2f);
    }

    private void UpdateInfo()
    {
        distance = GameManager.Instance.distance;
        coins = GameManager.Instance.coins;
        GameManager.Instance.score = Mathf.RoundToInt((1f * distance) + (0.5f * coins) + (0.5f * timeRan));
        if (distance > 0)
            distanceText.text = distance.ToString("#,#") + "  m";

        if (coins > 0)
            coinsText.text = coins.ToString("#,#");
    }

    public void AddOrUpdateConsumableUI(string consumableName, Sprite icon, float duration)
    {
        if (activeConsumableUI.ContainsKey(consumableName))
        {
            activeConsumableUI[consumableName].ResetDuration(duration);
        }
        else
        {
            GameObject newConsumableUI = Instantiate(consumableUIPrefab, consumablePanel);
            ConsumableUI consumableUI = newConsumableUI.GetComponent<ConsumableUI>();
            consumableUI.Initialize(icon, duration);
            activeConsumableUI.Add(consumableName, consumableUI);
        }
    }

    public void UpdateConsumableUI(string consumableName, float timeRemaining)
    {
        if (activeConsumableUI.ContainsKey(consumableName))
        {
            activeConsumableUI[consumableName].UpdateSlider(timeRemaining);
        }
    }

    public void RemoveConsumableUI(string consumableName)
    {
        if (activeConsumableUI.ContainsKey(consumableName))
        {
            Destroy(activeConsumableUI[consumableName].gameObject);
            activeConsumableUI.Remove(consumableName);
        }
    }

    public void Click_Pause()
    {
        SoundManager.Instance.PlayEffect(SoundList.sound_common_btn_in);
        _PopupPause.Open();
    }
}

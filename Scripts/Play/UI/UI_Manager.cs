
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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


    public PopupPause _PopupPause;
    public PopupContinue _PopupContinue;
    private float distance;
    private int coins;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateInfo", 0, .2f);
    }
    private void UpdateInfo()
    {
        distance = PlayManager.instance.distance;
        coins = PlayManager.instance.coins;
        if (distance > 0)
            distanceText.text = distance.ToString("#,#") + "  m";

        if (coins > 0)
            coinsText.text = coins.ToString("#,#");
    }



    public void Click_Pause()
    {
        SoundManager.Instance.PlayEffect(SoundList.sound_common_btn_in);
        _PopupPause.Open();
    }
}

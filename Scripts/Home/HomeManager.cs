using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor.U2D.Animation;
using UnityEngine;

public enum CostType
{
    Default,
    Coin,
    Credits,
    Ads
}
public class HomeManager : MonoBehaviour
{

    public PanelBase characterSelectionPanel;
    static HomeManager _instance;

    public static HomeManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<HomeManager>();
            }

            return _instance;
        }
    }

    [SerializeField] private List<CharacterInfo> characters;

    void Awake()
    {
        LoadCharacterStates();
    }
    public void Start()
    {
        SetData();
    }

    void SetData()
    {
        //Currency settings
        GameManager.Instance.commonUI._CurrencyUI.SetCoin();
        GameManager.Instance.commonUI._CurrencyUI.SetCredits();
        characterSelectionPanel.UIReset();
        characterSelectionPanel.SetData();
    }
    [SerializeField] private CanvasGroup cgHome;

    public void SetHomeUI(bool value, CanvasGroup targetGroup, float t = 0.25f)
    {
        float time = t;
        if (!value)
        {
            cgHome.DOKill();
            cgHome.DOFade(0f, time).SetEase(Ease.OutSine);
            cgHome.transform.DOScale(1.15f, time).SetEase(Ease.OutSine).OnComplete(() =>
           {
               cgHome.gameObject.SetActive(false);

               if (targetGroup != null)
               {
                   targetGroup.gameObject.SetActive(true);
                   targetGroup.DOKill();
                   targetGroup.DOFade(1f, time).SetEase(Ease.OutCubic);
                   targetGroup.transform.DOScale(1f, time).SetEase(Ease.OutCubic);

               }
           });
        }
        else
        {

            if (targetGroup != null)
            {
                targetGroup.DOKill();
                targetGroup.DOFade(0f, time).SetEase(Ease.OutCubic);
                targetGroup.transform.DOScale(0.95f, time).SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    targetGroup.gameObject.SetActive(false);
                    //Show home panel
                    cgHome.DOKill();
                    cgHome.gameObject.SetActive(true);
                    cgHome.DOFade(1f, time).SetEase(Ease.OutCubic);
                    cgHome.transform.DOScale(1f, time).SetEase(Ease.OutBack);
                });
            }
            else
            {
                //Show home panel
                cgHome.DOKill();
                cgHome.gameObject.SetActive(true);
                cgHome.DOFade(1f, time).SetEase(Ease.OutCubic);
                cgHome.transform.DOScale(1f, time).SetEase(Ease.OutBack);
            }
        }


    }

    public List<CharacterInfo> GetCharacters()
    {
        return characters;
    }

    private void LoadCharacterStates()
    {
        foreach (CharacterInfo character in characters)
        {
            character.isUnlocked = PlayerPrefs.GetInt(character.characterName, 0) == 1;
        }
    }
    public void ClickPlay()
    {
        GameManager.Instance.LoadScene(Data.scene_play);
    }
    public void ClickCharacterSelect()
    {
        characterSelectionPanel.Open();
    }

}

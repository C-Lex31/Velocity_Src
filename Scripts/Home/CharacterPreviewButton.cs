using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterPreviewButton : MonoBehaviour
{
    public Button Button;
    public Button UpgradeButton;
    [SerializeField] private Image characterSprite;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private Image costIcon;
    [SerializeField] private GameObject costContainer;
    [SerializeField] private GameObject selectedFrame;
    [SerializeField] private GameObject touchedFrame;

    public void Setup(CharacterInfo character)
    {
        characterSprite.sprite = character.sprite;
        characterName.text = character.characterName;
        if (character.isUnlocked)
        {
            costContainer.SetActive(false);
            UpgradeButton.gameObject.SetActive(true);
        }
        else
        {
            costContainer.SetActive(true);
            UpgradeButton.gameObject.SetActive(false);
        }
    }

    public void SetCostIcon(string iconPath)
    {
        costIcon.sprite = Resources.Load<Sprite>(iconPath);
    }

    public void HideCostAndShowUpgrade()
    {
        costContainer.SetActive(false);
        UpgradeButton.gameObject.SetActive(true);
    }

    public void SetSelectedFrame(bool active)
    {
        selectedFrame.SetActive(active);
    }

    public void SetTouchedFrame(bool active)
    {
        touchedFrame.SetActive(active);
    }

    public void Unlock()
    {
        costContainer.SetActive(false);
        UpgradeButton.gameObject.SetActive(true);
    }
}

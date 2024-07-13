using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CharacterSelectionPanel : PanelBase
{
    [SerializeField] private Transform characterListContainer;
    [SerializeField] private GameObject characterButtonPrefab;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    private CharacterInfo selectedCharacter;

    public override void SetData()
    {
        PopulateCharacterList();
    }
    private void PopulateCharacterList()
    {
        foreach (CharacterInfo character in HomeManager.instance.GetCharacters())
        {
            GameObject button = Instantiate(characterButtonPrefab, characterListContainer);
            //  button.GetComponent<Image>().sprite = character.icon;
            button.GetComponent<Button>().onClick.AddListener(() => OnCharacterButtonClicked(character));
        }
    }
    public void OnCharacterButtonClicked(CharacterInfo character)
    {
        selectedCharacter = character;
        UpdateActionButton();
    }
    private void UpdateActionButton()
    {
        if (selectedCharacter.isUnlocked)
        {
            actionButtonText.text = "Select";
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(SelectCharacter);
        }
        else
        {
            actionButtonText.text = "Unlock";
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(UnlockCharacter);
        }
    }
    private void OnConfirmButtonClicked()
    {
        if (selectedCharacter != null)
        {
            // Save the selected character (example using PlayerPrefs, replace with your saving system if different)
            PlayerPrefs.SetString("SelectedCharacter", selectedCharacter.characterName);
            PlayerPrefs.Save();

            // Load the game scene or proceed with the selected character
            Debug.Log($"Selected Character: {selectedCharacter.characterName}");
        }
    }


    private void UnlockCharacter()
    {
        // Add your unlocking logic here

        switch (selectedCharacter.costType)
        {
            case CostType.Coin:
                if (GlobalGameData.Coin >= selectedCharacter.costValue)
                {
                    GlobalGameData.Coin -= selectedCharacter.costValue;
                    GameManager.Instance.commonUI._CurrencyUI.SetCoin();
                    selectedCharacter.isUnlocked = true;
                    PlayerPrefs.SetInt(selectedCharacter.characterName, 1); // Save unlocked state
                    UpdateActionButton();
                }
                break;
        }

    }

    private void SelectCharacter()
    {
        // Add your selection logic here
        PlayerPrefs.SetString("SelectedCharacter", selectedCharacter.characterName);
        PlayerPrefs.Save();
        Debug.Log($"Selected Character: {selectedCharacter.characterName}");
    }
}

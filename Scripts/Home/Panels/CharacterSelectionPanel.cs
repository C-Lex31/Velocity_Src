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
    private string previouslySelectedCharacterName;

    public override void SetData()
    {
        PopulateCharacterList();
    }

    private void PopulateCharacterList()
    {
        // Load previously selected character
        previouslySelectedCharacterName = PlayerPrefs.GetString("SelectedCharacter", string.Empty);

        foreach (CharacterInfo character in HomeManager.instance.GetCharacters())
        {
            GameObject button = Instantiate(characterButtonPrefab, characterListContainer);
            button.transform.Find("CharacterSprite").GetComponent<Image>().sprite=character.sprite;
            button.GetComponent<Button>().onClick.AddListener(() => OnCharacterButtonClicked(character));

            // If this character is the previously selected one, set the button text to "Selected"
            if (character.characterName == previouslySelectedCharacterName)
            {
                actionButtonText.text = "Selected";
                selectedCharacter = character;
            }
        }
    }

    public void OnCharacterButtonClicked(CharacterInfo character)
    {
        selectedCharacter = character;
        UpdateActionButton();
    }

    private void UpdateActionButton()
    {
        if (selectedCharacter.characterName == previouslySelectedCharacterName)
        {
            actionButtonText.text = "Selected";
        }
        else if (selectedCharacter.isUnlocked)
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
            // Save the selected character 
            PlayerPrefs.SetString("SelectedCharacter", selectedCharacter.characterName);
            PlayerPrefs.Save();

            // Load the game scene or proceed with the selected character
            Debug.Log($"Selected Character: {selectedCharacter.characterName}");
        }
    }

    private void UnlockCharacter()
    {
        // unlocking logic here
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
        // selection logic here
        PlayerPrefs.SetString("SelectedCharacter", selectedCharacter.characterName);
        PlayerPrefs.Save();
        Debug.Log($"Selected Character: {selectedCharacter.characterName}");

        actionButtonText.text = "Selected";
        previouslySelectedCharacterName = selectedCharacter.characterName;
    }
}

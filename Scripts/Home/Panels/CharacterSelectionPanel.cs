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
    private GameObject currentlyTouchedButton;
    private Dictionary<CharacterInfo, GameObject> characterButtons = new Dictionary<CharacterInfo, GameObject>();

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
            characterButtons[character] = button;

            CharacterPreviewButton buttonUI = button.GetComponent<CharacterPreviewButton>();
            buttonUI.Setup(character);

            buttonUI.Button.onClick.AddListener(() => OnCharacterButtonClicked(character));
            buttonUI.UpgradeButton.onClick.AddListener(() => OnCharacterUpgradeButtonClicked(character));

            if (!character.isUnlocked)
                buttonUI.SetCostIcon($"Sprites/CostIcon_{(int)character.costType}");
            else
                buttonUI.HideCostAndShowUpgrade();

            // If this character is the previously selected one, set the button text to "Selected"
            if (character.characterName == previouslySelectedCharacterName)
            {
                actionButtonText.text = "Selected";
                selectedCharacter = character;
                buttonUI.SetSelectedFrame(true);
            }
        }
    }

    public void OnCharacterButtonClicked(CharacterInfo character)
    {
        SetCharacterPreview(character);
    }

    public void OnCharacterUpgradeButtonClicked(CharacterInfo character)
    {
        SetCharacterPreview(character);
        Debug.Log("OpenUpgradeWindow");
    }

    private void SetCharacterPreview(CharacterInfo character)
    {
        if (currentlyTouchedButton != null)
        {
            currentlyTouchedButton.GetComponent<CharacterPreviewButton>().SetTouchedFrame(false);
        }

        selectedCharacter = character;
        currentlyTouchedButton = characterButtons[character];
        currentlyTouchedButton.GetComponent<CharacterPreviewButton>().SetTouchedFrame(true);

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

    private void UnlockCharacter()
    {
        // Unlocking logic here
        switch (selectedCharacter.costType)
        {
            case CostType.Coin:
                if (GlobalGameData.Coin >= selectedCharacter.costValue)
                {
                    GlobalGameData.Coin -= selectedCharacter.costValue;
                    GameManager.Instance.commonUI._CurrencyUI.SetCoin();
                    selectedCharacter.isUnlocked = true;
                    PlayerPrefs.SetInt(selectedCharacter.characterName, 1); // Save unlocked state
                    characterButtons[selectedCharacter].GetComponent<CharacterPreviewButton>().Unlock();
                    UpdateActionButton();
                }
                else
                {
                    GameManager.Instance.commonUI.SetToast("Not enough Coins");
                }
                break;
        }
    }

    private void SelectCharacter()
    {
        // Selection logic here
        PlayerPrefs.SetString("SelectedCharacter", selectedCharacter.characterName);
        PlayerPrefs.Save();
        Debug.Log($"Selected Character: {selectedCharacter.characterName}");
        actionButtonText.text = "Selected";
        previouslySelectedCharacterName = selectedCharacter.characterName;

        // Update frames
        foreach (var pair in characterButtons)
        {
            CharacterPreviewButton buttonUI = pair.Value.GetComponent<CharacterPreviewButton>();
            buttonUI.SetSelectedFrame(pair.Key == selectedCharacter);
            buttonUI.SetTouchedFrame(false);
        }
    }
}

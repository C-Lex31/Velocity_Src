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

            button.transform.Find("CharacterSprite").GetComponent<Image>().sprite = character.sprite;
            button.transform.Find("CharacterName").GetComponent<TextMeshProUGUI>().text = character.characterName;
            button.GetComponent<Button>().onClick.AddListener(() => OnCharacterButtonClicked(character));
            button.transform.Find("UpgradeButton").GetComponent<Button>().onClick.AddListener(() => OnCharacterUpgradeButtonClicked(character));
            if (!character.isUnlocked)
                button.transform.Find("Cost").GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>($"Sprites/CostIcon_{(int)character.costType}");
            else
            {
                button.transform.Find("Cost").gameObject.SetActive(false);
                button.transform.Find("UpgradeButton").gameObject.SetActive(true);

            }

            // If this character is the previously selected one, set the button text to "Selected"
            if (character.characterName == previouslySelectedCharacterName)
            {
                actionButtonText.text = "Selected";
                selectedCharacter = character;
                button.transform.Find("SelectedFrame").gameObject.SetActive(true);
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
    void SetCharacterPreview(CharacterInfo character)
    {
        if (currentlyTouchedButton != null)
        {
            currentlyTouchedButton.transform.Find("TouchedFrame").gameObject.SetActive(false);
        }

        selectedCharacter = character;
        currentlyTouchedButton = characterButtons[character];
        currentlyTouchedButton.transform.Find("TouchedFrame").gameObject.SetActive(true);

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
                    characterButtons[selectedCharacter].gameObject.SetActive(false);
                    characterButtons[selectedCharacter].transform.Find("UpgradeButton").gameObject.SetActive(true);
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
        // selection logic here
        PlayerPrefs.SetString("SelectedCharacter", selectedCharacter.characterName);
        PlayerPrefs.Save();
        Debug.Log($"Selected Character: {selectedCharacter.characterName}");
        actionButtonText.text = "Selected";
        previouslySelectedCharacterName = selectedCharacter.characterName;

        // Update frames
        foreach (var pair in characterButtons)
        {
            pair.Value.transform.Find("SelectedFrame").gameObject.SetActive(pair.Key == selectedCharacter);
            pair.Value.transform.Find("TouchedFrame").gameObject.SetActive(false);
        }
    }
}

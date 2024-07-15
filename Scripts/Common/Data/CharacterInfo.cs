
using UnityEngine;




[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character")]
public class CharacterInfo : ScriptableObject
{
    public string characterName;
    public GameObject characterPrefab;
    public Sprite sprite;
    public bool isUnlocked;

    public int costValue;
    public CostType costType;
}

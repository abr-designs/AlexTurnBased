using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class GameManager : MonoBehaviour
{
    public enum GAMESTATE
    {
        CHARACTER_SELECT,
        MOVE_SELECT,
        TARGET_SELECT,
        ENEMY_TURN
        
    }

    //---------------------------------------------------------------------------------------//
    
    private GAMESTATE currentGamestate;

    [SerializeField, BoxGroup("Info"), ReadOnly]
    private int selectedIndex;
    [SerializeField, BoxGroup("Info"), ReadOnly]
    private int selectedCharacterIndex;
    [SerializeField, BoxGroup("Info"), ReadOnly]
    private int selectedAbilityIndex;
    
    //---------------------------------------------------------------------------------------//
    
    [SerializeField, FoldoutGroup("Target UI")]
    private Transform arrowTransform;

    [SerializeField, FoldoutGroup("Target UI")]
    private Vector3 arrowOffset;
    
    //---------------------------------------------------------------------------------------//

    [SerializeField, FoldoutGroup("Player UI")]
    private List<Button> actionButtons;

    private ButtonText[] actionButtonTexts;

    [SerializeField, FoldoutGroup("Player UI")]
    private GameObject partyMemberUIPrefab;

    [SerializeField, FoldoutGroup("Player UI")]
    private RectTransform partyUiContainerTransform;

    private List<PartMemberUIElement> memberElements;
   //[SerializeField, FoldoutGroup("Player UI")]
   //private GameObject buttonPrefab;

   //[SerializeField, FoldoutGroup("Player UI")]
   //private TextMeshProUGUI descriptionText;

   [SerializeField, FoldoutGroup("Player UI")]
   private EventSystem eventSystem;

   //private List<GameObject> buttons;

    //---------------------------------------------------------------------------------------//
    
    [SerializeField,FoldoutGroup("Game Character")]
    private List<CharacterBase> playerCharacters;
    
    [SerializeField,FoldoutGroup("Game Character")]
    private List<CharacterBase> enemyCharacters;
    
    [SerializeField, ReadOnly,BoxGroup("Info")]
    private List<CharacterBase> possibleTargets;
    
    //---------------------------------------------------------------------------------------//
    
    // Start is called before the first frame update
    void Start()
    {
        InitButtonUI();
        GeneratePartyUI();

        SetGameState(GAMESTATE.CHARACTER_SELECT);

    }

    // Update is called once per frame
    void Update()
    {
        //Input Checks
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            MoveHorizontal(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            MoveHorizontal(1);

        if (Input.GetKeyDown(KeyCode.UpArrow))
            MoveVertical(-1);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            MoveVertical(1);
        
        if(Input.GetKeyDown(KeyCode.Return))
            Enter();
        
        if(Input.GetKeyDown(KeyCode.Escape))
            Escape();
    }
    //---------------------------------------------------------------------------------------//

    private void SetGameState(GAMESTATE newState)
    {
        currentGamestate = newState;
        selectedIndex = 0;

        switch (currentGamestate)
        {
            case GAMESTATE.CHARACTER_SELECT:
                SetActionButtonsActive(false);
                HighlightCharacter(playerCharacters[selectedIndex]);
                eventSystem.SetSelectedGameObject(null);
                break;
            case GAMESTATE.MOVE_SELECT:
                SetActionButtonsActive(true);
                SelectCharacter(playerCharacters[selectedCharacterIndex]);
                eventSystem.SetSelectedGameObject(actionButtons[selectedIndex].gameObject);
                break;
            case GAMESTATE.TARGET_SELECT:
                GenerateTargetList(playerCharacters[selectedCharacterIndex].Abilities[selectedAbilityIndex],
                    out possibleTargets);
                HighlightTarget(possibleTargets[selectedIndex]);
                break;
            case GAMESTATE.ENEMY_TURN:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    //---------------------------------------------------------------------------------------//

    private void InitButtonUI()
    {
        actionButtonTexts = new[]
        {
            new ButtonText(actionButtons[0]),
            new ButtonText(actionButtons[1]),
            new ButtonText(actionButtons[2]),
        };
    }

    private void GeneratePartyUI()
    {
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            var temp = Instantiate(partyMemberUIPrefab).GetComponent<PartMemberUIElement>();
            var rect = temp.transform as RectTransform;

            rect.SetParent(partyUiContainerTransform);
            rect.localScale = Vector3.one;
            
            temp.Init(playerCharacters[i]);

            if(memberElements == null)
                memberElements = new List<PartMemberUIElement>();
            
            memberElements.Add(temp);
        }
    }
    
    
    //---------------------------------------------------------------------------------------//

    private void MoveHorizontal(int direction)
    {
        switch (currentGamestate)
        {
            case GAMESTATE.CHARACTER_SELECT:
                break;
            case GAMESTATE.TARGET_SELECT:
                break;
            default:
                return;
        }
    }

    private void MoveVertical(int direction)
    {
        switch (currentGamestate)
        {
            case GAMESTATE.CHARACTER_SELECT:
                selectedIndex = ClampListBounds(playerCharacters, selectedIndex, direction);             
                HighlightCharacter(playerCharacters[selectedIndex]);
                break;
            case GAMESTATE.MOVE_SELECT:
                //FIXME Need to actually only use an open type to navigate
                //ClampListBounds(actionButtons, selectedIndex, direction); 
                selectedIndex = ClampListBounds(actionButtons, selectedIndex, direction);
                eventSystem.SetSelectedGameObject(actionButtons[selectedIndex].gameObject);
                break;
            case GAMESTATE.TARGET_SELECT:
                selectedIndex = ClampListBounds(possibleTargets, selectedIndex, direction);
                //eventSystem.SetSelectedGameObject(actionButtons[selectedIndex].gameObject);
                HighlightTarget(possibleTargets[selectedIndex]);
                break;
            case GAMESTATE.ENEMY_TURN:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    private void Enter()
    {
        switch (currentGamestate)
        {
            case GAMESTATE.CHARACTER_SELECT:
                selectedCharacterIndex = selectedIndex;
                SetGameState(GAMESTATE.MOVE_SELECT);
                break;
            case GAMESTATE.MOVE_SELECT:
                selectedAbilityIndex = selectedIndex;
                SetGameState(GAMESTATE.TARGET_SELECT);
                break;
            case GAMESTATE.TARGET_SELECT:
                SelectTarget(possibleTargets[selectedIndex]);
                break;
            case GAMESTATE.ENEMY_TURN:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void Escape()
    {
        switch (currentGamestate)
        {
            case GAMESTATE.CHARACTER_SELECT:
                break;
            case GAMESTATE.MOVE_SELECT:
                selectedIndex = selectedCharacterIndex;
                selectedCharacterIndex = 0;
                SetGameState(GAMESTATE.CHARACTER_SELECT);
                break;
            case GAMESTATE.TARGET_SELECT:
                selectedIndex          = selectedAbilityIndex;
                selectedAbilityIndex = 0;
                SetGameState(GAMESTATE.MOVE_SELECT);
                HighlightCharacter(playerCharacters[selectedCharacterIndex]);
                break;
            case GAMESTATE.ENEMY_TURN:
                break;
        }
    }
    
    //---------------------------------------------------------------------------------------//

    //TODO I think that i can combine both the Target & Character Highlights
    private void HighlightCharacter(CharacterBase character)
    {
        arrowTransform.position = character.Transform.position + arrowOffset;
        //TODO Need to highlight the UI portion of the character
        
        for(int i = 0; i < memberElements.Count; i++)
            memberElements[i].Highlight(i == selectedIndex);
            
    }

    private void SelectCharacter(CharacterBase character)
    {
        Debug.LogError("Selected Character:" + character.characterName);
        //arrowTransform.position = character.Transform.position + arrowOffset;

        for (int i = 0; i < 3; i++)
        {
            if (character.Abilities[i] == null)
            {
                actionButtonTexts[i].interactable = false;
                continue;
            }

            actionButtonTexts[i].Text = character.Abilities[i].Name;
        }
        
        //TODO Need to spawn all button options for the character Abilities
        //TODO Need to display character name that has been selected
    }

    private void HighlightTarget(CharacterBase character)
    {
        arrowTransform.position = character.Transform.position + arrowOffset;
    }

    private void SelectTarget(CharacterBase target)
    {
        //TODO I should confirm the selection
    }
    
    //---------------------------------------------------------------------------------------//
    private void GenerateTargetList(AbilityScriptableObject ability, out List<CharacterBase> targets)
    {
        targets = new List<CharacterBase>();

        switch (ability.TargetType)
        {
            case TargetType.Enemy:
                targets.AddRange(enemyCharacters);
                break;
            case TargetType.Friendly:
                targets.AddRange(playerCharacters);
                break;
            case TargetType.Self:
                targets.Add(playerCharacters[selectedCharacterIndex]);
                break;
        }
        
        if(ability.CanTargetSelf && ability.TargetType != TargetType.Self)
            targets.Add(playerCharacters[selectedCharacterIndex]);

        targets = targets.Distinct().ToList();

    }
    
    //---------------------------------------------------------------------------------------//

    private void SetActionButtonsActive(bool state)
    {
        for(int i = 0; i < actionButtons.Count; i++)
            actionButtons[i].gameObject.SetActive(state);
    }
    
    //---------------------------------------------------------------------------------------//

    private static int ClampListBounds(IList list,int index,  int direction)
    {
        if (index + direction < 0)
            return list.Count - 1;
        
        
        if (index + direction >= list.Count)
            return 0;
        
        
        return index + direction;
    }

    
}

struct ButtonText
{
    private Button          button;
    private TextMeshProUGUI text;

    public string Text
    {
        set => text.text = value;
    }

    public ButtonText(Button button)
    {
        this.button = button;
        text        = button.GetComponentInChildren<TextMeshProUGUI>();
    }

    public bool interactable
    {
        set
        {
            button.interactable = value;

            if (!value)
                text.text = "-";
        }
    }

    public void SetActive(bool value)
    {
        button.gameObject.SetActive(value);
    }
}

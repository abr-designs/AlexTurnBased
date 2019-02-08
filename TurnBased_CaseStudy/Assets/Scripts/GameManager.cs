using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.EventSystems;
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
    
    //---------------------------------------------------------------------------------------//
    
    [SerializeField, FoldoutGroup("Target UI")]
    private Transform arrowTransform;

    [SerializeField, FoldoutGroup("Target UI")]
    private Vector3 arrowOffset;
    
    //---------------------------------------------------------------------------------------//

    [SerializeField, FoldoutGroup("Player UI")]
    private GameObject buttonPrefab;

    [SerializeField, FoldoutGroup("Player UI")]
    private TextMeshProUGUI descriptionText;

    [SerializeField, FoldoutGroup("Player UI")]
    private EventSystem eventSystem;

    private List<GameObject> buttons;

    //---------------------------------------------------------------------------------------//
    
    [SerializeField,FoldoutGroup("Game Character")]
    private List<CharacterBase> playerCharacters;
    
    [SerializeField,FoldoutGroup("Game Character")]
    private List<CharacterBase> enemyCharacters;

    [SerializeField,FoldoutGroup("Game Character"), ReadOnly]
    private int selectedIndex;
    
    //---------------------------------------------------------------------------------------//
    
    // Start is called before the first frame update
    void Start()
    {
        SelectCharacter(playerCharacters[selectedIndex]);
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
            MoveVertical(1);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            MoveVertical(-1);
    }
    
    //---------------------------------------------------------------------------------------//

    private void MoveHorizontal(int direction)
    {
        switch (currentGamestate)
        {
            case GAMESTATE.CHARACTER_SELECT:
                selectedIndex = ClampListBounds(playerCharacters, selectedIndex, direction);             
                SelectCharacter(playerCharacters[selectedIndex]);
                break;
            case GAMESTATE.TARGET_SELECT:
                break;
            default:
                return;
        }
    }

    private void MoveVertical(int direction)
    {
        
    }
    
    //---------------------------------------------------------------------------------------//

    private void SelectCharacter(CharacterBase character)
    {
        arrowTransform.position = character.Transform.position + arrowOffset;
        
        //TODO Need to spawn all button options for the character Abilities
        //TODO Need to display character name that has been selected
    }

    private void SelectTarget(CharacterBase character)
    {
        
    }
    
    //---------------------------------------------------------------------------------------//

    private static int ClampListBounds(List<CharacterBase> list,int index,  int direction)
    {
        if (index + direction < 0)
            return list.Count - 1;
        
        
        if (index + direction >= list.Count)
            return 0;
        
        
        return index + direction;
    }
}

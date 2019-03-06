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
using Random = UnityEngine.Random;

[RequireComponent(typeof(DamageController))]
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
    [SerializeField, BoxGroup("Info"), ReadOnly]
    private GAMESTATE currentGamestate;

    [SerializeField, BoxGroup("Info"), ReadOnly]
    private int currentTurn = 0;

    [SerializeField, BoxGroup("Info"), ReadOnly]
    private int selectedIndex;

    [SerializeField, BoxGroup("Info"), ReadOnly]
    private int selectedCharacterIndex;

    [SerializeField, BoxGroup("Info"), ReadOnly]
    private int selectedAbilityIndex;

    [SerializeField, BoxGroup("Info"), ReadOnly]
    private int selectedTargetIndex;

    [SerializeField, ReadOnly, BoxGroup("Info")]
    private List<CharacterBase> possibleTargets;

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
    [SerializeField, FoldoutGroup("Game Characters"), Required]
    public GameObject shieldSpritePrefab;

    [SerializeField, FoldoutGroup("Game Characters"), Required]
    public GameObject stunEffectPrefab;

    [SerializeField, FoldoutGroup("Game Characters")]
    private List<CharacterBase> playerCharacters;

    [SerializeField, FoldoutGroup("Game Characters")]
    private List<CharacterBase> enemyCharacters;

    //---------------------------------------------------------------------------------------//

    [SerializeField, FoldoutGroup("Game UI"), Required]
    private TextMeshProUGUI turnText;


    //---------------------------------------------------------------------------------------//

    [SerializeField, Required] public GameObject risingTextPrefab;

    //---------------------------------------------------------------------------------------//

    private DamageController _damageController;
    
    // Start is called before the first frame update
    private void Start()
    {
        _damageController = GetComponent<DamageController>();
        InitButtonUI();
        GeneratePartyUI();

        StartCoroutine(GameLoop());
        //StartPlayerTurn();

    }

    // Update is called once per frame
    private void Update()
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

        if (Input.GetKeyDown(KeyCode.Return))
            Enter();

        if (Input.GetKeyDown(KeyCode.Escape))
            Escape();
    }
    //---------------------------------------------------------------------------------------//


    private IEnumerator GameLoop()
    {
        while (IsAlive(playerCharacters) && IsAlive(enemyCharacters))
        {
            for (int i = 0; i < playerCharacters.Count; i++)
                playerCharacters[i].StartTurn();

            SetGameState(GAMESTATE.CHARACTER_SELECT);
            yield return StartCoroutine(ShowTurnTextCoroutine("Player Turn", Color.white));

            yield return new WaitUntil(() => !IsCharacterAvailable());

            Debug.LogError("Enemy Turn");
            SetGameState(GAMESTATE.ENEMY_TURN);
            yield return StartCoroutine(ShowTurnTextCoroutine("Enemy Turn", Color.red));

            yield return StartCoroutine(EnemyTurnCoroutine());

            currentTurn++;
        }
    }
    
    private void SetGameState(GAMESTATE newState)
    {
        currentGamestate = newState;
        selectedIndex    = 0;

        switch (currentGamestate)
        {
            case GAMESTATE.CHARACTER_SELECT:
                if (!GetNextAvailableCharacter(out selectedIndex))
                {
                    Debug.LogError("No more available moves");
                    return;
                }

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
                SetActionButtonsActive(false);
                Debug.LogError("Enemy Using Turn...");
                //StartPlayerTurn();
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator ShowTurnTextCoroutine(string message, Color color)
    {
        turnText.color = Color.clear;
        turnText.text  = message;

        float _t = 0f;
        while (_t < 1f)
        {
            turnText.color = Color.Lerp(Color.clear, color, _t += Time.deltaTime * 3f);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        _t = 0f;

        while (_t < 1f)
        {
            turnText.color = Color.Lerp(color, Color.clear, _t += Time.deltaTime);
            yield return null;
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

            if (memberElements == null)
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

                //We check here if this character has used their move for this turn
                if (playerCharacters[selectedIndex].turnDone)
                {
                    MoveVertical(direction);
                    return;
                }

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
                selectedTargetIndex = selectedIndex;
                SelectTarget(playerCharacters[selectedCharacterIndex].Abilities[selectedAbilityIndex],
                    possibleTargets[selectedTargetIndex]);
                FinishCharacterTurn(playerCharacters[selectedCharacterIndex]);
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
                selectedIndex          = selectedCharacterIndex;
                selectedCharacterIndex = 0;
                SetGameState(GAMESTATE.CHARACTER_SELECT);
                break;
            case GAMESTATE.TARGET_SELECT:
                selectedIndex        = selectedAbilityIndex;
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

        for (int i = 0; i < memberElements.Count; i++)
        {
            //Skip trying to set the colors of any characters that have finished their turn
            if (playerCharacters[i].turnDone)
                continue;

            memberElements[i].Highlight(i == selectedIndex);
        }

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
    }

    private void HighlightTarget(CharacterBase character)
    {
        arrowTransform.position = character.Transform.position + arrowOffset;
    }

    private void SelectTarget(AbilityScriptableObject ability, CharacterBase target)
    {
        int value = ability.GetValueRoll();

        //TODO I should confirm the selection
        switch (ability.AbilityType)
        {
            case AbilityType.LightAttack:
            case AbilityType.HeavyAttack:
                value = Mathf.CeilToInt(value *
                        _damageController.GetMultiplier(playerCharacters[selectedCharacterIndex].attackType,
                        target.attackType));
                
                //Damage character, based on chance and on range
                target.DoDamage(value);
                if (value > 0)
                    ability.ApplyEffectOnTarget(target);

                if (!IsAlive(enemyCharacters))
                {
                    //TODO Need a way of wrapping up match
                    StartCoroutine(ShowTurnTextCoroutine("Victory", Color.green));
                }
                
                break;
            case AbilityType.Stun:
                //Stuns Target
                target.Stun(1);
                ability.ApplyEffectOnTarget(target);

                break;
            case AbilityType.Heal:
                //Add health, based on range
                target.Heal(value);
                memberElements.Find(x => x.m_character == target).UpdateUI();
                ability.ApplyEffectOnTarget(target);

                break;
            case AbilityType.Block:
                //Sets target to be blocking
                target.Block();
                break;
            
            case AbilityType.Poison:
                target.Poison(ability);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }

        playerCharacters[selectedCharacterIndex].EndTurn();
        SetGameState(GAMESTATE.CHARACTER_SELECT);
    }

    //---------------------------------------------------------------------------------------//

    private IEnumerator EnemyTurnCoroutine()
    {
        for (int i = 0; i < enemyCharacters.Count; i++)
        {
            Debug.Log($"Start {i}");
            
            var e = enemyCharacters[i] as EnemyCharacter;

            if (e == null)
                continue;

            if (e.IsDead)
                continue;

            HighlightTarget(e);

            yield return new WaitForSeconds(1f);

            e.StartTurn();

            if (e.IsStunned)
            {
                e.ShowStunnedState();

                yield return new WaitForSeconds(1.5f);

                continue;
            }

            //If we're on the first turn, first enemy, pick a random target
            int targetIndex = currentTurn == 0 && i == 0 ? RandomCharacter() : LowestHealthPlayerCharacterIndex();

            //If we have less than 50% health & we can't potentially kill any character, prioritize heal
            //TODO Need to consider healing their teammates as well
            if (e.heal != null && e.CurrentHealthNormalized <= 0.5f)
            {
                yield return StartCoroutine(EnemyPassive(e, e, e.block, () => e.Heal(e.heal.GetValueRoll())));
            }
            //If we can attack someone and kill them with a light attack do that
            else if (e.lightAttack != null &&
                     playerCharacters[targetIndex].CurrentHealth - e.lightAttack.valueRange.y <= 0)
            {
                yield return StartCoroutine(EnemyAttack(targetIndex, e,playerCharacters[targetIndex], e.lightAttack));
            }
            //If we can attack and kill someone with a heavy attack, do that
            else if (e.heavyAttack != null &&
                     playerCharacters[targetIndex].CurrentHealth - e.heavyAttack.valueRange.y <= 0)
            {
                yield return StartCoroutine(EnemyAttack(targetIndex, e,playerCharacters[targetIndex], e.heavyAttack));
            }
            //If we can't kill anyone, default to heavy attack on lowest health target ,Then light attack or then block
            else
            {
                if (e.heavyAttack)
                {
                    yield return StartCoroutine(EnemyAttack(targetIndex, e,playerCharacters[targetIndex], e.heavyAttack));
                }
                else if (e.lightAttack)
                {
                    yield return StartCoroutine(EnemyAttack(targetIndex, e,playerCharacters[targetIndex], e.lightAttack));
                }
                else if (e.block)
                {
                    yield return StartCoroutine(EnemyPassive(e, e, e.block, () => e.Block()));
                }
            }

            e.EndTurn();
        }
    }

    private IEnumerator EnemyAttack(int index, CharacterBase caster, CharacterBase target, AbilityScriptableObject ability)
    {
        HighlightTarget(target);

        yield return new WaitForSeconds(1f);

        var value = ability.GetValueRoll();
        
        value = Mathf.CeilToInt(value *
                                _damageController.GetMultiplier(caster.attackType,
                                    target.attackType));

        target.DoDamage(value);
        memberElements[index].UpdateUI();

        if (value > 0)
            ability.ApplyEffectOnTarget(target);

        yield return new WaitForSeconds(2f);

        if (!IsAlive(playerCharacters))
            StartCoroutine(ShowTurnTextCoroutine("Defeat", Color.red));
    }

    private IEnumerator EnemyPassive(CharacterBase caster, CharacterBase target, AbilityScriptableObject ability,
                                     Action        onPreCall)
    {
        onPreCall?.Invoke();

        ability.ApplyEffectOnTarget(target);
        yield return new WaitForSeconds(2f);
    }

    private int LowestHealthPlayerCharacterIndex()
    {
        int lowest = 999;
        int index  = -1;

        for (int i = 0; i < playerCharacters.Count; i++)
        {
            if (playerCharacters[i].IsDead)
                continue;

            if (playerCharacters[i].CurrentHealth < lowest)
            {
                lowest = playerCharacters[i].CurrentHealth;
                index  = i;
            }
        }

        return index;
    }

    private int RandomCharacter()
    {
        return Random.Range(0, playerCharacters.Count);
    }

    //---------------------------------------------------------------------------------------//

    private void FinishCharacterTurn(CharacterBase character)
    {
        //TODO Need to check if all characters have finished their turn, and display warning otherwise
        memberElements[selectedCharacterIndex].SetActive(false);

        character.EndTurn();

    }

    private bool GetNextAvailableCharacter(out int availableIndex)
    {
        availableIndex = 0;
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            if (!playerCharacters[i].turnDone)
            {
                availableIndex = i;
                return true;
            }
        }

        return false;
    }

    private bool IsCharacterAvailable()
    {
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            if (!playerCharacters[i].turnDone)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAlive(List<CharacterBase> list)
    {
        foreach (var character in list)
        {
            if (character.CurrentHealth > 0)
                return true;
        }

        return false;
    }

    //---------------------------------------------------------------------------------------//
    private void GenerateTargetList(AbilityScriptableObject ability, out List<CharacterBase> targets)
    {
        targets = new List<CharacterBase>();

        switch (ability.TargetType)
        {
            case TargetType.Enemy:
                //targets.AddRange(enemyCharacters);
                foreach (var enemyCharacter in enemyCharacters)
                {
                    if (!enemyCharacter.IsDead)
                        targets.Add(enemyCharacter);
                }

                break;
            case TargetType.Friendly:
                targets.AddRange(playerCharacters);
                foreach (var character in playerCharacters)
                {
                    if (!character.IsDead)
                        targets.Add(character);
                }

                break;
            case TargetType.Self:
                targets.Add(playerCharacters[selectedCharacterIndex]);
                break;
        }

        if (ability.CanTargetSelf && ability.TargetType != TargetType.Self)
            targets.Add(playerCharacters[selectedCharacterIndex]);

        targets = targets.Distinct().ToList();

    }

    //---------------------------------------------------------------------------------------//

    private void SetActionButtonsActive(bool state)
    {
        for (int i = 0; i < actionButtons.Count; i++)
            actionButtons[i].gameObject.SetActive(state);
    }

    //---------------------------------------------------------------------------------------//

    private static int ClampListBounds(IList list, int index, int direction)
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

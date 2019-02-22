using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character_BaseStats", menuName = "Character/Stats", order = 1)]
public class CharacterBase : MonoBehaviour
{

    public bool turnDone = false;

    public bool isDead => currentState == STATE.DEAD;
    public bool isStunned => currentState == STATE.STUNNED;

    public Transform Transform => transform;

    public string characterName;

    protected STATE currentState;

    protected int stunTime = 0;
    
    //---------------------------------------------------------------------------------------//

    [SerializeField]
    protected TYPE attackType;
    
    [Required, SerializeField]
    protected CharacterStats stats;
    
    //---------------------------------------------------------------------------------------//

    public AbilityScriptableObject[] Abilities => stats.abilities;
    public int CurrentHealth { get; private set; }
    public int StartingHealth => stats.startingHealth;

    private new Transform transform;

    private static GameManager _gameManager;
    
    //---------------------------------------------------------------------------------------//
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        if (!_gameManager)
            _gameManager = FindObjectOfType<GameManager>();
        
        transform = gameObject.transform;

        CurrentHealth = stats.startingHealth;
    }

    //---------------------------------------------------------------------------------------//
    
    private void SetCurrentState(STATE targetState)
    {
        currentState = targetState;

        switch (currentState)
        {
            case STATE.AI:
                break;
            case STATE.WAITING:
                break;
            case STATE.STUNNED:
                break;
            case STATE.BLOCKING:
                break;
            case STATE.DEAD:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    //---------------------------------------------------------------------------------------//

    public void StartTurn()
    {
        if (currentState == STATE.STUNNED)
        {
            if(stunTime == 0)
                SetCurrentState(STATE.WAITING);
            else
            {
                stunTime--;
            }
        }
        else if(isDead)
            return;

        turnDone = false;
    }

    public void EndTurn()
    {
        turnDone = true;
    }

    //---------------------------------------------------------------------------------------//
    
    public void DoDamage(int value)
    {
        if (value == 0)
        {
            Instantiate(_gameManager.risingTextPrefab).GetComponent<RisingText>().Init("Miss", Color.cyan, Transform.position );
            return;
        }
        
        
        
        if (currentState == STATE.BLOCKING)
        {
            SetCurrentState(STATE.WAITING);
            Instantiate(_gameManager.risingTextPrefab).GetComponent<RisingText>().Init("Blocked", Color.cyan, Transform.position);
            return;
        }
        
        
        Instantiate(_gameManager.risingTextPrefab).GetComponent<RisingText>().Init(value.ToString(), Color.red, Transform.position);
        
        CurrentHealth = Mathf.Clamp(CurrentHealth - value, 0, StartingHealth);
        
        if(CurrentHealth == 0)
            SetCurrentState(STATE.DEAD);
    }

    public void Heal(int value)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + value, 0, StartingHealth);
        Instantiate(_gameManager.risingTextPrefab).GetComponent<RisingText>().Init(value.ToString(), Color.green, Transform.position);
    }

    public void Stun(int turns)
    {
        SetCurrentState(STATE.STUNNED);
        Instantiate(_gameManager.risingTextPrefab).GetComponent<RisingText>().Init("Stunned", Color.white, Transform.position);
    }
    
    public void Block()
    {
        SetCurrentState(STATE.STUNNED);
    }
    
    //---------------------------------------------------------------------------------------//

}

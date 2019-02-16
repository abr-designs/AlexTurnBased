using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character_BaseStats", menuName = "Character/Stats", order = 1)]
public class CharacterBase : MonoBehaviour
{

    public bool turnDone = false;
    
    public Transform Transform => transform;

    public string characterName;

    protected STATE currentState;
    
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
    
    //---------------------------------------------------------------------------------------//
    // Start is called before the first frame update
    void Awake()
    {
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
        if(currentState == STATE.STUNNED)
            SetCurrentState(STATE.WAITING);
        else if(currentState == STATE.DEAD)
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
            Debug.LogError("Miss");
            return;
        }
        
        
        
        if (currentState == STATE.BLOCKING)
        {
            SetCurrentState(STATE.WAITING);
            Debug.LogError("Blocked");
            return;
        }
        
        Debug.LogError("Damage: " + value);
        
        CurrentHealth = Mathf.Clamp(CurrentHealth - value, 0, StartingHealth);
        
        if(CurrentHealth == 0)
            SetCurrentState(STATE.DEAD);
    }

    public void Heal(int value)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + value, 0, StartingHealth);
    }

    public void Stun()
    {
        SetCurrentState(STATE.STUNNED);
    }
    
    public void Block()
    {
        SetCurrentState(STATE.STUNNED);
    }
    
    //---------------------------------------------------------------------------------------//

}

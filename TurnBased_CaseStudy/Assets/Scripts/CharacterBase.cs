using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CharacterBase : MonoBehaviour
{
    public AbilityScriptableObject lightAttack { get; protected set; }
    public AbilityScriptableObject heavyAttack { get; protected set; }
    public AbilityScriptableObject heal        { get; protected set; }
    public AbilityScriptableObject block       { get; protected set; }
    public AbilityScriptableObject stun { get; protected set; }
    
    //---------------------------------------------------------------------------------------//
    public bool turnDone = false;

    public bool IsDead => currentState == STATE.DEAD;
    public bool IsStunned => currentState == STATE.STUNNED;
    
    //---------------------------------------------------------------------------------------//
    public Transform Transform => transform;

    public string characterName;

    protected STATE currentState;

    protected int poisonTime = 0;
    protected AbilityScriptableObject poisonProfile;

    protected int stunTime = 0;

    protected int blockCount = 0;
    
    //---------------------------------------------------------------------------------------//

    //[SerializeField]
    public TYPE attackType;
    
    [Required, SerializeField]
    protected CharacterStats stats;
    
    //---------------------------------------------------------------------------------------//

    public AbilityScriptableObject[] Abilities => stats.abilities;
    
    //---------------------------------------------------------------------------------------//

    [SerializeField]
    protected CharacterSpriteLibrary _spriteLibrary;
    
    //---------------------------------------------------------------------------------------//

    
    public float CurrentHealthNormalized => (float)CurrentHealth / (float)StartingHealth;
    public int CurrentHealth { get; private set; }
    public int StartingHealth => stats.startingHealth;
    
    //---------------------------------------------------------------------------------------//

    private GameObject shieldGameObject;
    private GameObject stunGameObject;

    private new Transform transform;
    private new SpriteRenderer renderer;

    private static GameManager _gameManager;
    
    //---------------------------------------------------------------------------------------//
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        if (!_gameManager)
            _gameManager = FindObjectOfType<GameManager>();
        
        transform = gameObject.transform;
        renderer = GetComponent<SpriteRenderer>();

        CurrentHealth = stats.startingHealth;
        
        lightAttack = FindAbility(AbilityType.LightAttack);
        heavyAttack = FindAbility(AbilityType.HeavyAttack);
        heal        = FindAbility(AbilityType.Heal);
        block       = FindAbility(AbilityType.Block);
        stun = FindAbility(AbilityType.Stun);
    }

    //---------------------------------------------------------------------------------------//
    
    private void SetCurrentState(STATE targetState)
    {
        currentState = targetState;

        if(shieldGameObject)
            shieldGameObject.SetActive(false);
        
        if(stunGameObject)
            stunGameObject.SetActive(false);

        renderer.sprite = _spriteLibrary.GetSprite(currentState);
        
        switch (currentState)
        {
            case STATE.AI:
                break;
            case STATE.WAITING:
                break;
            case STATE.STUNNED:
                if (!stunGameObject)
                {
                    stunGameObject = Instantiate(_gameManager.stunEffectPrefab);
                    stunGameObject.transform.position = transform.position + stun.effectOffset;
                }
                
                stunGameObject.SetActive(true);
                break;
            case STATE.BLOCKING:
                if (!shieldGameObject)
                {
                    shieldGameObject                    = Instantiate(_gameManager.shieldSpritePrefab);
                    shieldGameObject.transform.position = transform.position;
                }

                blockCount = block.hitCount;
                shieldGameObject.SetActive(true);
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
        if(IsDead)
            return;
        
        if (poisonTime > 0)
        {
            poisonTime--;
            DoDamage(poisonProfile.valueRange.x, true);
            poisonProfile.ApplyEffectOnTarget(this);

            if (poisonTime == 0)
            {
                poisonProfile = null;
            }

        }
        
        if (currentState == STATE.STUNNED)
        {
            if(stunTime == 0)
                SetCurrentState(STATE.WAITING);
            else
            {
                stunTime--;
            }
        }

        turnDone = false;
    }

    public void EndTurn()
    {
        turnDone = true;
    }

    //---------------------------------------------------------------------------------------//
    
    public void DoDamage(int value, bool ignoreShield = false)
    {
        if (value == 0)
        {
            Instantiate(_gameManager.risingTextPrefab).GetComponent<RisingText>().Init("Miss", Color.cyan, Transform.position );
            return;
        }
        
        
        
        if (currentState == STATE.BLOCKING && !ignoreShield)
        {
            blockCount--;
            
            Instantiate(_gameManager.risingTextPrefab).GetComponent<RisingText>().Init("Blocked", Color.cyan, Transform.position);
            block.ApplyEffectOnTarget(this);

            if (blockCount <= 0)
            {
                blockCount = 0;
                SetCurrentState(STATE.WAITING);
            }
            
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
        stunTime = turns;
        Instantiate(_gameManager.risingTextPrefab).GetComponent<RisingText>().Init("Stunned", Color.white, Transform.position);
    }

    public void Poison(AbilityScriptableObject ability)
    {
        //TODO Going to need to consider having some sort of poison resist
        if (ability.AbilityType != AbilityType.Poison)
            return;

        poisonProfile = ability;
        poisonTime = poisonProfile.hitCount;
        
        DoDamage(poisonProfile.valueRange.x * 2, true);
        poisonProfile.ApplyEffectOnTarget(this);
    }

    public void ShowStunnedState()
    {
        if (currentState != STATE.STUNNED)
            return;
        
        Instantiate(_gameManager.risingTextPrefab).GetComponent<RisingText>().Init("Stunned", Color.white, Transform.position);
    }
    
    public void Block()
    {
        SetCurrentState(STATE.BLOCKING);
        Instantiate(_gameManager.risingTextPrefab).GetComponent<RisingText>().Init("Blocking", Color.cyan, Transform.position);
    }
    
    //---------------------------------------------------------------------------------------//
    
    protected AbilityScriptableObject FindAbility(AbilityType type)
    {
        for (int i = 0; i < stats.abilities.Length; i++)
        {
            if(stats.abilities[i] == null)
                continue;
			
            if (stats.abilities[i].AbilityType == type)
                return stats.abilities[i];
        }

        return null;
    }
    
    [System.Serializable]
    public struct CharacterSpriteLibrary
    {
        public Sprite idleSprite;
        public Sprite deadSprite;

        public Sprite GetSprite(STATE state)
        {
            switch (state)
            {
                case STATE.AI:
                case STATE.WAITING:
                case STATE.STUNNED:
                case STATE.BLOCKING:
                    return idleSprite;
                case STATE.DEAD:
                    return deadSprite;
                    
            }
            
            return idleSprite;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character_BaseStats", menuName = "Character/Stats", order = 1)]
public class CharacterBase : MonoBehaviour
{
    public Transform Transform => transform;

    public string characterName;

    protected STATE currentState;

    [SerializeField]
    protected TYPE attackType;
    
    [Required, SerializeField]
    protected CharacterStats stats;

    public AbilityScriptableObject[] Abilities => stats.abilities;
    public int CurrentHealth { get; private set; }
    public int StartingHealth => stats.startingHealth;

    private new Transform transform;
    
    
    // Start is called before the first frame update
    void Awake()
    {
        transform = gameObject.transform;

        CurrentHealth = stats.startingHealth;
    }

}

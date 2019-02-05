using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character_BaseStats", menuName = "Character/Stats", order = 1)]
public class CharacterStats : ScriptableObject
{
    //public STATE currentState = STATE.WAITING;

    public TYPE attackType;

    public int health;

    [ToggleGroup("lightAttack", groupTitle: "Light Attack")]
    public bool lightAttack;
    [ToggleGroup("lightAttack", groupTitle: "Light Attack")]
    public Vector2Int lightDamage;

    [ToggleGroup("heavyAttack", groupTitle: "Heavy Attack")]
    public bool heavyAttack;
    [ToggleGroup("heavyAttack", groupTitle: "Heavy Attack")]
    public Vector2Int heavyDamage;

    [ToggleGroup("heal", groupTitle: "Healing")]
    public bool heal;
    [ToggleGroup("heal", groupTitle: "Healing")]
    public Vector2Int healAmount;

    [ToggleGroup("block", groupTitle: "Blocking")]
    public bool block;


}

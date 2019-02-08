using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character_BaseStats", menuName = "Character/Stats", order = 1)]
public class CharacterStats : ScriptableObject
{

    public int health;

    public AbilityScriptableObject[] abilities = new AbilityScriptableObject[3];

}

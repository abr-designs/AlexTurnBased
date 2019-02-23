using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : CharacterBase
{
	public AbilityScriptableObject lightAttack { get; private set; }
	public AbilityScriptableObject heavyAttack { get; private set; }
	public AbilityScriptableObject heal { get; private set; }
	public AbilityScriptableObject block { get; private set; }
	
	protected override void Awake()
	{
		base.Awake();

		lightAttack = FindAbility(AbilityType.LightAttack);
		heavyAttack = FindAbility(AbilityType.HeavyAttack);
		heal = FindAbility(AbilityType.Heal);
		block = FindAbility(AbilityType.Block);
	}

	private AbilityScriptableObject FindAbility(AbilityType type)
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
}

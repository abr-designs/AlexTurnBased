using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "Character/Ability", order = 1)]
public class AbilityScriptableObject : ScriptableObject
{
	public string Name;

	public string Description;

	public AbilityType AbilityType;

	public TargetType TargetType;

	[Range(0f,1f)]
	public float chance;

	public Vector2Int valueRange;

	public bool CanTargetSelf;

}

public enum AbilityType
{
	LightAttack,
	HeavyAttack,
	Stun,
	Heal,
	Block
}

public enum TargetType
{
	Enemy,
	Friendly,
	Self
}

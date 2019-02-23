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

	public GameObject effectPrefab;
	public Vector3 effectOffset = Vector3.zero;

	public int GetValueRoll()
	{
		if (Random.value > chance)
			return 0;

		return Random.Range(valueRange.x, valueRange.y + 1);
	}

	public void ApplyEffectOnTarget(CharacterBase character)
	{
		if (effectPrefab == null)
			return;

		var temp = Instantiate(effectPrefab).transform;
		temp.position = character.Transform.position + effectOffset;
	}

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

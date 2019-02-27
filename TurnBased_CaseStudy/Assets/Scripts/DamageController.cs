using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class DamageController : MonoBehaviour
{
	public bool useMultiplier = true;
	[TableList(ShowIndexLabels = true), SerializeField]
	protected List<DamageMultiplier> multipliers;

	public float GetMultiplier(TYPE caster, TYPE target)
	{
		if (!useMultiplier)
			return 1f;
		
		for (int i = 0; i < multipliers.Count; i++)
		{
			if (caster == multipliers[i].casterType && target == multipliers[i].targetType)
				return multipliers[i].multiplier;
		}
		
		return 1f;
	}
	
	[System.Serializable]
	public struct DamageMultiplier
	{
		public TYPE casterType;
		
		public TYPE targetType;
		
		public float multiplier;
	}
}

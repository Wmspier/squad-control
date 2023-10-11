using System;
using UnityEngine;

namespace CharacterBehaviors.Abilities
{
	[Serializable]
	public struct AbilityEffects
	{
		[field: SerializeField] public float Damage { get; private set; }
		[field: SerializeField] public float Healing { get; private set; }
		[field: SerializeField] public bool ApplyStun { get; private set; }
		[field: SerializeField] public float StunDurationSeconds { get; private set; }
	}
}
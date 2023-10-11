using System;
using CharacterBehaviors;
using CharacterBehaviors.Abilities;
using UnityEngine;

namespace Data
{
	[CreateAssetMenu(fileName = "NewAbility", menuName = "Static Data/Ability")]
	[Serializable]
	public class AbilityPayload : ScriptableObject
	{
		[field:SerializeField] public string EffectId { get; private set; }
		[field:SerializeField] public TargetType Target { get; private set; }
		[field:SerializeField] public AbilityEffects Effects { get; private set; }
		[field:SerializeField] public CharacterVfx.VfxPayload VfxPayload { get; private set; }
		[field:SerializeField] public Sprite Icon { get; private set; }
		[field:SerializeField] public float MinRange { get; private set; }
		[field:SerializeField] public float MaxRange { get; private set; }
	}
}
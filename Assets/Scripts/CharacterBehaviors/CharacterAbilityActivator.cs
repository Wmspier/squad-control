using System;
using System.Collections.Generic;
using CharacterBehaviors.Abilities;
using Data;
using Enemy;
using Movement;
using Status;
using UnityEngine;

namespace CharacterBehaviors
{
	public class CharacterAbilityActivator : MonoBehaviour
	{
		[SerializeField] private CharacterAnimation characterAnimation;
		[SerializeField] private CharacterVfx characterVfx;
		[SerializeField] private CharacterCollisionLayer collisionLayer;
		[SerializeField] private List<AbilityPayload> abilities;
		
		[Header("Indication")]
		[SerializeField] private AttackIndicator meleeIndicator;
		[SerializeField] private AttackIndicator rangedIndicator;
		[SerializeField] private float indicationTimeSeconds = .75f;

		private readonly Dictionary<string, AbilityPayload> _abilitiesById = new();
		
		public bool IndicatingAttack { get; private set; }
		public IEnumerable<AbilityPayload> Abilities => abilities;

		public void Initialize(List<GameObject> collisionBlacklist)
		{
			characterVfx.Initialize(collisionBlacklist);
			
			collisionLayer = new CharacterCollisionLayer(gameObject, characterVfx, collisionBlacklist);
			collisionLayer.AbilityHitTarget += OnAbilityHitTarget;

			foreach (var ability in abilities)
			{
				_abilitiesById[ability.EffectId] = ability;
				characterVfx.RegisterVfxPrefab(ability.VfxPayload);
			}
		}

		private void OnDestroy()
		{
			collisionLayer.AbilityHitTarget -= OnAbilityHitTarget;
			collisionLayer.Dispose();
		}

		public void ClearIndicators()
		{
			IndicatingAttack = false;
			if(meleeIndicator != null) meleeIndicator.gameObject.SetActive(false);
			if(rangedIndicator != null) rangedIndicator.gameObject.SetActive(false);
		}

		public void ActivateAbilityAtIndex(int index, bool runIndicator = false, Vector3? targetPosition = null)
		{
			if (index > abilities.Count - 1)
			{
				Debug.LogError($"Failed to active ability at index {index}. Out of Range");
				return;
			}

			var abilityPayload = abilities[index];

			if (runIndicator)
			{
				ShowIndicator(abilityPayload, TriggerAndApply, targetPosition);
			}
			else
			{
				TriggerAndApply();
			}

			void TriggerAndApply()
			{
				characterAnimation.PlayWithTrigger(abilityPayload.EffectId);

				if (abilityPayload.Target == TargetType.Self)
				{
					ApplyEffect(abilityPayload, GetComponent<CharacterStatus>(), GetComponent<CharacterAnimation>());
				}
			}
		}

		private void ShowIndicator(AbilityPayload payload, Action postIndicationAction, Vector3? targetPosition = null)
		{
			IndicatingAttack = true;
			if (payload.VfxPayload.abilityType == CharacterVfx.AbilityType.Melee)
			{
				meleeIndicator.gameObject.SetActive(true);
				meleeIndicator.IndicationComplete = () =>
				{
					postIndicationAction?.Invoke();
					meleeIndicator.gameObject.SetActive(false);
					IndicatingAttack = false;
				};
				meleeIndicator.Run(indicationTimeSeconds);
			}
			else
			{
				rangedIndicator.gameObject.SetActive(true);
				
				var t = rangedIndicator.transform;
				t.parent = characterVfx.WorldVfxRoot;
				if (targetPosition != null)
					t.position = targetPosition.Value;

				characterVfx.SetProjectileDestination(t.position);
				rangedIndicator.Run(indicationTimeSeconds);
				rangedIndicator.IndicationComplete = () =>
				{
					postIndicationAction?.Invoke();
					rangedIndicator.transform.parent = transform;
					rangedIndicator.gameObject.SetActive(false);
					IndicatingAttack = false;
				};
			}
		}

		private void OnAbilityHitTarget(GameObject target, string effectId)
		{
			if (!_abilitiesById.TryGetValue(effectId, out var abilityPayload))
			{
				Debug.LogError($"Failed to activate ability for effect {effectId}. No payload found.");
				return;
			}

			if (!IsCorrectTarget(target, abilityPayload)) return;
			
			var status = target.GetComponent<CharacterStatus>();
			var animationHandler = target.GetComponent<CharacterAnimation>();
			if (status == null || animationHandler == null)
			{
				Debug.LogWarning($"Failed to activate ability for effect {effectId}. " +
				               $"StatusFound: {status != null} " +
				               $"AnimationHandlerFound: {animationHandler != null} +" +
				               $"Target: {target.name}");
				return;
			}
			
			ApplyEffect(abilityPayload, status, animationHandler);
		}

		private static void ApplyEffect(AbilityPayload abilityPayload, CharacterStatus targetStatus, CharacterAnimation targetAnimation)
		{
			// Apply Damage
			if (abilityPayload.Effects.Damage > 0f && targetStatus.IsAlive)
			{
				targetStatus.DealDamage(abilityPayload.Effects.Damage);
				targetAnimation.PlayHitReact();
			}

			// Heal
			if (abilityPayload.Effects.Healing > 0f && targetStatus.IsAlive)
			{
				targetStatus.Heal(abilityPayload.Effects.Healing);
			}
			
			// Stun
			if (abilityPayload.Effects.ApplyStun)
			{
				targetStatus.GetComponent<CharacterMovement>().ApplyStun(abilityPayload.Effects.StunDurationSeconds);
			}
		}

		private bool IsCorrectTarget(GameObject target, AbilityPayload abilityPayload)
		{
			// Target is not a character and is irrelevant
			if (!target.CompareTag("Squad_Member") && !target.CompareTag("Enemy")) return false;
			
			// For now, ability collisions should not affect the caster
			if (target == gameObject) return false;
			
			if (abilityPayload.Target == TargetType.Enemies)
			{
				return IsEnemy(target);
			}

			return !IsEnemy(target);
		}
		
		private bool IsEnemy(GameObject obj)
		{
			if (gameObject.CompareTag("Squad_Member")) return obj.CompareTag("Enemy");
			return gameObject.CompareTag("Enemy") && obj.CompareTag("Squad_Member");
		}
	}
}
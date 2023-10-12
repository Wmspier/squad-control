using System;
using System.Linq;
using CharacterBehaviors;
using Data;
using Squad;
using Status;
using UnityEngine;

namespace Player
{
	public class PlayerBasicAttack : MonoBehaviour
	{
		[SerializeField] private float basicAttackFrequencySeconds = .75f;
		[SerializeField] private CharacterVfx characterVfx;
		[SerializeField] private bool isMelee;

		public Action TargetKilled;
		
		private SquadZone _zone;
		private float _previousBasicAttackTime;
		private CharacterStatus _target;

		[field: SerializeField] public CharacterAbilityActivator AbilityActivator { get; private set; }
		private int AbilityIndex => isMelee ? 0 : 1;
		private AbilityPayload AbilityPayload => AbilityActivator.Abilities.ElementAt(AbilityIndex);

		public Vector3 TargetPosition => _target.transform.position;

		public float StoppingDistance => IsCloseEnoughToAttack() ? AbilityPayload.MinRange : 0f;

		public void SetTarget(GameObject enemy) => _target = enemy.GetComponent<CharacterStatus>();
		public void SetIsMelee(bool melee) => isMelee = melee;
		public void ClearTarget() => _target = null;

		public bool IsTarget(GameObject enemy) => _target != null && _target.gameObject == enemy;
		public bool HasTarget => _target != null;

		private void Update()
		{
			if (_target == null) return;
			
			Transform t;
			(t = transform).LookAt(_target.transform);
			
			var timeSinceAttack = Time.time - _previousBasicAttackTime;
			if(timeSinceAttack < basicAttackFrequencySeconds) return;

			if (!_target.IsAlive)
			{
				ClearTarget();
				TargetKilled?.Invoke();
				return;
			}

			if (!IsCloseEnoughToAttack()) return;

			
			characterVfx.SetProjectileDirection((_target.transform.position - t.position).normalized);
			AbilityActivator.ActivateAbilityAtIndex(AbilityIndex);
			_previousBasicAttackTime = Time.time;
		}
		
		private bool IsCloseEnoughToAttack()
		{
			return Vector3.Distance(transform.position, _target.transform.position) <= AbilityPayload.MaxRange;
		}
	}
}
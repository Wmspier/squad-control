using System;
using CharacterBehaviors;
using Movement;
using Player;
using UnityEngine;

namespace Squad
{
	public class SquadMember : MonoBehaviour
	{
		public enum MovementBehavior
		{
			Fixed, // Fixed in position in Squad
			FollowingTarget, // Following a target outside of Squad (but inside zone)
			ReturningToSquad, // Return to formation within Squad
			FollowingSquad // Following position in squad
		}

		[SerializeField] private FollowCharacterMovement followBehavior;
		[SerializeField] private float followStoppingDistance;
		[SerializeField] private float returningStoppingDistance;
		[SerializeField] private CharacterType type;
		
		[field: SerializeField] public CharacterAnimation Animation { get; private set; }
		[field: SerializeField] public MovementBehavior CurrentBehavior { get; private set; } = MovementBehavior.FollowingSquad;
		[field: SerializeField] public PlayerBasicAttack AttackBehavior { get; private set; }

		public event Action<SquadMember> TargetReached;
		public Action<SquadMember> TargetEnemyKilled;

		private float _previousTargetSearchTime;

		public bool HasTargetEnemy => AttackBehavior.HasTarget;
		public CharacterAbilityActivator AbilityActivator => AttackBehavior.AbilityActivator;
		public CharacterTypeEnum Type => type.Type;

		private void Awake()
		{
			AttackBehavior.TargetKilled = () => TargetEnemyKilled?.Invoke(this);
		}

		public void SetTargetPosition(Vector3 target) => followBehavior.SetTargetPosition(target);

		public void SetCharacterType(CharacterTypeEnum characterType) => type.SetType(characterType);
		
		public void SwitchMovementBehavior(MovementBehavior newBehavior)
		{
			CurrentBehavior = newBehavior;
			switch (newBehavior)
			{
				case MovementBehavior.Fixed:
				{
					followBehavior.enabled = false;
					break;
				}
				case MovementBehavior.FollowingTarget:
				{
					followBehavior.TargetReached += OnTargetReached;
					followBehavior.enabled = true;

					var stoppingDistance = HasTargetEnemy 
						? Mathf.Min(followStoppingDistance, AttackBehavior.StoppingDistance) 
						: followStoppingDistance;
					followBehavior.SetAgentStoppingDistance(stoppingDistance);
					break;
				}
				case MovementBehavior.ReturningToSquad:
				case MovementBehavior.FollowingSquad:
				{
					followBehavior.TargetReached += OnTargetReached;
					followBehavior.enabled = true;
					followBehavior.SetAgentStoppingDistance(returningStoppingDistance);
					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(newBehavior), newBehavior, null);
			}
		}

		private void OnTargetReached()
		{
			followBehavior.TargetReached -= OnTargetReached;
			TargetReached?.Invoke(this);
		}

		public void SetTargetEnemy(GameObject enemy) => AttackBehavior.SetTarget(enemy);
		
		public void ClearTarget() => AttackBehavior.ClearTarget();

		public bool IsTarget(GameObject enemy) => AttackBehavior.IsTarget(enemy);
	}
}
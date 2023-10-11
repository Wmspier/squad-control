using System.Collections.Generic;
using System.Linq;
using CharacterBehaviors;
using Movement;
using Status;
using Test;
using UnityEngine;

namespace Squad
{
	public class Squad : FixedCharacterMovement
	{
		[SerializeField] private SquadZoneGroup _squadZones;
		[SerializeField] private Transform _squadMemberRoot;
		[SerializeField] private float _squadMemberDistance;
		[SerializeField] private SquadMember _squadMemberPrefab;
		[SerializeField] private List<GameObject> _collisionBlacklist;

		private Vector3[] _memberPositionsRelative;
		private bool _isMoving;
		private bool _isAttacking;

		private List<SquadMember> Members { get; set; }

		private void Start()
		{
			_squadZones.InteractableEnteredZone += OnInteractableEnteredZones;
			_squadZones.AllInteractablesExitedZone += OnAllInteractablesExitedZones;

			_squadZones.EnemyEnteredZone += OnEnemyEnteredZones;
			_squadZones.EnemyExitedZone += OnEnemyExitedZones;

			Members = new List<SquadMember>(_squadMemberRoot.GetComponentsInChildren<SquadMember>());
			
			// Start with at least one member
			AddSquadMember();
		}

		private void Update()
		{
			_squadZones.transform.position = transform.position;

			if (IsReceivingInput)
			{
				if(!_isMoving) HandleStartMoving();
			} else if (_isMoving)
			{
				HandleStopMoving();
			}

			ResetMemberTargetPositions();
		}
		
		#region Configuration
		// Set the scale of the inner zone
		public void SetInnerZoneScale(float scale)
		{
			_squadZones.SetInnerZoneScale(scale);
			_squadZones.SetOuterZoneScaleRelative(1.5f);
		}
		// Set the relative scale of the outer zone
		public void SetOuterZoneScaleRelative(float scale)
		{
			_squadZones.SetOuterZoneScaleRelative(scale);
		}

		// Set the distance between squad members
		public void SetMemberDistance(float distance)
		{
			_squadMemberDistance = distance;
			if (Members is { Count: > 0 })
			{
				_memberPositionsRelative = GetMemberFixedPositions(Members.Count);
				ResetMemberTargetPositions();
			}
		}
		#endregion

		#region Member Management

		// Instantiate and Add a new Squad Member
		public void AddSquadMember()
		{
			var newMember = Instantiate(_squadMemberPrefab, _squadMemberRoot);
			newMember.transform.position = transform.position;
			var blacklist = new List<GameObject>(_collisionBlacklist) { newMember.gameObject };
			var isMelee = Random.Range(0, 2) == 0;
			
			newMember.AbilityActivator.Initialize(blacklist);
			newMember.TargetEnemyKilled = OnMemberKilledTarget;
			newMember.AttackBehavior.SetIsMelee(isMelee);
			newMember.SetCharacterType(isMelee ? CharacterTypeEnum.PlayerMelee : CharacterTypeEnum.PlayerRanged);

			Members.Add(newMember);
			
			_memberPositionsRelative = GetMemberFixedPositions(Members.Count);
			ResetMemberTargetPositions();
			ResetMemberScales();
		}

		// Remove the last added Squad Member
		public void RemoveSquadMember()
		{
			if(Members.Count == 0) return;
			var latestMember = Members.Last();
			Members.Remove(latestMember);
			_memberPositionsRelative = GetMemberFixedPositions(Members.Count);
			Destroy(latestMember.gameObject);
			ResetMemberTargetPositions();
		}

		public SquadMember GetRandomMember() => Members[Random.Range(0, Members.Count)];
		
		#endregion

		#region Movement
		protected override void HandleMove()
		{
			base.HandleMove();

			if (_memberPositionsRelative is not { Length: > 0 }) return;

			var normalizedMagnitude = Mathf.InverseLerp(config.MinRunDirectionalMagnitude, config.MaxRunDirectionalMagnitude, _direction.magnitude);

			for (var i = 0; i < _memberPositionsRelative.Length; i++)
			{
				var member = Members[i];

				// Member is following a target, don't force its position
				if (member.CurrentBehavior 
				    is SquadMember.MovementBehavior.FollowingTarget 
				    or SquadMember.MovementBehavior.FollowingSquad) 
					return;
				
				// Member is returning to position, update its target
				if (member.CurrentBehavior is SquadMember.MovementBehavior.ReturningToSquad)
				{
					SetMemberTargetClamped(member, transform.position + _memberPositionsRelative[i]);
					continue;
				}

				var memberTransform = member.transform;
				var squadTransform = transform;
				
				// Member is fixed, force its position
				memberTransform.position = squadTransform.position + _memberPositionsRelative[i];
				memberTransform.rotation = squadTransform.rotation;
				member.Animation.SetRunBlend(normalizedMagnitude);
			}
		}

		private void HandleStartMoving()
		{
			_isMoving = true;
			
			// If the squad needs to be stopped to interact
			// clear all interaction when moving
			if (StaticTestData.Instance.StopToInteract)
			{
				OnAllInteractablesExitedZones(true);
				ClearAllMemberTargetEnemies();
			}
			
			_squadZones.ToggleColor(_isMoving, _isAttacking);
		}
		
		private void HandleStopMoving()
		{
			_isMoving = false;
			
			// If the squad needs to be stopped to interact
			// set up all interactions when stopped
			if (StaticTestData.Instance.StopToInteract)
			{
				// Prioritize interactables
				if (_squadZones.IsInteractableInZone(true))
				{
					var interactable = _squadZones.Interactables(true).First();
					foreach (var member in Members)
					{
						member.SwitchMovementBehavior(SquadMember.MovementBehavior.FollowingTarget);
						SetMemberTargetClamped(member, interactable.transform.position);
					}
				}
				else
				{
					SetAllMemberTargetEnemies();
				}
			}
			
			_squadZones.ToggleColor(_isMoving, _isAttacking);
		}
		#endregion
		
		#region Interactables
		private void OnInteractableEnteredZones(GameObject interactable, bool isInner)
		{
			// Ignore outer zone for now
			if (!isInner) return;
			
			// Stop to move is enabled and the zone is moving
			if (StaticTestData.Instance.StopToInteract && IsReceivingInput) return;
			
			foreach (var member in Members)
			{
				member.SwitchMovementBehavior(SquadMember.MovementBehavior.FollowingTarget);
				SetMemberTargetClamped(member, interactable.transform.position);
			}
		}

		private void OnAllInteractablesExitedZones(bool isInner)
		{
			// Ignore outer zone for now
			if (!isInner) return;
			
			for (var i = 0; i < Members.Count; i++)
			{
				Members[i].TargetReached += OnTargetReached;
				SetMemberTargetClamped(Members[i], transform.position + _memberPositionsRelative[i]);
				Members[i].SwitchMovementBehavior(SquadMember.MovementBehavior.ReturningToSquad);
			}

			void OnTargetReached(SquadMember member)
			{
				member.TargetReached -= OnTargetReached;
				member.SwitchMovementBehavior(SquadMember.MovementBehavior.FollowingSquad);
				//member.SwitchMovementBehavior(SquadMember.MovementBehavior.Fixed);
			}
		}
		#endregion
		
		#region Enemies
		private void OnEnemyEnteredZones(GameObject enemy, bool isInner)
		{
			// If the squad is moving and needs to stop to attack don't do anything
			if(StaticTestData.Instance.StopToInteract && _isMoving) return;

			// If this enemy is dead ignore it
			if (!enemy.GetComponent<CharacterStatus>().IsAlive) return;

			// Outer Zone
			if (!isInner)
			{
				var rangedMembers = Members.Where(m => m.Type == CharacterTypeEnum.PlayerRanged).ToList();
				if (rangedMembers.Count == 0) return;
				
				_isAttacking = true;
				_squadZones.ToggleColor(_isMoving, _isAttacking);
				foreach (var member in rangedMembers)
				{
					if (!member.HasTargetEnemy)
					{
						SetMemberTargetEnemy(member, enemy.gameObject);
					}
				}

				return;
			}
			
			_isAttacking = true;
			_squadZones.ToggleColor(_isMoving, _isAttacking);
			
			foreach (var member in Members)
			{
				if (!member.HasTargetEnemy)
				{
					SetMemberTargetEnemy(member, enemy.gameObject);
				}
			}
		}

		private void OnEnemyExitedZones(GameObject enemy, bool isInner)
		{
			// enemies have left all zones
			if (_squadZones.GetLivingEnemies(true).Count == 0 && 
			    _squadZones.GetLivingEnemies(false).Count == 0)
			{
				_isAttacking = false;
				_squadZones.ToggleColor(_isMoving, _isAttacking);
			}
			
			foreach (var member in Members)
			{
				// Member is focusing on someone else
				if (!member.IsTarget(enemy)) continue;

				// If the enemy only left the inner zone and the member is ranged skip it
				if (isInner && member.Type == CharacterTypeEnum.PlayerRanged) continue;
				
				// Clear the target
				member.ClearTarget();
				if (_squadZones.Enemies(true).Count == 0)
				{
					// If there are no other enemies in the zone return to the squad
					member.SwitchMovementBehavior(SquadMember.MovementBehavior.ReturningToSquad);
				}
				// Only find new enemies if zone doesn't need to be stopped or zone needs to be and is stopped
				else if (!StaticTestData.Instance.StopToInteract || StaticTestData.Instance.StopToInteract && !_isMoving)
				{
					var newEnemy = _squadZones.GetFirstLivingEnemy(true);
					if (newEnemy == null)
					{
						// If there are no other living enemies in the zone return to the squad
						member.SwitchMovementBehavior(SquadMember.MovementBehavior.ReturningToSquad);
						continue;
					}

					SetMemberTargetEnemy(member, newEnemy.gameObject);
				}
			}
		}
		
		private void OnMemberKilledTarget(SquadMember member)
		{
			// If the squad is moving and needs to stop to attack don't do anything
			if(StaticTestData.Instance.StopToInteract && _isMoving) return;
			
			// If the member is melee, only get enemies in the inner zone
			var newEnemy = _squadZones.GetFirstLivingEnemy(member.Type == CharacterTypeEnum.PlayerMelee);
			if (newEnemy == null)
			{
				// If there are no other living enemies in the zone return to the squad
				member.SwitchMovementBehavior(SquadMember.MovementBehavior.ReturningToSquad);
				
				// Toggle the attacking/moving zone states
				_isAttacking = false;
				_squadZones.ToggleColor(_isMoving, _isAttacking);
				
				return;
			}

			SetMemberTargetEnemy(member, newEnemy.gameObject);
		}
		
		private void SetMemberTargetEnemy(SquadMember member, GameObject enemy)
		{
			member.SetTarget(enemy.gameObject);
			SetMemberTargetClamped(member, enemy.transform.position);
			member.SwitchMovementBehavior(SquadMember.MovementBehavior.FollowingTarget);
		}

		private void SetAllMemberTargetEnemies()
		{
			var enemy = _squadZones.GetFirstLivingEnemy(false);
			// No enemies in any zone
			if(enemy == null) return;
			
			// Assign targets to ranged members first using the outer zone
			var rangedMembers = Members.Where(m => m.Type == CharacterTypeEnum.PlayerRanged).ToList();
			foreach (var member in rangedMembers)
			{
				SetMemberTargetEnemy(member, enemy.gameObject);
			}
			_isAttacking = rangedMembers.Count > 0;
			
			// Then assign targets to all melee members using inner zone
			enemy = _squadZones.GetFirstLivingEnemy(true);
			// No enemies in inner zone
			if(enemy == null) return;
			
			var meleeMembers = Members.Where(m => m.Type == CharacterTypeEnum.PlayerMelee).ToList();
			foreach (var member in meleeMembers)
			{
				SetMemberTargetEnemy(member, enemy.gameObject);
			}
			
			_isAttacking |= meleeMembers.Count > 0;
		}
		
		#endregion
		
		#region Utilities
		private void SetMemberTargetClamped(SquadMember member, Vector3 target)
		{
			var position = transform.position;
			var distance = Vector3.Distance(position, target);
			if (distance <= _squadZones.InnerZoneScale/2)
			{
				member.SetTargetPosition(target);
			}
			else
			{
				var direction = (target - position).normalized;
				member.SetTargetPosition(position + direction * _squadZones.InnerZoneScale/2);
			}
		}
		
		// Set the target position of all members but clamp it to the radius of the squad zone
		public void SetMembersTargetClamped(Vector3 target)
		{
			foreach (var member in Members)
			{
				SetMemberTargetClamped(member, target);
			}
		}

		private Vector3[] GetMemberFixedPositions(int numMembers)
		{
			var positionOffsets = new Vector3[numMembers];
			if (numMembers == 0) return positionOffsets;

			if (numMembers == 1)
			{
				positionOffsets[0] = Vector3.zero;
				return positionOffsets;
			}

			if (StaticTestData.Instance.SquadLeader)
			{
				positionOffsets[0] = Vector3.zero;

				var degreesPerMember = 360 / (numMembers - 1);
				var currentDegree = 0;
				for (var i = 1; i < numMembers; i++)
				{
					var positionOffsetX = _squadMemberDistance * Mathf.Cos(Mathf.Deg2Rad * currentDegree);
					var positionOffsetZ = _squadMemberDistance * Mathf.Sin(Mathf.Deg2Rad * currentDegree);
					var memberPosition = new Vector3(positionOffsetX, 0, positionOffsetZ);
					positionOffsets[i] = memberPosition;

					currentDegree += degreesPerMember;
				}
			}
			else
			{
				var degreesPerMember = 360 / numMembers;
				var currentDegree = 0;
				for (var i = 0; i < numMembers; i++)
				{
					var positionOffsetX = _squadMemberDistance * Mathf.Cos(Mathf.Deg2Rad * currentDegree);
					var positionOffsetZ = _squadMemberDistance * Mathf.Sin(Mathf.Deg2Rad * currentDegree);
					var memberPosition = new Vector3(positionOffsetX, 0, positionOffsetZ);
					positionOffsets[i] = memberPosition;

					currentDegree += degreesPerMember;
				}
			}

			return positionOffsets;
		}

		private void ClearAllMemberTargetEnemies()
		{
			foreach (var member in Members)
			{
				if(!member.HasTargetEnemy) continue;
				
				member.ClearTarget();
				member.SwitchMovementBehavior(SquadMember.MovementBehavior.ReturningToSquad);
			}
		}

		public void ResetMemberTargetPositions()
		{
			if (_memberPositionsRelative == null) return;

			_memberPositionsRelative = GetMemberFixedPositions(Members.Count);
			
			for (var i = 0; i < Mathf.Min(Members.Count, _memberPositionsRelative.Length); i++)
			{
				if (Members[i].CurrentBehavior is SquadMember.MovementBehavior.FollowingTarget) continue;
				
				SetMemberTargetClamped(Members[i], transform.position + _memberPositionsRelative[i]);
			}
		}

		public void ResetMemberScales()
		{
			if (Members == null) return;
			
			for (var index = 0; index < Members.Count; index++)
			{
				var member = Members[index];
				
				var scale = StaticTestData.Instance.SquadMemberScale * 
				            (index == 0 && StaticTestData.Instance.SquadLeader 
					            ? StaticTestData.SquadMemberLeaderScaleFactor 
					            : 1f);
				member.transform.localScale = new Vector3(scale, scale, scale);
			}
		}
		
		#endregion
	}
}
using System.Collections.Generic;
using Enemy;
using UnityEngine;
using UnityEngine.Assertions;

namespace Player
{
	public class GroupedPlayerMovement : AbstractPlayerMovement
	{
		[SerializeField] private Transform membersRoot;

		private List<BasicEnemyController> _members = new();
		
		private void Start()
		{
			_members.AddRange(membersRoot.GetComponentsInChildren<BasicEnemyController>());
		}
		
		private void Awake()
		{
			Assert.IsNotNull(virtualJoystick, "VirtualJoystick is null and PlayerMovement depends on it.");
			Assert.IsNotNull(characterController, "CharacterController is null and PlayerMovement depends on it.");
		}

		protected override void HandleMove()
		{
			base.HandleMove();
			
			var directionMagnitude = _direction.magnitude;
			var normalizedDirection = _direction.normalized;
			var normalizedMagnitude =
				Mathf.InverseLerp(minRunDirectionalMagnitude, maxRunDirectionalMagnitude, directionMagnitude);
			var adjustedSpeed = speed * normalizedMagnitude;

			var motion = adjustedSpeed * normalizedDirection;
			// Only move the character and adjust the animation if outside of dead zone
			if (directionMagnitude > 0)
			{
				characterController.SimpleMove(motion);
			}
			
			foreach (var member in _members)
			{
				member.SetDestination(transform.position);
			}
		}
	}
}
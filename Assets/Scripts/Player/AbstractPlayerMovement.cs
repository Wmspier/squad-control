using Movement;
using UI;
using UnityEngine;

namespace Player
{
	public class AbstractPlayerMovement : CharacterMovement
	{
		[SerializeField] protected VirtualJoystick virtualJoystick;
		[SerializeField] protected CharacterController characterController;

		[Header("Settings")] 
		[SerializeField] private bool handleRotation = true;
		[SerializeField] protected float speed;
		[SerializeField] private float rotationFactorPerFrame;
		[SerializeField] private float movementDecay;
		
		[SerializeField] protected float maxRunDirectionalMagnitude;
		[SerializeField] protected float minRunDirectionalMagnitude;
		
		protected Vector3 _direction;
		
		protected override void HandleMove()
		{
			UpdateDirection();
			if (handleRotation)
			{
				UpdateRotation();
			}
		}
		
		private void UpdateDirection()
		{
			if (virtualJoystick.Direction.HasValue)
			{
				_direction.x = virtualJoystick.Direction.Value.x;
				_direction.z = virtualJoystick.Direction.Value.y;
				return;
			}

			_direction *= 1 - movementDecay * Time.deltaTime;
		}

		private void UpdateRotation()
		{
			if (_direction == Vector3.zero) return;

			var currentRotation = transform.rotation;
			var targetRotation = Quaternion.LookRotation(_direction);
			transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame);
		}
	}
}
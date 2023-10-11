using System;
using System.Collections.Generic;
using CharacterBehaviors;
using Data;
using UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace Movement
{
	/// <summary>
	/// Fixed movement assures that the characters' movement is directly based on the input from the joystick
	/// </summary>
	[RequireComponent(typeof(CharacterController))]
	public class FixedCharacterMovement : CharacterMovement
	{
		[SerializeField] protected VirtualJoystick joystick;
		[SerializeField] protected CharacterController characterController;
		[SerializeField] protected CharacterAnimation characterAnimationNullable;
		[SerializeField] protected List<CharacterAnimation> characterAnimationsNullable = new();
		[SerializeField] protected MovementConfig config;
		
		[SerializeField][Tooltip("Rotate character to always face forward")] private bool handleRotation = true;
		
		protected Vector3 _direction;

		private void OnDisable() => characterController.enabled = false;
		private void OnEnable() => characterController.enabled = true;

		protected bool IsReceivingInput => joystick.Direction?.magnitude > 0.01f;
		
		public void SetHandleRotation(bool handle) => handleRotation = handle;
		public void SetJoystick(VirtualJoystick joystick) => this.joystick = joystick;
		
		private void Awake()
		{
			if(!enabled) return;
			
			Assert.IsNotNull(joystick, "Joystick is null and FixedCharacterMovement depends on it.");

			characterController.slopeLimit = config.SlopeLimit;
			characterController.stepOffset = config.StepOffset;
		}

		protected override void HandleMove()
		{
			UpdateDirection();
			if (handleRotation && _direction.magnitude > 0)
			{
				UpdateRotation();
			}
			
			var normalizedMagnitude = Mathf.InverseLerp(config.MinRunDirectionalMagnitude, config.MaxRunDirectionalMagnitude, _direction.magnitude);
			var adjustedSpeed = config.Speed * normalizedMagnitude;
			var motion = adjustedSpeed * _direction.normalized;
			
			// Only move the character and adjust the animation if outside of dead zone
			if (_direction.magnitude > 0)
			{
				characterController.SimpleMove(motion);
				if (characterAnimationNullable != null)
				{
					characterAnimationNullable.SetRunBlend(normalizedMagnitude);
				}
				foreach (var anim in characterAnimationsNullable)
				{
					anim.SetRunBlend(normalizedMagnitude);
				}
			}
			// Otherwise set the run animation to 0
			else
			{
				if (characterAnimationNullable != null)
				{
					characterAnimationNullable.SetRunBlend(0f);
				}

				foreach (var anim in characterAnimationsNullable)
				{
					anim.SetRunBlend(0f);
				}
			}
		}
		
		private void UpdateDirection()
		{
			if (joystick.Direction.HasValue)
			{
				_direction.x = joystick.Direction.Value.x;
				_direction.z = joystick.Direction.Value.y;
				return;
			}

			_direction *= 1 - config.MovementDecay * Time.deltaTime;
		}

		private void UpdateRotation()
		{
			if (_direction == Vector3.zero) return;

			var currentRotation = transform.rotation;
			var targetRotation = Quaternion.LookRotation(_direction);
			transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, config.RotationFactorPerFrame);
		}
	}
}
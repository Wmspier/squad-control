using System.Collections;
using CharacterBehaviors;
using Status;
using UnityEngine;
using UnityEngine.Assertions;

namespace Player
{
	public class PlayerMovement : AbstractPlayerMovement
	{
		[SerializeField] private CharacterAnimation characterAnimation;
		[SerializeField] private CharacterStatus status;
		
		[SerializeField] private float rollSpeedMultiplier;
		[SerializeField] private float rollDuration;
		[SerializeField] private float rollInvulnerabilityPeriodSeconds = .15f;

		// For Debug
		private float _adjustedSpeed;

		private bool _rolling;

		private bool CanRoll => !_rolling && status.Stamina.IsFull;

		private void Awake()
		{
			Assert.IsNotNull(virtualJoystick, "VirtualJoystick is null and PlayerMovement depends on it.");
			Assert.IsNotNull(characterController, "CharacterController is null and PlayerMovement depends on it.");
			Assert.IsNotNull(characterAnimation, "CharacterAnimation is null and PlayerMovement depends on it.");
			Assert.IsNotNull(status, "CharacterStatus is null and PlayerMovement depends on it.");

			virtualJoystick.TapEvent += OnTap;
		}

		protected override void HandleMove()
		{
			base.HandleMove();

			var directionMagnitude = _direction.magnitude;
			var normalizedDirection = _direction.normalized;
			var normalizedMagnitude =
				Mathf.InverseLerp(minRunDirectionalMagnitude, maxRunDirectionalMagnitude, directionMagnitude);
			var adjustedSpeed = _rolling ? speed * rollSpeedMultiplier : speed * normalizedMagnitude;
			_adjustedSpeed = adjustedSpeed;

			var motion = adjustedSpeed * normalizedDirection;
			// Only move the character and adjust the animation if outside of dead zone
			if (directionMagnitude > 0)
			{
				characterController.SimpleMove(motion);
				characterAnimation.SetRunBlend(normalizedMagnitude);
			}
			// Otherwise set the run animation to 0
			else
			{
				characterAnimation.SetRunBlend(0f);
			}
		}

		private void OnDestroy() { virtualJoystick.TapEvent -= OnTap; }

		private void OnTap()
		{
			if (!CanRoll) return;

			_rolling = true;
			status.ToggleInvulnerable(true);
			status.ExhaustStamina();
			characterAnimation.PlayRoll();

			StartCoroutine(RollCooldown());

			IEnumerator RollCooldown()
			{
				var elapsed = 0f;
				while (elapsed < rollDuration)
				{
					elapsed += Time.deltaTime;

					if (elapsed >= rollInvulnerabilityPeriodSeconds) status.ToggleInvulnerable(false);

					yield return null;
				}

				_rolling = false;
			}
		}
	}
}
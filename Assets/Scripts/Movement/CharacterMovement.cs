using UnityEngine;

namespace Movement
{
	public abstract class CharacterMovement : MonoBehaviour
	{
		protected bool Stunned { get; private set; }

		private float _stunStartTime;
		private float _currentStunDuration;

		public void ApplyStun(float duration)
		{
			// Already stunned by a longer effect
			if (Stunned && Time.time - _currentStunDuration >= duration) return;
			
			Stunned = true;
			_stunStartTime = Time.time;
			_currentStunDuration = duration;
		}

		protected void LateUpdate()
		{
			if (!Stunned)
			{
				HandleMove();
				return;
			}

			if (Time.time - _stunStartTime >= _currentStunDuration)
			{
				Stunned = false;
			}
		}

		protected abstract void HandleMove();
	}
}
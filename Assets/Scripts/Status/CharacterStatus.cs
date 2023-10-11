using System;
using UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace Status
{
	public class CharacterStatus : MonoBehaviour
	{
		[SerializeField] private StatBar healthBar;
		[SerializeField] [Min(1f)] private float startingHealth;
		[SerializeField] private float invulnerabilityPeriodSeconds = .75f;

		[SerializeField] private StatBar staminaBar;
		[SerializeField] [Min(1f)] private float startingStamina;
		[SerializeField] private float staminaRegardRateSeconds = 2f;

		private Stat _health;
		private bool _invulnerable;
		private float _previousDamageTime;
		private Stat _stamina;

		public bool IsAlive => !_health.IsEmpty;
		public Stat Stamina => _stamina;

		public void Awake()
		{
			_health = new Stat(startingHealth);
			if (healthBar != null) healthBar.SetImmediate(1f);

			_stamina = new Stat(startingStamina);
			if (staminaBar != null) staminaBar.SetImmediate(1f);
		}

		private void Update()
		{
			if (_stamina.IsFull) return;

			var staminaChange = staminaRegardRateSeconds * Time.deltaTime;
			_stamina.Modify(staminaChange);
			if(staminaBar != null) staminaBar.Modify(_stamina.NormalizedAmount(staminaChange));

			if (_stamina.IsFull) staminaBar.Punch();
		}

		public event Action HealthDepleted;

		public void ToggleInvulnerable(bool invulnerable) { _invulnerable = invulnerable; }

		public void ToggleDisplay(bool visible)
		{
			if(healthBar != null) healthBar.Toggle(visible);
			if(staminaBar != null) staminaBar.Toggle(visible);
		}

		public void DealDamage(float amount)
		{
			if (_invulnerable || Time.time - _previousDamageTime < invulnerabilityPeriodSeconds
			    || amount <= 0) return;

			_health.Modify(-amount);
			_previousDamageTime = Time.time;

			if(healthBar != null) healthBar.Toggle(true);
			if(healthBar != null) healthBar.Modify(-_health.NormalizedAmount(amount), true);
			if(healthBar != null) healthBar.Flash(Color.red);

			if (_health.IsEmpty) HealthDepleted?.Invoke();
		}

		public void Heal(float amount)
		{
			if (amount <= 0) return;
			
			_health.Modify(amount);
			if(healthBar != null) healthBar.Toggle(true);
			if(healthBar != null) healthBar.Modify(_health.NormalizedAmount(amount), true);
			if(healthBar != null) healthBar.Flash(Color.green);
		}

		public void ExhaustStamina()
		{
			_stamina.SetImmediate(0f);
			if(staminaBar != null) staminaBar.SetImmediate(0f);
		}
	}
}
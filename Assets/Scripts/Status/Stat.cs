using UnityEngine;

namespace Status
{
	public struct Stat
	{
		public float BaseValue { get; }
		public float CurrentValue;

		public float NormalizedValue => CurrentValue / BaseValue;
		public bool IsEmpty => CurrentValue <= 0f;
		public bool IsFull => CurrentValue >= BaseValue;

		public Stat(float baseValue) { BaseValue = CurrentValue = baseValue; }

		public void Modify(float amount) { CurrentValue = Mathf.Clamp(CurrentValue + amount, 0f, BaseValue); }

		public void SetImmediate(float amount) { CurrentValue = Mathf.Max(0f, amount); }

		public float NormalizedAmount(float amount) { return amount / BaseValue; }
	}
}
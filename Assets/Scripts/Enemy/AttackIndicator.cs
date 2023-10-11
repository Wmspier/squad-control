using System;
using UnityEngine;

namespace Enemy
{
	public abstract class AttackIndicator : MonoBehaviour
	{
		public Action IndicationComplete;

		public abstract void Run(float durationSeconds);
	}
}
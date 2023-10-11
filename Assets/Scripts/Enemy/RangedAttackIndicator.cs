using DG.Tweening;
using UnityEngine;

namespace Enemy
{
	public class RangedAttackIndicator : AttackIndicator
	{
		[SerializeField] private Transform _fill;

		public override void Run(float durationSeconds)
		{
			_fill.DOScale(Vector3.one, durationSeconds).OnComplete(() =>
			{
				_fill.localScale = Vector3.zero;
				if (!gameObject.activeSelf) return;
				IndicationComplete?.Invoke();
			});
		}
	}
}
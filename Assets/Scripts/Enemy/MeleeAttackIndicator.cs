using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Enemy
{
	public class MeleeAttackIndicator : AttackIndicator
	{
		[SerializeField] private Image _fillImage;

		public override void Run(float durationSeconds)
		{
			_fillImage.fillAmount = 0f;

			_fillImage.DOFillAmount(1f, durationSeconds).OnComplete(() =>
			{
				if (!gameObject.activeSelf) return;
				IndicationComplete?.Invoke();
			});
		}
	}
}
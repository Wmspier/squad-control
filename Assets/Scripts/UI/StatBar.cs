using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class StatBar : MonoBehaviour
	{
		[SerializeField] private Image fill;
		[SerializeField] private GameObject anchor;
		[SerializeField] private float minimizeAfterSeconds = 5f;
		[SerializeField] private bool alwaysVisible;

		private float _previousDisplayTime;
		private bool _visible;
		private Sequence _flashSequence;

		private void Start() { _visible = anchor.activeSelf; }

		private void Update()
		{
			if (alwaysVisible) return;

			if (_visible && Time.time - _previousDisplayTime >= minimizeAfterSeconds) Toggle(false);
		}

		public void Toggle(bool visible)
		{
			if (visible)
			{
				anchor.SetActive(true);
				anchor.transform.localScale = Vector3.zero;
				var sequence = DOTween.Sequence()
					.Append(anchor.transform.DOScale(Vector3.one, .25f))
					.Append(anchor.transform.DOPunchScale(new Vector3(.5f, .5f, .5f), .25f));

				sequence.Play();
				_previousDisplayTime = Time.time;
				_visible = true;
			}
			else
			{
				anchor.transform.localScale = Vector3.one;
				var sequence = DOTween.Sequence()
					.Append(anchor.transform.DOScale(Vector3.zero, .25f))
					.AppendCallback(() => { anchor.SetActive(false); });

				sequence.Play();
				_visible = false;
			}
		}

		public void Modify(float amountNormalized, bool doShake = false)
		{
			if (doShake)
				transform.DOShakePosition(
					.5f,
					.25f,
					100);

			fill.fillAmount += amountNormalized;
		}

		public void Punch() { transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0.25f), .25f); }

		public void Flash(Color color)
		{
			if (_flashSequence != null && _flashSequence.IsPlaying()) return;
			
			var originalColor = fill.color;
			_flashSequence = DOTween.Sequence();
			_flashSequence.Append(fill.DOColor(color, .25f))
				.Append(fill.DOColor(originalColor, .25f))
				.Play();
		}

		public void SetImmediate(float amountNormalized) { fill.fillAmount = amountNormalized; }
	}
}
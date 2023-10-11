using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
	public class TestPanel : MonoBehaviour
	{
		[SerializeField] private Button _backgroundClose;
		[SerializeField] private float _height;
		[SerializeField] private Button _toggle;

		private bool _visible = true;

		private void Start()
		{
			_toggle.onClick.AddListener(OnToggle);
			_backgroundClose.onClick.AddListener(OnToggle);
			OnToggle();
		}

		private void OnToggle()
		{
			if (_visible)
			{
				GetComponent<RectTransform>().DOAnchorPosY(-_height, .25f).SetEase(Ease.OutSine);
				_backgroundClose.gameObject.SetActive(false);
			}
			else
			{
				GetComponent<RectTransform>().DOAnchorPosY(0, .25f).SetEase(Ease.InSine);
				_backgroundClose.gameObject.SetActive(true);
			}

			_visible = !_visible;
		}
	}
}
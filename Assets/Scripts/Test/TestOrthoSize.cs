using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
	public class TestOrthoSize : MonoBehaviour
	{
		[SerializeField] private TMP_Text _text;
		[SerializeField] private Slider _slider;
		[SerializeField] private CinemachineVirtualCamera _virtualCamera;

		private void Awake()
		{
			_slider.onValueChanged.AddListener(OnSliderChanged);
		}

		private void OnSliderChanged(float value)
		{
			_virtualCamera.m_Lens.OrthographicSize = value;
			_text.text = $"Ortho Size: {value}";
		}
	}
}
using System;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
	public class TestCameraAngle : MonoBehaviour
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
			value = (float)Math.Round(value, 1);
			var rotationEuler = _virtualCamera.transform.rotation.eulerAngles;
			rotationEuler.x = value;
			_virtualCamera.transform.rotation = Quaternion.Euler(rotationEuler);
			_text.text = $"Camera Angle: {value}";
		}
	}
}
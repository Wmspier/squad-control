using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
	public class TestSquadZoneScale : MonoBehaviour
	{
		[SerializeField] private TMP_Text _innerZoneText;
		[SerializeField] private Slider _innerZoneSlider;
		
		[SerializeField] private TMP_Text _outerZoneText;
		[SerializeField] private Slider _outerZoneSlider;
		
		[SerializeField] private Squad.Squad _squad;

		private void Awake()
		{
			_innerZoneSlider.onValueChanged.AddListener(OnInnerSliderChanged);
			_outerZoneSlider.onValueChanged.AddListener(OnOuterSliderChanged);
			if (StaticTestData.Instance.SquadInnerZoneScale != 0f)
			{
				OnInnerSliderChanged(StaticTestData.Instance.SquadInnerZoneScale);
				_innerZoneSlider.value = StaticTestData.Instance.SquadInnerZoneScale;
			}
			if (StaticTestData.Instance.SquadOuterZoneScale != 0f)
			{
				OnOuterSliderChanged(StaticTestData.Instance.SquadOuterZoneScale);
				_outerZoneSlider.value = StaticTestData.Instance.SquadOuterZoneScale;
			}
		}

		private void OnInnerSliderChanged(float value)
		{
			_squad.SetInnerZoneScale(value);
			_innerZoneText.text = $"Inner Zone Scale: {value}";
			StaticTestData.Instance.SquadInnerZoneScale = value;
		}
		
		private void OnOuterSliderChanged(float value)
		{
			_squad.SetOuterZoneScaleRelative(value);
			_outerZoneText.text = $"Outer Zone Relative Scale: {Math.Round(value, 2)}";
			StaticTestData.Instance.SquadOuterZoneScale = value;
		}
	}
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
	public class TestSquadConfiguration : MonoBehaviour
	{
		[SerializeField] private Squad.Squad _squad;
		
		// Member Scale
		[SerializeField] private TMP_Text _memberScaleText;
		[SerializeField] private Slider _memberScaleSlider;
		
		// Member Spacing
		[SerializeField] private TMP_Text _memberSpacingText;
		[SerializeField] private Slider _memberSpacingSlider;
		
		// Add/Remove Member
		[SerializeField] private Button _addMemberButton;
		[SerializeField] private Button _removeMemberButton;
		
		// Stop to interact
		[SerializeField] private Toggle _stopToInteractToggle;
		
		// Squad Leader
		[SerializeField] private Toggle _squadLeaderToggle;
		
		private void Awake()
		{
			_memberScaleSlider.onValueChanged.AddListener(OnMemberScaleSliderChanged);
			_memberSpacingSlider.onValueChanged.AddListener(OnMemberSpacingSliderChanged);
			
			_addMemberButton.onClick.AddListener(OnAddMember);
			_removeMemberButton.onClick.AddListener(OnRemoveMember);
			_stopToInteractToggle.onValueChanged.AddListener(OnStopToInteractToggle); 
			_squadLeaderToggle.onValueChanged.AddListener(OnSquadLeaderToggle);

			if (StaticTestData.Instance.SquadMemberScale != 0f)
			{
				_memberScaleSlider.value = StaticTestData.Instance.SquadMemberScale;
				OnMemberScaleSliderChanged(_memberScaleSlider.value);
			}
			if (StaticTestData.Instance.SquadMemberSpacing != 0f)
			{
				_memberSpacingSlider.value = StaticTestData.Instance.SquadMemberSpacing;
				OnMemberSpacingSliderChanged(_memberSpacingSlider.value);
			}
			_stopToInteractToggle.isOn = StaticTestData.Instance.StopToInteract;
			_squadLeaderToggle.isOn = StaticTestData.Instance.SquadLeader;
		}

		private void OnMemberScaleSliderChanged(float value)
		{
			value = (float)Math.Round(value, 1);
			StaticTestData.Instance.SquadMemberScale = value;
			_squad.ResetMemberScales();
			
			_memberScaleText.text = $"Member Size: {value}";
		}

		private void OnMemberSpacingSliderChanged(float value)
		{
			value = (float)Math.Round(value, 1);
			StaticTestData.Instance.SquadMemberSpacing = value;
			_squad.SetMemberDistance(value);
			_memberSpacingText.text = $"Member Spacing: {value}";
		}
		
		private void OnAddMember() => _squad.AddSquadMember();
		private void OnRemoveMember() => _squad.RemoveSquadMember();
		
		private static void OnStopToInteractToggle(bool value) => StaticTestData.Instance.StopToInteract = value;

		private void OnSquadLeaderToggle(bool value)
		{
			StaticTestData.Instance.SquadLeader = value;
			_squad.ResetMemberTargetPositions();
			_squad.ResetMemberScales();
		}
	}
}
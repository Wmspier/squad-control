using System.Collections.Generic;
using Movement;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
	public class TestRotateFormation : MonoBehaviour
	{
		[SerializeField] private Toggle _toggle;
		[SerializeField] private FixedCharacterMovement _groupMovement;
		[SerializeField] private List<FixedCharacterMovement> _squadMovements;

		private List<Quaternion> _squadStartingRotations = new();
		private List<Vector3> _squadStartingPositions = new();
		
		private void Awake()
		{
			_toggle.onValueChanged.AddListener(OnToggle);

			foreach (var movement in _squadMovements)
			{
				_squadStartingPositions.Add(movement.transform.localPosition);
				_squadStartingRotations.Add(movement.transform.localRotation);
			}
		}

		private void OnToggle(bool toggled)
		{
			for (var i = 0; i < _squadMovements.Count; i++)
			{
				_squadMovements[i].transform.localPosition = _squadStartingPositions[i];
				_squadMovements[i].transform.localRotation = _squadStartingRotations[i];
			}
			
			if (toggled)
			{
				_groupMovement.SetHandleRotation(true);
				foreach (var movement in _squadMovements)
				{
					movement.enabled = false;
					movement.SetHandleRotation(false);
				}
			}
			else
			{
				_groupMovement.SetHandleRotation(false);
				foreach (var movement in _squadMovements)
				{
					movement.enabled = true;
					movement.SetHandleRotation(true);
				}
			}
		}
	}
}
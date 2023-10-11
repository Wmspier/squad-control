using System.Collections.Generic;
using Movement;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
	public class TestJoystick : MonoBehaviour
	{
		[SerializeField] private Toggle _toggle;
		[SerializeField] private VirtualJoystick _dynamicJoystick;
		[SerializeField] private VirtualJoystick _fixedJoystick;
		[SerializeField] private List<FixedCharacterMovement> _fixedMovements;

		private void Awake()
		{
			_toggle.onValueChanged.AddListener(OnToggle);
		}

		private void OnToggle(bool dynamic)
		{
			foreach (var movement in _fixedMovements)
			{
				movement.SetJoystick(dynamic ? _dynamicJoystick : _fixedJoystick);
			}
			_dynamicJoystick.gameObject.SetActive(dynamic);
			_fixedJoystick.gameObject.SetActive(!dynamic);
		}
	}
}
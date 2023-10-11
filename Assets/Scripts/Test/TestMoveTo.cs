using UnityEngine;

namespace Test
{
	public class TestMoveTo : MonoBehaviour
	{
		[SerializeField] private Squad.Squad _squad;
		[SerializeField] private GameObject _waypoint;

		private Vector3 _lastWaypointPosition = Vector3.negativeInfinity;
		
		private void Start() { } // Dummy start to allow disable
		
		private void Awake()
		{
			GlobalTouchHandler.Instance.TapEvent += OnTap;
		}


		private void OnTap(Vector2 screenPosition)
		{
			if(!enabled) return;
			
			var positionNullable = GroundCollisionHandler.Instance.GetGroundCollisionFromScreenSpaceNullable(screenPosition);
			if (positionNullable.HasValue)
			{
				_lastWaypointPosition = positionNullable.Value;

				_squad.SetMembersTargetClamped(_lastWaypointPosition);
				_waypoint.SetActive(true);
				_waypoint.transform.position = _lastWaypointPosition;
			}
		}
	}
}
using UnityEngine;

namespace UI
{
	public class DynamicVirtualJoystick : VirtualJoystick
	{
		[SerializeField] protected float durationBeforeOriginResetSeconds;
		
		private Vector2? _cachedStartPosition;
		
		private void Start()
		{
			var touchHandler = GlobalTouchHandler.Instance;
			touchHandler.TouchStartEvent += OnTouchStart;
			touchHandler.TouchEndEvent += OnTouchEnd;
			touchHandler.TouchMovedEvent += OnTouchMove;
		}

		private void OnDestroy()
		{
			var touchHandler = GlobalTouchHandler.Instance;
			touchHandler.TouchStartEvent -= OnTouchStart;
			touchHandler.TouchEndEvent -= OnTouchEnd;
			touchHandler.TouchMovedEvent -= OnTouchMove;
		}

		protected override void OnTouchStart(Vector2 position)
		{
			if (Time.time - LastTouchEndTime <= durationBeforeOriginResetSeconds)
				centerCircle.position = _cachedStartPosition ?? position;
			else
				centerCircle.position = position;

			_cachedStartPosition = centerCircle.position;

			if (Time.time - LastTouchEndTime <= tapWindowSeconds) TriggerTap();
		}

		protected override void OnTouchEnd(Vector2 position)
		{
			ToggleCircles(false);
			Direction = null;
			LastTouchEndTime = Time.time;
		}

		protected override void OnTouchMove(Vector2 newPosition)
		{
			ToggleCircles(true);

			directionCircle.position = newPosition;
			Direction = directionCircle.anchoredPosition - centerCircle.anchoredPosition;
		}
	}
}
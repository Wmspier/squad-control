using System;
using UnityEngine;

namespace UI
{
	public class VirtualJoystick : MonoBehaviour
	{
		[SerializeField] protected RectTransform centerCircle;
		[SerializeField] protected RectTransform directionCircle;
		[SerializeField] protected float tapWindowSeconds;
		
		protected float LastTouchEndTime;

		public Vector2? Direction { get; protected set; }

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

		// Triggers on TouchStart very shortly after TouchEnd
		public event Action TapEvent;

		protected void ToggleCircles(bool visible)
		{
			centerCircle.gameObject.SetActive(visible);
			directionCircle.gameObject.SetActive(visible);
		}

		protected virtual void OnTouchStart(Vector2 position) { }

		protected virtual void OnTouchEnd(Vector2 position) { }

		protected virtual void OnTouchMove(Vector2 newPosition) { }

		protected void TriggerTap()
		{
			TapEvent?.Invoke();
		}
	}
}
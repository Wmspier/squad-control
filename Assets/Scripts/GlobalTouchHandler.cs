using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GlobalTouchHandler : MonoBehaviour
{
	public static GlobalTouchHandler Instance;

	[SerializeField] private Camera mainCamera;
	[SerializeField] private float maxTimeForTapSeconds = .25f;
	[SerializeField] private float minDistanceForDrag = .1f;

	private readonly RaycastHit[] _raycastAlloc = new RaycastHit[5];
	private bool _dragging;
	private float _touchDownTime;

	private Vector2 _touchStartPosition;

	private void Awake() { Instance = this; }

	private void Update()
	{
		switch (Input.touchCount)
		{
			case 0:
			{
				return;
			}
			case 1:
			{
				if (IsPointerOverUIElement(GetEventSystemRaycastResults()))
				{
					return;
				}

				break;
			}
			case > 1:
				// Only handle single touch for now
				return;
		}

		var touch0 = Input.GetTouch(0);

		switch (touch0.phase)
		{
			case TouchPhase.Began:
				HandleTouchDown(touch0);
				break;
			case TouchPhase.Moved:
				HandleTouchMoved(touch0);
				break;
			case TouchPhase.Stationary:
				HandleTouchMoved(touch0);
				break;
			case TouchPhase.Ended:
			case TouchPhase.Canceled:
				HandleTouchUp(touch0);
				break;
		}
	}

	public event Action<RaycastHit[], int> RayCastHit;
	public event Action<Vector2> TouchEndEvent;
	public event Action<Vector2> TouchStartEvent;
	public event Action<Vector2> TouchMovedEvent;
	public event Action<Vector2> TapEvent;

	private void HandleTouchDown(Touch touch)
	{
		DoRaycast(touch.position);
		_touchDownTime = Time.time;
		_touchStartPosition = touch.position;
		TouchStartEvent?.Invoke(touch.position);
	}

	private void HandleTouchMoved(Touch touch)
	{
		DoRaycast(touch.position);
		if ((touch.position - _touchStartPosition).magnitude >= minDistanceForDrag) _dragging = true;

		TouchMovedEvent?.Invoke(touch.position);
	}

	private void HandleTouchUp(Touch touch)
	{
		TouchEndEvent?.Invoke(touch.position);
		_dragging = false;

		var touchDuration = Time.time - _touchDownTime;
		if (touchDuration <= maxTimeForTapSeconds && !_dragging) TapEvent?.Invoke(touch.position);
	}

	private void DoRaycast(Vector2 screenPoint)
	{
		var ray = mainCamera.ScreenPointToRay(screenPoint);
		var hitCount = Physics.RaycastNonAlloc(ray, _raycastAlloc);

		if (hitCount == 0) return;

		RayCastHit?.Invoke(_raycastAlloc, hitCount);
	}
	
	private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)
	{
		foreach (var curRaycastResult in eventSystemRaycastResults)
		{
			if (curRaycastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
				return true;
		}

		return false;
	}

	private static List<RaycastResult> GetEventSystemRaycastResults()
	{
		var eventData = new PointerEventData(EventSystem.current)
		{
			position = Input.mousePosition
		};
		var raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, raycastResults);
		return raycastResults;
	}
}
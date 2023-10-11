using System;
using UnityEngine;

namespace UI
{
	public class FixedJoystick : VirtualJoystick
	{
		[SerializeField] private float maxDistanceBetweenCircles;
		
		protected override void OnTouchStart(Vector2 position)
		{
			LastTouchEndTime = Time.time;
		}

		protected override void OnTouchEnd(Vector2 position)
		{
			if (Time.time - LastTouchEndTime <= tapWindowSeconds)
			{
				TriggerTap();
			}
		}

		protected override void OnTouchMove(Vector2 newPosition)
		{
			var centerPosition = centerCircle.position;
			var diff = new Vector3(newPosition.x, newPosition.y) - centerPosition;
			
			var offset2D = diff.normalized * Mathf.Min(diff.magnitude, maxDistanceBetweenCircles);
			var clampedPosition = centerPosition;
			clampedPosition.x += offset2D.x;
			clampedPosition.y += offset2D.y;
			
			directionCircle.position = clampedPosition;
			Direction = directionCircle.anchoredPosition - centerCircle.anchoredPosition;
			
			HandleRotation();
		}

		private void HandleRotation()
		{
			var dir = directionCircle.position - centerCircle.position;
			var a = dir.y;
			var o = dir.x;
			var h = dir.magnitude;
			var theta = Mathf.Acos(a / h) * 180 / Mathf.PI;
			directionCircle.rotation = Quaternion.Euler(new Vector3(0,0,180 - Mathf.Abs(theta) * (o < 0 ? -1 : 1)));
		}
	}
}
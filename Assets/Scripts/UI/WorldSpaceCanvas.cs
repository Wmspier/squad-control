using UnityEngine;

namespace UI
{
	[ExecuteInEditMode]
	public class WorldSpaceCanvas : MonoBehaviour
	{
		[SerializeField] private Vector3 positionOffset;
		[SerializeField] private Vector3 rotation;
		
		private void Update() { UpdateRotationPosition(); }

		private void UpdateRotationPosition()
		{
			var t = transform;

			var pos = t.parent.position + positionOffset;
			t.position = pos;

			t.rotation = Quaternion.Euler(rotation);
		}
	}
}
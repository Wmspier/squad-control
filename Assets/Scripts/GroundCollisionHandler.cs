using UnityEngine;

public class GroundCollisionHandler : MonoBehaviour
{
	[SerializeField] private Camera _mainCamera;
	[SerializeField] private string _groundTag;

	private RaycastHit[] _rayCastResults = new RaycastHit[10];
	
	public static GroundCollisionHandler Instance;

	private void Awake()
	{
		Instance = this;
	}

	public Vector3? GetGroundCollisionFromScreenSpaceNullable(Vector2 screenPos)
	{
		var ray = _mainCamera.ScreenPointToRay(screenPos);
		var hits = Physics.RaycastNonAlloc(ray, _rayCastResults);
		for (var i = hits-1; i >= 0; i--)
		{
			var hit = _rayCastResults[i];
			if (hit.collider.gameObject.tag.Equals(_groundTag))
			{
				return hit.point;
			}
		}
		
		return null;
	}
}
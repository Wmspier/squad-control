using UnityEditor;
using UnityEngine;

namespace Spawning
{
	public class RadialEnemySpawner : EnemySpawner
	{
		[SerializeField] private Transform target;
		[SerializeField] private float radius;

		private void OnDrawGizmos()
		{
			if (!showGizmos) return;
		#if UNITY_EDITOR
			Handles.color = Color.red;
			var center = target != null ? target.transform.position : transform.position;
			Handles.DrawWireDisc(center, Vector3.up, radius, 2f);
		#endif
		}

		protected override Vector3? FindSpawnLocation()
		{
			var randomRadius = radius * Mathf.Sqrt(Random.value);
			var theta = Random.value * Mathf.PI * 2;
			var center = target != null ? target.transform.position : transform.position;
			var x = center.x + randomRadius * Mathf.Cos(theta);
			var z = center.z + randomRadius * Mathf.Sin(theta);

			var position = transform.position;

			var maxAttempts = 50;
			do
			{
				var ray = new Ray(new Vector3(position.x + x, 100f, position.z + z), Vector3.down);
				var count = Physics.RaycastNonAlloc(ray, RaycastAlloc, float.MaxValue);
				var hitObstacle = false;
				Vector3 hitLocation = default;

				for (var i = 0; i < count; i++)
				{
					var hit = RaycastAlloc[i];
					if (hit.collider.gameObject.CompareTag("Obstacle"))
						hitObstacle = true;
					else if (hit.collider.gameObject.CompareTag("Ground")) hitLocation = hit.point;
				}

				maxAttempts--;
				if (hitObstacle) continue;
				if (hitLocation == default) continue;

				return hitLocation;
			} while (maxAttempts > 0);

			return null;
		}
	}
}
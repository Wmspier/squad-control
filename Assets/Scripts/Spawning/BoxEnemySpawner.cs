using UnityEngine;

namespace Spawning
{
	public class BoxEnemySpawner : EnemySpawner
	{
		[SerializeField] [Min(1f)] private float spawnRegionX;
		[SerializeField] [Min(1f)] private float spawnRegionZ;

		private void OnDrawGizmos()
		{
			if (!showGizmos) return;
			
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(transform.position, new Vector3(spawnRegionX, 1f, spawnRegionZ));
		}

		protected override Vector3? FindSpawnLocation()
		{
			var x = Random.Range(-spawnRegionX / 2, spawnRegionX / 2);
			var z = Random.Range(-spawnRegionZ / 2, spawnRegionZ / 2);
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
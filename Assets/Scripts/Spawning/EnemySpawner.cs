using System.Collections.Generic;
using CharacterBehaviors;
using Enemy;
using UnityEngine;

namespace Spawning
{
	public abstract class EnemySpawner : MonoBehaviour
	{
		[SerializeField] private BasicEnemyController enemyPrefab;
		[SerializeField] private float spawnFrequencySeconds = 5f;
		[SerializeField] private Transform enemyRoot;
		[SerializeField] private int maxConcurrentEnemies = 25;
		[SerializeField] private int maxConcurrentDeadEnemies = 15;
		
		[SerializeField] private List<GameObject> collisionBlacklist;

		[Header("Debug")]
		[SerializeField] protected bool showGizmos;

		protected readonly RaycastHit[] RaycastAlloc = new RaycastHit[5];
		private List<BasicEnemyController> _activeEnemies;
		private List<BasicEnemyController> _deadEnemies;
		private float _previousSpawnTime;

		private void Awake()
		{
			_activeEnemies = new List<BasicEnemyController>(maxConcurrentEnemies);
			_deadEnemies = new List<BasicEnemyController>(maxConcurrentDeadEnemies);
		}

		private void Start() { _activeEnemies.AddRange(enemyRoot.GetComponentsInChildren<BasicEnemyController>()); }

		private void Update()
		{
			if (Time.time - _previousSpawnTime < spawnFrequencySeconds ||
			    _activeEnemies.Count >= maxConcurrentEnemies) return;

			var spawnLocation = FindSpawnLocation();
			if (spawnLocation.HasValue) InstantiateEnemy(spawnLocation.Value);
		}

		protected virtual Vector3? FindSpawnLocation() { return null; }

		private void InstantiateEnemy(Vector3 hitLocation)
		{
			var newEnemy = Instantiate(enemyPrefab, enemyRoot);
			var isMelee = Random.Range(0, 2) == 0;
			
			newEnemy.transform.position = hitLocation;
			newEnemy.Died += OnEnemyDied;
			newEnemy.AbilityActivator.Initialize(collisionBlacklist);
			newEnemy.SetIsMelee(isMelee);
			newEnemy.Type.SetType(isMelee ? CharacterTypeEnum.EnemyMelee : CharacterTypeEnum.EnemyRanged);

			_activeEnemies.Add(newEnemy);
			_previousSpawnTime = Time.time;
		}

		private void OnEnemyDied(BasicEnemyController enemy)
		{
			var index = _activeEnemies.IndexOf(enemy);
			_activeEnemies.RemoveAt(index);
			_deadEnemies.Add(enemy);

			if (_deadEnemies.Count <= maxConcurrentDeadEnemies) return;

			var toRemove = _deadEnemies[0];
			toRemove.Died -= OnEnemyDied;
			_deadEnemies.RemoveAt(0);
			Destroy(toRemove.gameObject);
		}
	}
}
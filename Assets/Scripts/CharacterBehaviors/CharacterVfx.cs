using System;
using System.Collections.Generic;
using CharacterBehaviors.Abilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CharacterBehaviors
{
	public class CharacterVfx : MonoBehaviour
	{
		[SerializeField] private AnimationEventListener eventListener;
		[SerializeField] private Transform vfxRoot;

		private readonly Dictionary<string, VfxPayload> _vfxPrefabRegistryByEffectId = new(10);
		private readonly Dictionary<string, List<(GameObject instance, float spawnTime, float? lifetimeNullable, bool isProjectle)>> _vfxRegistryByEffectId = new(10);
		private readonly Dictionary<GameObject, string> _vfxRegistryByInstance = new(10);

		private List<GameObject> _collisionBlacklist;

		private Vector3 _projectileDirection;
		private Vector3 _projectileDestination;
		
		public Transform WorldVfxRoot { get; private set; }

		private void Start()
		{
			eventListener.EventTriggered += OnAnimationEventTriggered;
			WorldVfxRoot = GameObject.FindWithTag("VFX_Root").transform;

			_projectileDirection = transform.forward;
		}

		public void Initialize(List<GameObject> collisionBlacklist)
		{
			_collisionBlacklist = collisionBlacklist;
		}

		private void OnDestroy() { eventListener.EventTriggered -= OnAnimationEventTriggered; }

		public void SetProjectileDirection(Vector3 direction) { _projectileDirection = direction; }

		public void SetProjectileDestination(Vector3 destination) { _projectileDestination = destination; }
		
		// Collider of the object the vfx hit, effect id and a hash of effect Id and effect instance Id
		public event Action<Collider, string, int> CollisionTriggerEnter;

		// hash of effect Id and effect instance Id
		public event Action<int> EffectExpired;

		// hash of effect Id and effect instance Id
		public event Action<int> EffectCreated;

		public void RegisterVfxPrefab(VfxPayload payload) => _vfxPrefabRegistryByEffectId[payload.id] = payload;
		
		private void OnAnimationEventTriggered(string animationTag)
		{
			var splitTag = animationTag.Split('|');
			switch (splitTag[0])
			{
				case "create":
					OnCreateEffect(splitTag[1]);
					return;
				case "destroy":
					//OnDestroyEffect(splitTag[1]);
					return;
			}
		}

		private void Update()
		{
			foreach (var (effectId, entry) in _vfxRegistryByEffectId)
			{
				var toRemove = new List<(GameObject instance, float spawnTime, float? lifetimeNullable, bool isProjectile)>();
				foreach (var group in entry)
				{
					// vfx does not have a lifetime
					if (!group.lifetimeNullable.HasValue) continue;
					
					// vfx has not reached end of lifetime
					if (!(Time.time - group.spawnTime >= group.lifetimeNullable)) continue;
					
					EffectExpired?.Invoke(GetUniqueEffectId(effectId, group.instance));
					Destroy(group.instance);
					toRemove.Add(group);
				}

				foreach (var r in toRemove)
				{
					entry.Remove(r);
				}
			}
		}

		private void OnCreateEffect(string effectId)
		{
			if (!_vfxPrefabRegistryByEffectId.TryGetValue(effectId, out var payload)) return;
			if (payload.vfx == null) return;

			// Create vfx instance
			var vfxInstance = Instantiate(payload.vfx, payload.worldPosition ? WorldVfxRoot : vfxRoot);
			if (payload.worldPosition) vfxInstance.transform.position = transform.position;

			// Register the collision event with the relay
			var relay = vfxInstance.GetComponentInChildren<CollisionEventRelay>();
			relay.Caster = gameObject;
			if (relay != null) relay.TriggerEnter += c => OnCollisionTriggerEntered(c, vfxInstance);

			// Register the instance in the effect registry, or update an existing entry
			if (_vfxRegistryByEffectId.TryGetValue(effectId, out var effectList))
			{
				effectList.Add((vfxInstance, Time.time, payload.hasLifetime ? payload.lifetime : null, payload.IsProjectile()));
			}
			else
			{
				_vfxRegistryByEffectId[effectId] = new List<(GameObject instance, float spawnTime, float? lifetimeNullable, bool isProjectile)>
				{
					(vfxInstance, Time.time, payload.hasLifetime ? payload.lifetime : null, payload.IsProjectile())
				};
			}

			// Launch if effect is projectile
			if (payload.IsProjectile())
			{
				var projectileComponent = vfxInstance.GetComponent<Projectile>();
				if (payload.abilityType == AbilityType.Bullet)
				{
					projectileComponent.LaunchBullet(payload.speed, _projectileDirection);
				} 
				else if (payload.abilityType == AbilityType.Volley)
				{
					projectileComponent.LaunchVolley(payload.speed, _projectileDestination);
				}
			}

			// Add instance to instance registry
			_vfxRegistryByInstance[vfxInstance] = effectId;
			
			EffectCreated?.Invoke(GetUniqueEffectId(effectId, vfxInstance));
		}

		private void OnCollisionTriggerEntered(Collider other, GameObject vfxInstance)
		{
			if (_collisionBlacklist.Contains(other.gameObject)) return;
			
			var effectId = _vfxRegistryByInstance[vfxInstance];

			CollisionTriggerEnter?.Invoke(other, effectId, GetUniqueEffectId(effectId, vfxInstance));

			var payload = _vfxPrefabRegistryByEffectId[effectId];
			if (!payload.IsProjectile()) return;
			
			// If the effect is a projectile, remove it from the registry and destroy it
			var entry = _vfxRegistryByEffectId[effectId];
			var toRemove = new List<(GameObject instance, float spawnTime, float? lifetimeNullable, bool isProjectile)>();
			foreach (var group in entry)
			{
				// vfx does not have a lifetime
				if (group.instance != vfxInstance) continue;

				EffectExpired?.Invoke(GetUniqueEffectId(effectId, group.instance));
				Destroy(group.instance);
				toRemove.Add(group);
			}

			foreach (var r in toRemove)
			{
				entry.Remove(r);
			}
		}

		private static int GetUniqueEffectId(string effectId, Object effectInstance)
		{
			return string.Concat(effectId, effectInstance.GetInstanceID()).GetHashCode();
		}

		[Serializable]
		public struct VfxPayload
		{
			public string id;
			public GameObject vfx;
			public bool worldPosition;
			public bool hasLifetime;
			public float lifetime;
			public AbilityType abilityType;
			public float speed;

			public bool IsProjectile() => abilityType is AbilityType.Bullet or AbilityType.Volley;
		}

		public enum AbilityType
		{
			Melee,
			Bullet,
			Volley
		}
	}
}
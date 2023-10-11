using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterBehaviors
{
	[Serializable]
	public class CharacterCollisionLayer : IDisposable
	{
		[SerializeField] private float minimumCollisionThresholdSeconds = .5f;

		private readonly Dictionary<int, Dictionary<int, float>> _effectCollisionRegistry = new(10);

		private CharacterVfx _characterVfx;
		private GameObject _owner;
		private List<GameObject> _collisionBlacklist;

		public event Action<GameObject, string> AbilityHitTarget;

		public CharacterCollisionLayer(GameObject owner, CharacterVfx vfxComponent, List<GameObject> collisionBlacklist)
		{
			_owner = owner;
			_characterVfx = vfxComponent;
			_collisionBlacklist = collisionBlacklist;

			_characterVfx.EffectCreated += OnEffectCreated;
			_characterVfx.EffectExpired += OnEffectExpired;
			_characterVfx.CollisionTriggerEnter += OnEffectCollision;
		}

		public void Dispose()
		{
			_characterVfx.EffectCreated -= OnEffectCreated;
			_characterVfx.EffectExpired -= OnEffectExpired;
			_characterVfx.CollisionTriggerEnter -= OnEffectCollision;
		}

		private void OnEffectCreated(int effectHash)
		{
			_effectCollisionRegistry[effectHash] = new Dictionary<int, float>();
		}

		private void OnEffectExpired(int effectHash) { _effectCollisionRegistry.Remove(effectHash); }

		private void OnEffectCollision(Collider other, string effectId, int effectHash)
		{
			// Ignore this collision, as the collider is blacklisted
			if (_collisionBlacklist.Contains(other.gameObject)) return;
			
			var otherInstance = other.gameObject.GetInstanceID();

			// Effect has expired and will be removed
			if (!_effectCollisionRegistry.ContainsKey(effectHash)) return;

			// A collision with this object has already happened and we're avoiding registering it too often
			var collisionHistory = _effectCollisionRegistry[effectHash];
			if (collisionHistory.TryGetValue(otherInstance, out var lastCollision)
			    && Time.time - lastCollision < minimumCollisionThresholdSeconds) return;
			
			collisionHistory[otherInstance] = Time.time;
			AbilityHitTarget?.Invoke(other.gameObject, effectId);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using Status;
using UnityEngine;

namespace Squad
{
	public class SquadZoneGroup : MonoBehaviour
	{
		[field: SerializeField] public SquadZone InnerZone { get; private set; }
		[field: SerializeField] public SquadZone OuterZone { get; private set; }
		
		public float InnerZoneScale { get; private set; }
		public float OuterZoneScaleRelative { get; private set; }
		
		public event Action<GameObject, bool> InteractableEnteredZone;
		public event Action<bool> AllInteractablesExitedZone;
		public event Action<GameObject, bool> EnemyEnteredZone;
		public event Action<GameObject, bool> EnemyExitedZone;
		
		public bool IsInteractableInZone(bool isInner) => (isInner ? InnerZone : OuterZone).Interactables.Count > 0;
		public IReadOnlyList<GameObject> Interactables(bool isInner) => (isInner ? InnerZone : OuterZone).Interactables;
		public IReadOnlyList<GameObject> Enemies(bool isInner) => (isInner ? InnerZone : OuterZone).Enemies;

		private void Awake()
		{
			InnerZone.InteractableEnteredZone += go => { InteractableEnteredZone?.Invoke(go, true); };
			InnerZone.AllInteractablesExitedZone += () => AllInteractablesExitedZone?.Invoke(true);
			InnerZone.EnemyEnteredZone += go => { EnemyEnteredZone?.Invoke(go, true); };
			InnerZone.EnemyExitedZone += go => { EnemyExitedZone?.Invoke(go, true); };
			
			OuterZone.InteractableEnteredZone += go => { InteractableEnteredZone?.Invoke(go, false); };
			OuterZone.AllInteractablesExitedZone += () => AllInteractablesExitedZone?.Invoke(false);
			OuterZone.EnemyEnteredZone += go => { EnemyEnteredZone?.Invoke(go, false); };
			OuterZone.EnemyExitedZone += go => { EnemyExitedZone?.Invoke(go, false); };
		}

		public void SetInnerZoneScale(float scale)
		{
			InnerZoneScale = scale;
			var outerZoneScale = InnerZoneScale * OuterZoneScaleRelative;

			InnerZone.transform.localScale = new Vector3(InnerZoneScale, InnerZoneScale, InnerZoneScale);
			OuterZone.transform.localScale = new Vector3(outerZoneScale, outerZoneScale, outerZoneScale);
		}

		public void SetOuterZoneScaleRelative(float scaleRelative)
		{
			OuterZoneScaleRelative = scaleRelative;
			var outerZoneScale = InnerZoneScale * OuterZoneScaleRelative;
			OuterZone.transform.localScale = new Vector3(outerZoneScale, outerZoneScale, outerZoneScale);
		}
		
		public void ToggleColor(bool isMoving, bool isAttacking, bool isInner = true) => (isInner ? InnerZone : OuterZone).ToggleColor(isMoving, isAttacking);

		public CharacterStatus GetFirstLivingEnemy(bool isInner)
		{
			return Enemies(isInner)
				.FirstOrDefault(e => e != null && e.TryGetComponent(typeof(CharacterStatus), out var status) && ((CharacterStatus)status).IsAlive)
				?.GetComponent<CharacterStatus>();
		}

		public List<CharacterStatus> GetLivingEnemies(bool isInner)
		{
			return Enemies(isInner)
				.Where(e => e != null && e.TryGetComponent(typeof(CharacterStatus), out var status) && ((CharacterStatus)status).IsAlive)
				.Select(e => e.GetComponent<CharacterStatus>())
				.ToList();
		}

	}
}
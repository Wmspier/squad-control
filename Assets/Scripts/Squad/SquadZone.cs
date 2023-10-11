using System;
using System.Collections.Generic;
using UnityEngine;

namespace Squad
{
	public class SquadZone : MonoBehaviour
	{
		[SerializeField] private Color stationaryColor;
		[SerializeField] private Color movingColor;
		[SerializeField] private Color attackingColor;
		[SerializeField] private Material primaryMaterial;
		
		private readonly List<GameObject> _interactables = new();
		private readonly List<GameObject> _enemies = new();
		private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

		public event Action<GameObject> InteractableEnteredZone;
		public event Action AllInteractablesExitedZone;

		public event Action<GameObject> EnemyEnteredZone;
		public event Action<GameObject> EnemyExitedZone;

		public bool IsInteractableInZone => _interactables.Count > 0;
		public IReadOnlyList<GameObject> Interactables => _interactables;
		public IReadOnlyList<GameObject> Enemies => _enemies;

		private void OnValidate()
		{
			primaryMaterial.color = stationaryColor;
			primaryMaterial.SetColor(EmissionColor, stationaryColor * 5);
		}

		public void ToggleColor(bool isMoving, bool isAttacking)
		{
			// Moving color has priority over attack color
			primaryMaterial.color = isMoving ? movingColor : isAttacking ? attackingColor : stationaryColor;
			primaryMaterial.SetColor(EmissionColor, (isMoving ? movingColor : isAttacking ? attackingColor : stationaryColor) * 5);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.tag.Equals("Obstacle_interactable"))
			{
				_interactables.Add(other.gameObject);
				InteractableEnteredZone?.Invoke(other.gameObject);
			}

			if (other.tag.Equals("Enemy"))
			{
				_enemies.Add(other.gameObject);
				EnemyEnteredZone?.Invoke(other.gameObject);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.tag.Equals("Obstacle_interactable") && _interactables.Contains(other.gameObject))
			{
				_interactables.Remove(other.gameObject);

				if (_interactables.Count == 0)
				{
					AllInteractablesExitedZone?.Invoke();
				}
			}
			if (other.tag.Equals("Enemy"))
			{
				_enemies.Remove(other.gameObject);
				EnemyExitedZone?.Invoke(other.gameObject);
			}
		}
	}
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ActionTimeline
{
	public class ActionTimelineTicker : MonoBehaviour
	{
		[SerializeField] private List<Image> childImageComponents = new();
		[SerializeField] private Image iconImage;
		[field: SerializeField] public Color DefaultColor { get; private set; }
		[field: SerializeField] public Color ActivationColor { get; private set; }

		public IEnumerable<Image> ChildImageComponents => childImageComponents;

		private void OnValidate()
		{
			childImageComponents.Clear();
			childImageComponents.AddRange(GetComponentsInChildren<Image>());

			foreach (var childImage in childImageComponents) childImage.color = DefaultColor;
		}

		public void SetIcon(Sprite icon) => iconImage.sprite = icon;
	}
}
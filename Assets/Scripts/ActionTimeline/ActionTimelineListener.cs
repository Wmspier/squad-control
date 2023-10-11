using CharacterBehaviors;
using UnityEngine;
using UnityEngine.Assertions;

namespace ActionTimeline
{
	public class ActionTimelineListener : MonoBehaviour
	{
		[SerializeField] private ActionTimeline actionTimeline;
		[SerializeField] private CharacterAbilityActivator characterAbilityActivator;

		private void Start()
		{
			Assert.IsNotNull(actionTimeline, "Timeline is null and ActionActivator depends on it.");
			Assert.IsNotNull(characterAbilityActivator,
				"CharacterAbilityActivator is null and ActionActivator depends on it.");
			Vibration.Init();

			actionTimeline.TickerHit += OnTickerHit;
			actionTimeline.TickerAnticipateHit += OnTickerAnticipateHit;
		}

		private void OnDestroy()
		{
			actionTimeline.TickerHit -= OnTickerHit;
			actionTimeline.TickerAnticipateHit -= OnTickerAnticipateHit;
		}

		private static void OnTickerHit(int tickIndex) { Vibration.VibratePeek(); }

		private void OnTickerAnticipateHit(int tickIndex) => characterAbilityActivator.ActivateAbilityAtIndex(tickIndex);
	}
}
using System;
using System.Collections.Generic;
using CharacterBehaviors;
using DG.Tweening;
using UnityEngine;

namespace ActionTimeline
{
	public class ActionTimeline : MonoBehaviour
	{
		[SerializeField] private RectTransform scrubber;
		[SerializeField] private float scrubTimeSeconds = 5f;
		[SerializeField] private List<ActionTimelineTicker> tickers;
		[SerializeField] private ActionTimelineTicker tickerPrefab;

		private int _currentTick;
		private int _currentTickAnticipation;
		private float _scrubElapsed;

		private float _timeLineCellWidth;
		private float _timelineWidth;
		
		public event Action<int> TickerHit;
		public event Action<int> TickerAnticipateHit;

		private void Start()
		{
			InitializeTickers();
			var rectTransform = GetComponent<RectTransform>();
			_timelineWidth = rectTransform.rect.width;
			_timeLineCellWidth = _timelineWidth / (tickers.Count * 2);
		}

		private void InitializeTickers()
		{
			var player = GameObject.FindWithTag("Player").GetComponent<CharacterAbilityActivator>();
			foreach (var abilityPayload in player.Abilities)
			{
				var newTicker = Instantiate(tickerPrefab, transform);
				newTicker.SetIcon(abilityPayload.Icon);
				tickers.Add(newTicker);
			}
		}

		private void Update()
		{
			_scrubElapsed += Time.deltaTime;
			if (_scrubElapsed > scrubTimeSeconds)
			{
				_scrubElapsed = 0f;
				_currentTick = 0;
				_currentTickAnticipation = 0;
			}

			var positionNormalized = _scrubElapsed / scrubTimeSeconds;
			var scrubberPosition = _timelineWidth * positionNormalized;
			scrubber.anchoredPosition = new Vector2(scrubberPosition, 0);

			var nextCenterTickerPosition = (1 + _currentTick * 2) * _timeLineCellWidth;
			var nextBeginningTickerPosition =
				(1 + _currentTickAnticipation * 2) * _timeLineCellWidth - _timeLineCellWidth / 2;

			if (scrubberPosition > nextBeginningTickerPosition)
			{
				TickerAnticipateHit?.Invoke(_currentTick);
				_currentTickAnticipation++;
			}
			else if (scrubberPosition > nextCenterTickerPosition)
			{
				TickerHit?.Invoke(_currentTick);
				DoTickActivation(tickers[_currentTick]);
				_currentTick++;
			}
		}

		private static void DoTickActivation(ActionTimelineTicker ticker)
		{
			ticker.transform.DOPunchScale(new Vector3(.5f, .5f, .5f), .25f).Play();

			var activeColorSequence = DOTween.Sequence();
			foreach (var childImage in ticker.ChildImageComponents)
				activeColorSequence.Insert(0, childImage.DOColor(ticker.ActivationColor, .25f));

			var defaultColorSequence = DOTween.Sequence();
			foreach (var childImage in ticker.ChildImageComponents)
				defaultColorSequence.Insert(0, childImage.DOColor(ticker.DefaultColor, .25f));

			DOTween.Sequence()
				.Append(activeColorSequence)
				.Append(defaultColorSequence)
				.Play();
		}
	}
}
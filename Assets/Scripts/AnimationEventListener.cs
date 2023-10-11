using System;
using UnityEngine;

public class AnimationEventListener : MonoBehaviour
{
	public event Action<string> EventTriggered;

	public void AnimationEventTrigger(string animationTag) { EventTriggered?.Invoke(animationTag); }
}
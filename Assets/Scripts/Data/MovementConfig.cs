using UnityEngine;

namespace Data
{
	[CreateAssetMenu(fileName = "MovementConfig", menuName = "Config/Movement")]
	public class MovementConfig : ScriptableObject
	{
		[field: Header("Character Controller")]
		[field: SerializeField] public float SlopeLimit { get; private set; }
		[field: SerializeField] public float StepOffset { get; private set; }
		
		[field: Header("Fixed")]
		[field: SerializeField][field: Tooltip("How fast the character moves")]  public float Speed { get; private set; }
		[field: SerializeField][field: Tooltip("How quickly the character rotates to face forward")]  public float RotationFactorPerFrame { get; private set; }
		[field: SerializeField][field: Tooltip("How quickly the character slows if no directional input is applied")]  public float MovementDecay { get; private set; }
		[field: SerializeField][field: Tooltip("Distance of joystick from center to apply max speed")] public float MaxRunDirectionalMagnitude { get; private set; }
		[field: SerializeField][field: Tooltip("Distance of joystick from center to apply 0 speed")] public float MinRunDirectionalMagnitude { get; private set; }
		
		[field: Header("Follow")]
		[field: SerializeField][field: Tooltip("How frequently the agent updates its destination based on the target")] public float TargetSearchFrequencySeconds { get; private set; }
		[field: SerializeField][field: Tooltip("The maximum distance from the target to send target reached event (must be larger than agent stopping distance)")] public float ReachedTargetRange { get; private set; }
	}
}
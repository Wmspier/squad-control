using Cinemachine;
using UnityEngine;

[ExecuteInEditMode]
[SaveDuringPlay]
[AddComponentMenu("")]
public class LockCameraYExtension : CinemachineExtension
{
	[Tooltip("Lock the camera's Y position to this value")] [SerializeField]
	private float yPosition = 10;

	protected override void PostPipelineStageCallback(
		CinemachineVirtualCameraBase vcam,
		CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (stage == CinemachineCore.Stage.Body)
		{
			var pos = state.RawPosition;
			pos.y = yPosition;
			state.RawPosition = pos;
		}
	}
}
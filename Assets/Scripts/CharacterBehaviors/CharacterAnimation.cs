using UnityEngine;

namespace CharacterBehaviors
{
	public class CharacterAnimation : MonoBehaviour
	{
		[SerializeField] private Animator animator;
		private readonly int _blendHash = Animator.StringToHash("Blend");
		private readonly int _dieTriggerHash = Animator.StringToHash("die");
		private readonly int _hitReactTriggerHash = Animator.StringToHash("hitReact");
		private readonly int _rollTriggerHash = Animator.StringToHash("roll");
		private readonly int _spawnTriggerHash = Animator.StringToHash("spawn");

		public void PlayWithTrigger(string trigger) => animator.SetTrigger(Animator.StringToHash(trigger));
		
		public void PlayHitReact() { animator.SetTrigger(_hitReactTriggerHash); }

		public void PlaySpawn() { animator.SetTrigger(_spawnTriggerHash); }

		public void PlayDie() { animator.SetTrigger(_dieTriggerHash); }

		public void PlayRoll() { animator.SetTrigger(_rollTriggerHash); }

		public void SetRunBlend(float blend) { animator.SetFloat(_blendHash, blend); }
	}
}
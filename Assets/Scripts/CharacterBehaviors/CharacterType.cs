using UnityEngine;

namespace CharacterBehaviors
{
	public enum CharacterTypeEnum
	{
		PlayerMelee,
		PlayerRanged,
		EnemyMelee,
		EnemyRanged
	}
	
	public class CharacterType : MonoBehaviour
	{
		[SerializeField] private Material playerMeleeMat;
		[SerializeField] private Material playerRangedeMat;
		[SerializeField] private Material EnemyMeleeMat;
		[SerializeField] private Material EnemyRangedMat;
		
		private SkinnedMeshRenderer[] _renderers;
		
		public CharacterTypeEnum Type { get; private set; }

		private void Awake()
		{
			_renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
		}

		public void SetType(CharacterTypeEnum type)
		{
			Type = type;
			var mat = type switch
			{
				CharacterTypeEnum.PlayerMelee => playerMeleeMat,
				CharacterTypeEnum.PlayerRanged => playerRangedeMat,
				CharacterTypeEnum.EnemyMelee => EnemyMeleeMat,
				CharacterTypeEnum.EnemyRanged => EnemyRangedMat
			};

			foreach (var r in _renderers)
			{
				r.material = mat;
			}
		}
	}
}
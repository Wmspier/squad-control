using UnityEngine;

namespace Test
{
	public class StaticTestLoader : MonoBehaviour
	{
		private void Awake()
		{
			StaticTestData.Instance.Load();
		}

		private void OnDestroy()
		{
			StaticTestData.Instance.Save();
		}
	}
}
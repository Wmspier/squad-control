using UnityEngine;

namespace Test
{
	public class StaticTestData
	{
		private static string StopToInteractKey = "StopToInteract";
		private static string SquadLeaderKey = "SquadHasLeader";
		private static string SquadMemberScaleKey = "SquadMemberScale";
		private static string SquadMemberSpacingKey = "SquadMemberSpacing";
		private static string SquadZoneInnerScaleKey = "SquadZoneInnerScale";
		private static string SquadZoneOuterScaleKey = "SquadZoneOuterScale";
		
		public const float SquadMemberLeaderScaleFactor = 1.5f;

		public static StaticTestData Instance => _instance ??= new StaticTestData();

		private static StaticTestData _instance;
		
		public bool StopToInteract;
		public bool SquadLeader;
		public float SquadMemberScale;
		public float SquadMemberSpacing;
		public float SquadInnerZoneScale;
		public float SquadOuterZoneScale;

		public void Save()
		{
			PlayerPrefs.SetInt(StopToInteractKey, StopToInteract ? 1 : 0);
			PlayerPrefs.SetInt(SquadLeaderKey, SquadLeader ? 1 : 0);
			PlayerPrefs.SetFloat(SquadMemberScaleKey, SquadMemberScale);
			PlayerPrefs.SetFloat(SquadMemberSpacingKey, SquadMemberSpacing);
			PlayerPrefs.SetFloat(SquadZoneInnerScaleKey, SquadInnerZoneScale);
			PlayerPrefs.SetFloat(SquadZoneOuterScaleKey, SquadOuterZoneScale);
			PlayerPrefs.Save();
		}

		public void Load()
		{
			if (PlayerPrefs.HasKey(StopToInteractKey)) StopToInteract = PlayerPrefs.GetInt(StopToInteractKey) == 1;
			if (PlayerPrefs.HasKey(SquadLeaderKey)) SquadLeader = PlayerPrefs.GetInt(SquadLeaderKey) == 1;
			if (PlayerPrefs.HasKey(SquadMemberScaleKey)) SquadMemberScale = PlayerPrefs.GetFloat(SquadMemberScaleKey);
			if (PlayerPrefs.HasKey(SquadMemberSpacingKey)) SquadMemberSpacing = PlayerPrefs.GetFloat(SquadMemberSpacingKey);
			if (PlayerPrefs.HasKey(SquadZoneInnerScaleKey)) SquadInnerZoneScale = PlayerPrefs.GetFloat(SquadZoneInnerScaleKey);
			if (PlayerPrefs.HasKey(SquadZoneOuterScaleKey)) SquadOuterZoneScale = PlayerPrefs.GetFloat(SquadZoneOuterScaleKey);
		}
	}
}
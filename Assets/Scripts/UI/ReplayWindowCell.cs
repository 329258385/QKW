using System;
using System.Collections.Generic;
using UnityEngine;
using Solarmax;

public class ReplayWindowCell : MonoBehaviour
{
	public GameObject bg;
	public GameObject[] races;
	public UILabel result;
	public UILabel resultScore;
	public UILabel time;

	private BattleReportData reportData;

	private Color winColor = new Color (1, 0.63f, 0.04f, 1);
	private Color loseColor = new Color (1, 0.42f, 0.42f, 1);

	public void SetInfo(int index, BattleReportData brd, bool myReport)
	{
		reportData = brd;

		List<SimplePlayerData> userList = new List<SimplePlayerData> ();
		foreach (var i in brd.playerList) {
			userList.Add (i);
		}
	


		// 玩家信息
		for (int i = 0; i < userList.Count; ++i)
		{
			var pd = userList [i];

			if (pd.userId == LocalPlayer.Get ().playerData.userId) {

			}

            //Team team = null;
            //for (int ti = 0; ti < brd.playerList.Count; ++ti)
            //{
            //    if (brd.playerList [ti].userId == pd.userId) {
            //        team = BattleSystem.Instance.sceneManager.teamManager.GetTeam ((TEAM)(ti + 1));
            //        break;
            //    }
            //}

			UILabel name = races [i].transform.Find ("name").GetComponent<UILabel> ();
			name.text = pd.name;
		}

		
		// 时间
		DateTime dt = new DateTime (1970, 1, 1);
		dt = dt.AddSeconds (reportData.time);
		TimeSpan ts = TimeSystem.Instance.GetServerTime () - dt;
		if (ts.Days > 0) {
			time.text = string.Format (DictionaryDataProvider.GetValue (102), ts.Days, ts.Hours);
		} else if (ts.Hours > 0) {
			time.text = string.Format (DictionaryDataProvider.GetValue (103), ts.Hours, ts.Minutes);
		} else {
			time.text = string.Format (DictionaryDataProvider.GetValue (104), Mathf.CeilToInt(ts.Minutes));
		}
		#if UNITY_EDITOR
		time.text += dt.ToString("M/d HH:mm:ss");
		#endif

		bg.gameObject.SetActive (index % 2 == 0);
	}

	public void OnPlayClick()
	{
		NetSystem.Instance.helper.BattleReportPlay (reportData);
		Invoke ("HideInvoke", 3.0f);
	}

	private void HideInvoke ()
	{

	}
}



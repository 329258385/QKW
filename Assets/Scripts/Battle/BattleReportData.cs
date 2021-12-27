using System;
using System.Collections.Generic;

public class BattleReportData
{
	public long id;
	public int playCount;
	public string group;
	public string matchId;
	public List<SimplePlayerData> playerList;
	public List<int> groupList;
	public long time;

	public void Init (NetMessage.BattleReport data)
	{
		id = data.id;
		playCount = data.play_count;

		playerList = new List<SimplePlayerData> ();
		for (int i = 0; i < data.data.Count; ++i) {
			SimplePlayerData spd = new SimplePlayerData ();
			spd.Init (data.data[i]);
			if (spd.userId < 0) {
				spd.name = AIManager.GetAIName (spd.userId);
				spd.icon = AIManager.GetAIIcon (spd.userId);
			}
			playerList.Add (spd);
		}

		groupList = new List<int> ();
		for (int i = 0; i < playerList.Count; ++i) {
			groupList.Add (-1);
		}

		if (data.group != null ) {
			group = data.group;
			///解析组队信息
			if (!string.IsNullOrEmpty (group)) {
				string[] strGroup = group.Split ('|');
				for (int i = 0; i < strGroup.Length; i++) {
					string[] teams = strGroup [i].Split (',');
					for (int j = 0; j < teams.Length; j++) {
						int userIndex = int.Parse (teams [j]);
						groupList [userIndex] = i;
					}
				}
			}
		}

		if (data.match_id != "")
			matchId = data.match_id;

		time = data.time;
	}
}


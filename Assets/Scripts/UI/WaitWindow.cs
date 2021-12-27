using UnityEngine;
using System.Collections.Generic;
using Solarmax;

public class WaitWindow : BaseWindow
{
	public GameObject infoTemplate;
	public GameObject[] posRoots; //1v1, 1v1v1, 1v1v1v1, 2v2


	private List<GameObject> infoList = new List<GameObject>();

	public void Awake()
	{
		
	}

	public override bool Init ()
	{
		RegisterEvent (EventId.OnReady);

		return true;
	}

	public override void OnShow()
	{
		// 隐藏所有位置节点
		for (int i = 0; i < posRoots.Length; ++i)
		{
			posRoots [i].SetActive (false);
		}

		AudioManger.Get ().PlayAudioBG ("Wandering");

	}

	public override void OnHide()
	{
		for (int i = 0; i < infoList.Count; ++i)
		{
			Destroy (infoList [i]);
		}
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		switch ((EventId)eventId) 
		{
		case EventId.OnReady:
			{
				// 设置房间显示数据
				SetPage();

				// 获取设置所有数据
				MapConfig matchMap = MapConfigProvider.Instance.GetData (BattleSystem.Instance.battleData.matchId);
				List<Team> groupTeams = new List<Team> ();

				for (int i = 0; i < matchMap.player_count; ++i) {
					TEAM team = (TEAM)(i + 1);
					Team t =  BattleSystem.Instance.sceneManager.teamManager.GetTeam (team);
					groupTeams.Add (t);
				}

				//按照队伍进行排序
				groupTeams.Sort((arg0, arg1) => {
					return arg0.groupID.CompareTo(arg1.groupID);
				});

				// 设置信息
				for (int i = 0; i < groupTeams.Count; ++i) {
					SetPosInfo (i, groupTeams[i]);
				}

				// 进入动画
				PlayAnimation ();
			}
			break;
		}
	}

	private void PlayAnimation()
	{
		MapConfig map = MapConfigProvider.Instance.GetData (BattleSystem.Instance.battleData.matchId);

		string statename = string.Empty;
		if (BattleSystem.Instance.battleData.teamFight) {
			statename = "WaitWindow_2v2_in";
		} else {
			if (map.player_count == 2)
				statename = "WaitWindow_1v1_in";
			else if (map.player_count == 3)
				statename = "WaitWindow_1v1v1_in";
			else if (map.player_count == 4)
				statename = "WaitWindow_1v1v1v1_in";
		}

		Animator ani = gameObject.GetComponent<Animator> ();
		ani.Play (statename);
	}


	/// <summary>
	/// 设置页面信息
	/// </summary>
	/// <param name="matchId">Match identifier.</param>
	private void SetPage()
	{
		MapConfig map = MapConfigProvider.Instance.GetData (BattleSystem.Instance.battleData.matchId);

		// 根据类型和人数判断选用哪种模式
		GameObject root = GetPosRoot();
		root.SetActive (true);

		for (int i = 0; i < map.player_count; ++i) {
			GameObject pos = root.transform.Find ("pos" + (i + 1)).gameObject;
			GameObject info = NGUITools.AddChild (pos, infoTemplate);
			info.name = "info";
			info.SetActive (true);
			SetPosInfo (i, null);
		}
	}

	private GameObject GetPosRoot()
	{
		MapConfig map = MapConfigProvider.Instance.GetData (BattleSystem.Instance.battleData.matchId);

		GameObject root = null;
		if (BattleSystem.Instance.battleData.teamFight) {
			root = posRoots [3];
		} else {
			root = posRoots [map.player_count - 2];
		}

		return root;
	}

	private void SetPosInfo(int pos, Team t)
	{
		GameObject root = GetPosRoot ();
		GameObject info = root.transform.Find ("pos" + (pos + 1)).Find("info").gameObject;

		if (t == null) {
			info.transform.Find ("full").gameObject.SetActive(false);
			info.transform.Find ("empty").gameObject.SetActive(true);

			return;
		}

        NetPlayer data = t.playerData;
		Color color = t.color;
		color.a = 1.0f;

		info.transform.Find ("full").gameObject.SetActive(true);
		info.transform.Find ("empty").gameObject.SetActive(false);

		UILabel score = info.transform.Find ("full/score/num").GetComponent<UILabel> ();
		UISprite kuang = info.transform.Find ("full/icon").GetComponent<UISprite> ();
		NetTexture icon = info.transform.Find ("full/icon/usericon").GetComponent<NetTexture> ();
		UILabel name = info.transform.Find ("full/icon/name").GetComponent<UILabel> ();
		
		score.text = data.score.ToString();
		icon.picUrl = data.icon;
		icon.picColor = color;
		kuang.color = color;
		name.text = data.name;
		name.color = color;

		Animator ani = info.GetComponent<Animator> ();
		if (data.userId == LocalPlayer.Get ().playerData.userId) {
			ani.Play ("WaitWindow_information1_mine");
		} else {
			ani.Play ("WaitWindow_information1_other");
		}
	}

	public void OnCloseClick()
	{
		UISystem.Get ().HideWindow ("WaitWindow");
		UISystem.Get ().ShowWindow ("StartWindow");
	}
}


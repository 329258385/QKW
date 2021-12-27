using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Solarmax;





public class BattleWindow_off : BaseWindow
{
	
	/// <summary>
	/// 飞船数量Label
	/// </summary>
	public UILabel          populationLabel = null;
	public UILabel          populationValueLabel = null;


	public UILabel popLable1;
	public UILabel popLableValue1;
	public UILabel popLableAdd;

	void Start()
	{
		SetPercent ();
	}


	public override bool Init ()
	{
		RegisterEvent (EventId.NoticeSelfTeam);
		RegisterEvent (EventId.OnBattleDisconnect);
		RegisterEvent (EventId.RequestUserResult);
		RegisterEvent (EventId.OnTouchBegin);
		RegisterEvent (EventId.OnTouchPause);
		RegisterEvent (EventId.OnTouchEnd);
		RegisterEvent (EventId.OnPopulationUp);
		RegisterEvent (EventId.OnPopulationDown);


		return true;
	}

	/// <summary>
	/// 显示当前飞船数量
	/// </summary>
	public override void OnShow()
	{

		Team team = BattleSystem.Instance.sceneManager.teamManager.GetTeam (BattleSystem.Instance.battleData.currentTeam);
		Color color = team.color;
		color.a = 0.7f;
        int current     = 0;
        int currentMax  = 0;
        for (int i = 0; i < team.battleArray.Count; i++ )
        {
            current     += team.battleArray[i].current;
            currentMax  += team.battleArray[i].currentMax;
        }
		// 人口颜色
		populationLabel.color = color;
		populationValueLabel.color = color;

        populationValueLabel.text = string.Format("{0}/{1}", current, currentMax);

		TimerProc ();
	}


	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.NoticeSelfTeam) {

			// 获取当前战斗中所有己方星球的ID

			if (BattleSystem.Instance.battleData.isReplay)
				return;	// 如果重播，则不提示这个窗口

			MapConfig map = MapConfigProvider.Instance.GetData (BattleSystem.Instance.battleData.matchId);

			Team selfTeam =  BattleSystem.Instance.sceneManager.teamManager.GetTeam (BattleSystem.Instance.battleData.currentTeam);

			for (int i = 0; i < map.player_count; ++i) {
				Team team = BattleSystem.Instance.sceneManager.teamManager.GetTeam ((TEAM)(i + 1));
				for (int j = 0; j < map.players.Count; ++j) {
					MapPlayerConfig pi = map.players [j];
					if (pi.camption == (int)team.team) {
						if (team == selfTeam) {

							//Node n = BattleSystem.Instance.sceneManager.nodeManager.GetNode (pi.tag);
							//EffectManager.Get ().ShowGuideEffect (n, true);
						} else if (selfTeam.IsFriend (team.groupID)) {

							//Node n = BattleSystem.Instance.sceneManager.nodeManager.GetNode (pi.tag);
							//EffectManager.Get ().ShowGuideEffect (n, false);
						}
					}
				}
			}

		}

		if (eventId == EventId.OnPopulationUp) {

			//设置显示人口数量
			int current = (int)args [0];
			int max = (int)args [1];
			int pop = (int)args [2];

			populationValueLabel.text = string.Format ("{0} / {1}", current, max);
			popLableValue1.text = populationValueLabel.text;

			//设置变色lable
			popLable1.color = new Color (0x00, 0xFF, 0x00, 1f);
			popLableValue1.color = new Color (0x00, 0xFF, 0x00, 1f);


			//设置增加数量
			popLableAdd.text = string.Format("+{0}", pop);
			popLableAdd.color = new Color (0.2f, 1f, 0.2f, 1f);

			// 加字的偏移
			popLableAdd.transform.localPosition = popLableValue1.transform.localPosition + new Vector3 (popLableValue1.printedSize.x + 30, 0, 0);

		}

		if (eventId == EventId.OnPopulationDown) {

			//设置显示人口数量
			int current = (int)args [0];
			int max = (int)args [1];
			int pop = (int)args [2];

			populationValueLabel.text = string.Format ("{0} / {1}", current, max);
			popLableValue1.text = populationValueLabel.text;

			//设置变色lable
			popLable1.color = new Color (0xFF, 0x00, 0x00, 1f);
			popLableValue1.color = new Color (0xFF, 0x00, 0x00, 1f);

			//设置增加数量
			popLableAdd.text = string.Format("-{0}", pop);
			popLableAdd.color = new Color (1f, 0.2f, 0.2f, 1f);

			// 加字的偏移
			popLableAdd.transform.localPosition = popLableValue1.transform.localPosition + new Vector3 (popLableValue1.printedSize.x + 30, 0, 0);
		}
	}

	public override void OnHide()
	{
		
	}


	void Update()
	{
		if (popLable1.alpha > 0) {
			popLableValue1.alpha = popLable1.alpha = (popLable1.alpha-Time.deltaTime * 0.5f);
			if (popLable1.alpha < 0f)
				popLableValue1.alpha = popLable1.alpha = 0f;
		}

		if (popLableAdd.alpha > 0) {
			popLableAdd.alpha -= Time.deltaTime * 0.5f;
			if (popLableAdd.alpha < 0f)
				popLableAdd.alpha = 0f;
		}
	}

	private void TimerProc()
	{
		Team team =  BattleSystem.Instance.sceneManager.teamManager.GetTeam (BattleSystem.Instance.battleData.currentTeam);
        int current     = 0;
        int currentMax  = 0;
        for (int i = 0; i < team.battleArray.Count; i++ )
        {
            current     += team.battleArray[i].current;
            currentMax  += team.battleArray[i].currentMax;
        }
        populationValueLabel.text = string.Format("{0} / {1}", current, currentMax);
		popLableValue1.text = populationValueLabel.text;

		Invoke ("TimerProc", 0.5f);
	}

	
	public void OnCloseClick()
	{
		UISystem.Get ().HideAllWindow ();
		UISystem.Get ().ShowWindow ("CustomSelectWindowNew");
	}

	private void SetPercent()
	{
		
	}

	private void GetBattleMapIndex (string mapId, out int index, out int total)
	{
		List<string> mapList = new List<string> ();
		Dictionary<string, MapConfig> mapDict = MapConfigProvider.Instance.GetAllData ();
		foreach (var i in mapDict) {
			if (i.Value.vertical == true) {
				mapList.Add ((string)i.Key);
			}
		}
		index = 0;
		total = mapList.Count;
		for (int i = 0; i < total; ++i) {
			if (mapId.Equals (mapList [i])) {
				index = i;
				break;
			}
		}
		mapList.Clear ();
	}

	/// <summary>
	/// 设置背景图
	/// </summary>
	public static void SetBattleBg (int mapIndex, int mapTotalCount)
	{
		GameObject bgParent = GameObject.Find ("Battle/BG");
        float x = -58.4f / mapTotalCount;
		x *= mapIndex;
		bgParent.transform.localPosition = new Vector3 (x, 0, 0);
	}
}


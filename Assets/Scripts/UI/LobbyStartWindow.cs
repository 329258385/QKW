using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Solarmax;






public class LobbyStartWindow : BaseWindow
{

    public UISprite         userIconTexture;
	public UILabel          userNameLabel;
	public UILabel          scoreLabel;
	public UILabel          goldLabel;
	public UILabel          gemLabel;


	private static int		selectMapIndex = 1;  // 当前选中
	private int				MapIndexMax = 0;        // 最大边界
	private List<string>	mapList = new List<string>();


	private void Awake()
	{
 
	}

	public override bool Init ()
	{
		RegisterEvent (EventId.OnStartSingleBattle);
		RegisterEvent (EventId.ReconnectResult);
		return true;
	}

	public override void OnShow()
	{
		SetPlayerInfo ();
		AudioManger.Get ().PlayAudioBG ("Empty");

		// 获取地图数据
		string alls		= GameVariableConfigProvider.Instance.GetData(1);
		string[] names	= alls.Split(',');
		for (int i = 0; i < names.Length; ++i)
		{
			mapList.Add(names[i]);
		}
		MapIndexMax		= mapList.Count - 1;
	}

	public override void OnHide ()
	{
        
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		switch ((EventId)eventId)
		{
			case EventId.OnStartSingleBattle:
				{
					OnStartSingleBattle();
					break;
				}
		
			case EventId.ReconnectResult:
				{
					// 刷新界面
					SetPlayerInfo ();
				}
				break;
		}
	}

	private void SetPlayerInfo()
	{
		// 设置用户名，头像等
		//userNameLabel.text = string.Format ("Hi, {0}", LocalPlayer.Get().playerData.name);
  //      if (string.IsNullOrEmpty(LocalPlayer.Get().playerData.icon))
  //          userIconTexture.spriteName = "select_head_head_1";
  //      else
		//    userIconTexture.spriteName = LocalPlayer.Get().playerData.icon;
	}

	/// <summary>
	/// 开始战斗
	/// </summary>
	public void JoinGameOnClicked()
	{
		OnStartBattle();
	}


	/// <summary>
	/// 设置
	/// </summary>
	public void OnSettingClick()
	{
		//UISystem.Get ().HideAllWindow ();
		//UISystem.Get ().ShowWindow ("SettingWindow");
	}


    /// <summary>
    /// 竞技场
    /// </summary>
    public void OnGloryPreviewClick()
    {
        //UISystem.Get().HideAllWindow();
        //UISystem.Get().ShowWindow("GloryPreviewWindow");
    }


    /// <summary>
    /// 我的機庫
    /// </summary>
    /// <param name="go">Go.</param>
    public void OnGarageWindowClick()
	{
		//UISystem.Get ().HideAllWindow ();
		//UISystem.Get ().ShowWindow ("GarageWindow");
	}

	/// <summary>
	/// 成就
	/// </summary>
	/// <param name="go">Go.</param>
	public void OnRewardClick()
	{
		//UISystem.Get ().HideAllWindow ();
        //UISystem.Get ().ShowWindow("FriendWindow");
	}
	/// <summary>
	/// 排行
	/// </summary>
	/// <param name="go">Go.</param>
	public void OnRankClick()
	{
		//UISystem.Get ().HideAllWindow ();
		//UISystem.Get ().ShowWindow ("RankWindow");
	}
	/// <summary>
	/// 实验室入口
	/// </summary>
	public void OnLaboratoryClick()
	{
		//UISystem.Get ().HideAllWindow ();
		//UISystem.Get ().ShowWindow ("LaboratoryWindow");
	}


    /// <summary>
    /// 军情任务入口
    /// </summary>
    public void OnBattleQuestClick()
    {
        //UISystem.Get().HideAllWindow();
        //UISystem.Get().ShowWindow("LaboratoryWindow");
    }


    /// <summary>
    /// 商店入口
    /// </summary>
    public void OnShipShopClick()
    {
        //UISystem.Get().HideAllWindow();
        //UISystem.Get().ShowWindow("ShopWindow");
    }

	public void TimeBoxClicked()
	{
		//UISystem.Instance.HideAllWindow ();
		//UISystem.Instance.ShowWindow ("ChapterWindow");
	}

	/// ---------------------------------------------------------------------------------------
	/// <summary>
	/// 单机返回按钮
	/// </summary>
	/// ---------------------------------------------------------------------------------------
	public void OnStartBattle()
	{
		if (selectMapIndex == 0 || selectMapIndex > MapIndexMax)
			return;

		BuildTypeManager buildManager = Game.game.sceneRoot.GetComponentInChildren<BuildTypeManager>();
		if (buildManager != null)
		{
			buildManager.InitSceneBuilds();

			// 开始单机
			string map = mapList[selectMapIndex];
			NetSystem.Instance.helper.RequestSingleMatch(map, GameType.Single, buildManager.MapList );
		}
	}

	public void OnStartSingleBattle()
	{

		TweenAlpha ta = gameObject.GetComponent<TweenAlpha>();
		if (ta == null)
		{
			ta = gameObject.AddComponent<TweenAlpha>();
		}

		ta.ResetToBeginning();
		ta.from = 1f;
		ta.to = 0;
		ta.duration = 0.6f;
		ta.SetOnFinished(() => {
			StartSingleBattle();
		});
	}

	/// <summary>
	/// 开始单人战斗放在这里了 fix by ljp 
	/// </summary>
	public void StartSingleBattle()
	{
        // 进入战斗
        UISystem.Get().HideAllWindow();
		BattleSystem.Instance.StartLockStep();
		UISystem.Get().HideAllWindow();
		UISystem.Get().ShowWindow("BattlePublicyWindow");
	}
}


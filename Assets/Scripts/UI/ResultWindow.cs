using UnityEngine;
using System.Collections.Generic;
using Solarmax;





public class ResultWindow : BaseWindow
{
	public UILabel              result;
	public GameObject[]         posRoots;           //1v1, 1v1v1, 1v1v1v1, 2v2
    public GameObject[]         btnAttention;
    
    public UIPlayAnimation      aniPlayer;	// 动画
	public GameObject           mvpSprite;

    public UITable              uiTable;
	public GameObject           moneyObj;
	public UILabel              moneyLabel;
    private bool                IsNeedPlayMvpEffect = false;


	public override bool Init ()
	{
		RegisterEvent (EventId.OnFinished);
        RegisterEvent(EventId.OnFriendFollowResult);
        for (int i = 0; i < 4; i++ )
        {
            btnAttention[i].GetComponent<UIEventListener>().onClick += OnAttentionClick;
        }
        return true;
	}

	public override void OnShow()
	{
		// 根据类型和人数判断选用哪种模式
		SetPage();

		// 进入动画
        PlayAnimation("ResultWindow_h2");
	}

	public override void OnHide()
	{

    }

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnFinished)
        {
            
		}

        if (eventId == EventId.OnFriendFollowResult)
        {
            // 关注好友结果
            int userId                  = (int)args[0];
            NetMessage.ErrCode errCode  = (NetMessage.ErrCode)args[2];
            if (errCode == NetMessage.ErrCode.EC_Ok)
            {
                Tips.Make(Tips.TipsType.FlowUp, "关注成功", 1.0f);
                for (int i = 0; i < 4; i++)
                {
                    UILabel ID = btnAttention[i].transform.Find("useid").GetComponent<UILabel>();
                    if (ID == null || string.IsNullOrEmpty(ID.text))
                        continue;

                    /// 隐藏关注按钮
                    int useID = int.Parse(ID.text);
                    if (userId == useID)
                        btnAttention[i].SetActive(false);
                }
            }
        }
	}


    /// <summary>
    /// 播放MVP动画
    /// </summary>
    public void OnPlayerFistAniamionEnd()
    {
        Invoke("PlayerFistAniamion", 0.5f);
    }


    /// <summary>
    /// 播放MVP动画
    /// </summary>
    private void PlayerFistAniamion()
    {
        PlayMvpEffect();
    }

	/// <summary>
	/// 设置页面信息
	/// </summary>
	private void SetPage()
	{

        // 隐藏所有位置节点
        for (int i = 0; i < posRoots.Length; ++i)
        {
            posRoots[i].SetActive(false);
        }


        MapConfig map           = MapConfigProvider.Instance.GetData (BattleSystem.Instance.battleData.matchId);
		List<Team> groupTeams   = new List<Team> ();
		Team selfTeam           = BattleSystem.Instance.sceneManager.teamManager.GetTeam (BattleSystem.Instance.battleData.currentTeam);
		Team winTeam            = BattleSystem.Instance.sceneManager.teamManager.GetTeam (BattleSystem.Instance.battleData.winTEAM);
		bool timeout            = false;
		//CalculateWinTeam (map, out winTeam, out timeout);


        // 胜利方的友方
		for (int i = 0; i < map.player_count; ++i) {
			TEAM T = (TEAM)(i + 1);
			Team t =  BattleSystem.Instance.sceneManager.teamManager.GetTeam (T);
			if (t != winTeam && winTeam.IsFriend (t.groupID)) 
			{
				groupTeams.Add (t);
			} 
            else if (t == winTeam)
            {
                groupTeams.Add (t);
            }
		}
		if (groupTeams.Count > 0) {
			groupTeams.Sort ((arg0, arg1) => {
				int ret = arg0.resultOrder.CompareTo (arg1.resultOrder);
				if (ret == 0)
					ret = arg0.scoreMod.CompareTo (arg1.scoreMod);
				if (ret == 0)
					ret = arg0.destory.CompareTo (arg1.destory);
				return -ret;
			});
		}

		// 失败方
        List<Team> failTeams = new List<Team>();
        for (int i = 0; i < map.player_count; ++i)
        {
            TEAM T = (TEAM)(i + 1);
            Team t = BattleSystem.Instance.sceneManager.teamManager.GetTeam(T);
            if (t != winTeam && !winTeam.IsFriend(t.groupID) && !groupTeams.Contains(t))
            {
                failTeams.Add(t);
            }
        }
		failTeams.Sort((arg0, arg1) => {
			int ret = arg0.resultOrder.CompareTo (arg1.resultOrder);
			if (ret == 0)
				ret = arg0.scoreMod.CompareTo (arg1.scoreMod);
			if (ret == 0)
				ret = arg0.destory.CompareTo (arg1.destory);
			return -ret;
		});

		groupTeams.AddRange (failTeams);
	

		if (groupTeams.Contains (selfTeam))
		{
			// 此场战斗中包含自己
			// 判断mvp还是胜利
			if (timeout) {
				// 平局
				result.text = DictionaryDataProvider.GetValue(110);
			} else {
				if (selfTeam == winTeam) {
					// 注意此种逻辑只能在胜方包含己方时，因为重播如果有可能win和self都是neutral
					// mvp
					result.text = DictionaryDataProvider.GetValue (105);
				} else if (selfTeam.scoreMod > 0) {
					// 胜利
					result.text = DictionaryDataProvider.GetValue (100);
				} else {
					// 失败
					result.text = DictionaryDataProvider.GetValue (101);
				}
			}
			// 音效
			if (selfTeam.scoreMod > 0) {
				AudioManger.Get ().PlayEffect ("onPVPvictory");
			} else {
				AudioManger.Get ().PlayEffect ("onPVPdefeated");
			}
		}
		else
		{
			// 此场战斗不包含自己
			result.text = DictionaryDataProvider.GetValue(106);
		}
        

		// 根据类型和人数判断选用哪种模式
		for (int i = 0; i < groupTeams.Count; ++i)
        {
			Team t = groupTeams [i];
			SetPosInfo (i, t, t.scoreMod, t.destory, winTeam);
		}

        if (winTeam.team != TEAM.Neutral)
        {
            IsNeedPlayMvpEffect = true;
        }

        /// 只奖励金钱
        moneyLabel.text = string.Format("X{0}", map.reward );
    }

	/// <summary>
	/// 计算赢的队伍
	/// </summary>
	/// <returns>The window team.</returns>
	private void CalculateWinTeam (MapConfig map, out Team win, out bool timeout)
	{
		win = BattleSystem.Instance.sceneManager.teamManager.GetTeam (TEAM.Neutral);
		timeout = false;
		for (int i = 0; i < map.player_count; ++i) {
			TEAM T = (TEAM)(i + 1);
			Team t =  BattleSystem.Instance.sceneManager.teamManager.GetTeam (T);

			if (t.resultEndtype == NetMessage.EndType.ET_Win) {
				win = t;
			} else if (t.resultEndtype == NetMessage.EndType.ET_Timeout) {
				timeout = true;
			}
		}
	}

	
	private void SetPosInfo(int pos, Team team, int score, int destroy, Team winTeam )
	{
		if (pos >= posRoots.Length) 
		{
			return;
		}

        GameObject info     = posRoots[pos];
        info.SetActive(true);
        Color color         = team.color;
        color.a             = 1.0f;

        UISprite icon       = info.transform.Find("icon").GetComponent<UISprite>();
        UISprite iconbg     = info.transform.Find("icon_g").GetComponent<UISprite>();
        UISprite kuang      = info.transform.Find("kuang").GetComponent<UISprite>();
        UILabel name        = info.transform.Find("name").GetComponent<UILabel>();
        UILabel sco         = info.transform.Find("score").GetComponent<UILabel>();
        UILabel des         = info.transform.Find("num").GetComponent<UILabel>();
        UISprite win        = info.transform.Find("winbg").GetComponent<UISprite>();
        UISprite mvp        = info.transform.Find("mvp").GetComponent<UISprite>();

        if (winTeam.IsFriend(team.groupID))
        {
            win.spriteName = "result_victory_bg";
            mvp.gameObject.SetActive(true);
        }
        else
        {
            win.spriteName = "result_failure_bg";
            mvp.gameObject.SetActive(false);
        }

        
        {
            iconbg.spriteName   = "";
            icon.spriteName     = team.playerData.icon;
        }
        

        kuang.spriteName        = team.iconname;
        name.text               = team.playerData.name;
        name.color              = color;

        GameObject attention = info.transform.Find("attention").gameObject;
        Team selfTeam = BattleSystem.Instance.sceneManager.teamManager.GetTeam(BattleSystem.Instance.battleData.currentTeam);
        if (selfTeam.team != team.team && team.playerData.userId > 0 && !FriendDataHandler.Get().IsMyFollowEX(team.playerData.userId))
        {
            attention.SetActive(true);
            UILabel UseID = attention.transform.Find("useid").GetComponent<UILabel>();
            if (UseID != null)
            {
                UseID.text = team.playerData.userId.ToString();
            }
        }
        else
        {
            attention.SetActive(false);
            UILabel UseID = attention.transform.Find("useid").GetComponent<UILabel>();
            if (UseID != null)
            {
                UseID.text = "";
            }
        }

        string str;
        if (score > 0)
            str = string.Format("+{0}", score);
        else
            str = score.ToString();
        sco.text = str;
        des.text = destroy.ToString();
    }

	private void PlayAnimation( string strAni )
	{
		// 播放进入动画
        aniPlayer.clipName = strAni;
        aniPlayer.resetOnPlay = true;
        aniPlayer.Play(true, false);
	}


    private void PlayMvpEffect()
    {
        if (!IsNeedPlayMvpEffect)
            return;

        mvpSprite.gameObject.SetActive(true);
        IsNeedPlayMvpEffect = false;
    }


	public void OnGoHomeClick()
	{
		BattleSystem.Instance.Reset ();
        BattleSystem.Instance.battleData.resumingFrame = -1;
        UISystem.Get().HideAllWindow();
        UISystem.Get().ShowWindow("StartWindow");
    }

	public void OnShareClick()
	{
		Debug.LogWarning ("You have click the share button!");
	}



	public void OnLeagueConfirmClick ()
	{
		// 回到联赛主页面
		BattleSystem.Instance.Reset ();
		UISystem.Get ().HideAllWindow ();
		UISystem.Get ().ShowWindow ("CustomSelectWindowNew");
		EventSystem.Instance.FireEvent (EventId.OnManualSelectLeaguePage);
	}



    /// <summary>
    /// 关注对方，针对PVP 战斗
    /// </summary>
    /// <param name="go"></param>
    public void OnAttentionClick( GameObject go )
    {
        if (go == null)
            return;

        UILabel name = go.transform.Find("useid").GetComponent<UILabel>();
        if (name == null || string.IsNullOrEmpty(name.text))
            return;

        int useID = int.Parse(name.text);
        NetSystem.Instance.helper.FriendFollow(useID, true);
    }
}


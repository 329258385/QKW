using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Solarmax;

public class LeagueWindow : BaseWindow {

	/// <summary>
	/// 未打开页
	/// </summary>
	public GameObject notopenPage;
	/// <summary>
	/// 报名页
	/// </summary>
	public GameObject signupPage;
	public UILabel signLastTime;
	public UILabel signLeagueTime;
	public UILabel signLeagueDesc;
	public UIButton signBtn;
	/// <summary>
	/// 未开始页
	/// </summary>
	public GameObject waitPage;
	public UILabel waitLastTime;
	public UILabel waitAreadyTime;
	public UILabel waitLeagueTotalTime;
	public UILabel waitLeaguePlayer;
	public UILabel waitLeagutTime;
	public UIButton waitBattleBtn;
	/// <summary>
	/// 战斗页
	/// </summary>
	public GameObject battlePage;
	public UILabel battleLastTime;
	public UILabel battleScore;
	public UILabel battleMVP;
	public GameObject[] battleStaticRanks;
	public UIScrollView battleRankScrollView;
	public UIGrid battleRankGrid;
	/// <summary>
	/// 活动结束页
	/// </summary>
	public GameObject resultPage;
	public UILabel resultRank;
	public UILabel resultScore;
	public UILabel resultMVP;
	public GameObject[] resultStaticRanks;
	public UIScrollView resultRankScrollView;
	public UIGrid resultRankGrid;

	private DateTime start;
	private DateTime end;

	private NetMessage.LeagueInfo leagueInfo;
	private NetMessage.MemberInfo selfInfo;

	/// <summary>
	/// 以下均为动态列表所需要的数据
	/// </summary>
	private List<NetMessage.MemberInfo> rankList;
	private int selfRankIndex;
	private int showIndexMin;
	private int showIndexMax;
	private UIGrid grid;
	private GameObject template;

	public override bool Init ()
	{
		rankList = new List<NetMessage.MemberInfo> ();

		RegisterEvent (EventId.OnGetLeagueInfoResult);
		RegisterEvent (EventId.OnLeagueListResult);
		RegisterEvent (EventId.OnLeagueSignUpResult);
		RegisterEvent (EventId.OnLeagueRankResult);
		RegisterEvent (EventId.OnLeagueMatchResult);

		return true;
	}

	public override void OnShow ()
	{
		ShowPage (false, false, false, false, false);


		InvokeRepeating ("UpdateTime", 1.0f, 1.0f);
	}

	public override void OnHide ()
	{
		rankList.Clear ();
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnGetLeagueInfoResult) {
			// 获取的是自身的联赛信息，则要么在战斗，要么在等待
			leagueInfo = (NetMessage.LeagueInfo)args [0];
			selfInfo = (NetMessage.MemberInfo)args [1];

			start = TimeSystem.Instance.GetTime (leagueInfo.combat_start);
			end = TimeSystem.Instance.GetTime (leagueInfo.combat_finish);
			DateTime serverTime = TimeSystem.Instance.GetServerTime ();

			if (serverTime.CompareTo (start) < 0) {
				// 未开始
				SetWaitPage ();
			} else if (serverTime.CompareTo (end) < 0) {
				// 战斗中
				if (selfInfo.battle_num == 0) {
					SetWaitPage ();
				} else {
					SetBattlePage ();
				}
			} else {
				// 已结束
				SetResultPage ();
			}

		} else if (eventId == EventId.OnLeagueListResult) {
			// 展示报名信息
			int start = (int)args [0];
			IList<NetMessage.LeagueInfo> leagueList = (IList<NetMessage.LeagueInfo>)args [1];

			if (leagueList == null || leagueList.Count == 0) {
				// 没有活动
				SetNotOpenPage ();
			} else {
				// 显示第一个活动
				SetSignUpPage (leagueList [0]);
			}
		} else if (eventId == EventId.OnLeagueSignUpResult) {
			// 报名结果
			NetMessage.ErrCode code = (NetMessage.ErrCode)args [0];
			string leagueId = (string)args [1];
			if (code == NetMessage.ErrCode.EC_Ok) {
				// 不需要，自动发送获取联赛信息的消息

			} else if (code == NetMessage.ErrCode.EC_LeagueIsFull) {
				Tips.Make ("报名活动失败！");
			} else if (code == NetMessage.ErrCode.EC_LeagueIn) {
				Tips.Make ("您已经在其他锦标赛活动中！");
			} else if (code == NetMessage.ErrCode.EC_LeagueNotExist) {
				Tips.Make ("当前锦标赛活动不存在！");
			} else if (code == NetMessage.ErrCode.EC_LeagueNotOpen) {
				Tips.Make ("当前锦标赛活动未开启！");
			} else {
				Tips.Make ("未知错误！code:" + code);	
			}
		} else if (eventId == EventId.OnLeagueRankResult) {
			IList<NetMessage.MemberInfo> list = (IList<NetMessage.MemberInfo>)args [1];
			rankList.AddRange (list);
//			for (int i = 0; i < 100; ++i) {
//				NetMessage.MemberInfo.Builder mib = new NetMessage.MemberInfo.Builder ();
//				mib.SetId (10000 + i);
//				mib.SetIcon ("----");
//				mib.SetName ("Temp:" + i);
//				
//				mib.SetMvp (i);
//				mib.SetScore (i);
//				mib.SetBattleNum (i);
//				rankList.Add (mib.Build ());
//			}
			if (battlePage.activeSelf) {
				SetRankScrollView (battleRankScrollView, battleRankGrid, battleStaticRanks);
			} else if (resultPage.activeSelf) {
				SetRankScrollView (resultRankScrollView, resultRankGrid, resultStaticRanks);
				// 设置结果自己的排名
				SetResultSelfRank ();
			}
		} else if (eventId == EventId.OnLeagueMatchResult) {
			NetMessage.ErrCode code = (NetMessage.ErrCode)args [0];
			if (code == NetMessage.ErrCode.EC_Ok) {
				
			} else if (code == NetMessage.ErrCode.EC_LeagueNotInMatchTime) {
				Tips.Make ("当前赛事不在战斗时间内！");
			} else {
				Tips.Make ("开始锦标赛战斗失败！");
			}
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void UpdateTime ()
	{
		if (signupPage.activeSelf)
			UpdateTimeSignUp ();
		else if (waitPage.activeSelf)
			UpdateTimeWait ();
		else if (battlePage.activeSelf)
			UpdateTimeBattle ();
	}

	/// <summary>
	/// 设置打开的分页
	/// </summary>
	private void ShowPage (bool notOpen, bool signUp, bool wait, bool battle, bool result)
	{
		notopenPage.gameObject.SetActive (notOpen);
		signupPage.gameObject.SetActive (signUp);
		waitPage.gameObject.SetActive (wait);
		battlePage.gameObject.SetActive (battle);
		resultPage.gameObject.SetActive (result);
	}

	private void SetNotOpenPage ()
	{
		ShowPage (true, false, false, false, false);
	}

	/// <summary>
	/// 设置报名窗口
	/// </summary>
	/// <param name="leagueInfo">League info.</param>
	private void SetSignUpPage (NetMessage.LeagueInfo leagueInfo)
	{
		this.leagueInfo = leagueInfo;
		ShowPage (false, true, false, false, false);

		start = TimeSystem.Instance.GetTime (leagueInfo.signup_start);
		end = TimeSystem.Instance.GetTime (leagueInfo.signup_finish);

		// 报名剩余时间
		UpdateTimeSignUp ();

		// 比赛时间
		DateTime battleStart = TimeSystem.Instance.GetTimeCST (leagueInfo.combat_start);
		DateTime battleFinish = TimeSystem.Instance.GetTimeCST (leagueInfo.combat_finish);
		if (battleStart.Year == battleFinish.Year && battleStart.Month == battleFinish.Month && battleStart.Day == battleFinish.Day) {
			signLeagueTime.text = DictionaryDataProvider.Format (703, battleStart.Year, battleStart.Month, battleStart.Day, battleStart.Hour, battleStart.Minute
				, battleFinish.Hour, battleFinish.Minute);
		} else {
			signLeagueTime.text = DictionaryDataProvider.Format (704, battleStart.Year, battleStart.Month, battleStart.Day, battleStart.Hour, battleStart.Minute
				, battleFinish.Year, battleFinish.Month, battleFinish.Day, battleFinish.Hour, battleFinish.Minute);
		}

		// 说明
		signLeagueDesc.text = leagueInfo.desc;
	}

	/// <summary>
	/// 报名
	/// </summary>
	public void OnSignupClick ()
	{
		
		// 弹出提示框
		DateTime battleStart = TimeSystem.Instance.GetTimeCST (leagueInfo.combat_start);
		DateTime battleFinish = TimeSystem.Instance.GetTimeCST (leagueInfo.combat_finish);
		string str = string.Empty;
		if (battleStart.Year == battleFinish.Year && battleStart.Month == battleFinish.Month && battleStart.Day == battleFinish.Day) {
			str = DictionaryDataProvider.Format (717, battleStart.Year, battleStart.Month, battleStart.Day, battleStart.Hour, battleStart.Minute
				, battleFinish.Hour, battleFinish.Minute);
		} else {
			str = DictionaryDataProvider.Format (718, battleStart.Year, battleStart.Month, battleStart.Day, battleStart.Hour, battleStart.Minute
				, battleFinish.Year, battleFinish.Month, battleFinish.Day, battleFinish.Hour, battleFinish.Minute);
		}
	}

	/// <summary>
	/// 刷新报名时间
	/// </summary>
	private void UpdateTimeSignUp ()
	{
		DateTime serverTime = TimeSystem.Instance.GetServerTime ();

		if (start.CompareTo (serverTime) > 0) {
			// 表明尚未开始
			signLastTime.text = DictionaryDataProvider.GetValue (707);
			signBtn.enabled = false;
		} else if (end.CompareTo (serverTime) < 0) {
			// 已结束
			signLastTime.text = DictionaryDataProvider.GetValue (708);
			signBtn.enabled = false;
		} else {
			TimeSpan t = end - serverTime;
			if (t.TotalDays > 1) {
				signLastTime.text = DictionaryDataProvider.Format (701, t.Days, t.Hours);
			} else {
				signLastTime.text = DictionaryDataProvider.Format (702, t.Hours, t.Minutes, t.Seconds);
			}
			signBtn.enabled = true;
		}
	}

	/// <summary>
	/// 注册页帮助按钮
	/// </summary>
	public void OnSinupHelpBtnClick ()
	{
		UISystem.Get ().ShowDialogWindow (1, DictionaryDataProvider.GetValue (750));
	}

	/// <summary>
	/// 设置等待界面
	/// </summary>
	private void SetWaitPage ()
	{
		ShowPage (false, false, true, false, false);

		start = TimeSystem.Instance.GetTime (leagueInfo.combat_start);
		end = TimeSystem.Instance.GetTime (leagueInfo.combat_finish);

		UpdateTimeWait ();

		//时长
		TimeSpan battleTS = end - start;
		if (battleTS.Hours == 0) {
			waitLeagueTotalTime.text = DictionaryDataProvider.Format (719, battleTS.Minutes);
		} else if (battleTS.Hours > 0 && battleTS.Minutes == 0) {
			waitLeagueTotalTime.text = DictionaryDataProvider.Format (715, battleTS.Hours);
		} else {
			waitLeagueTotalTime.text = DictionaryDataProvider.Format (720, battleTS.Hours, battleTS.Minutes);
		}

		// NUM
		waitLeaguePlayer.text = leagueInfo.cur_num.ToString ();

		// time
		DateTime showStart = TimeSystem.Instance.GetTimeCST (start);
		DateTime showEnd = TimeSystem.Instance.GetTimeCST (end);
		if (showStart.Year == showEnd.Year && showStart.Month == showEnd.Month && showStart.Day == showEnd.Day) {
			waitLeagutTime.text = DictionaryDataProvider.Format (703, showStart.Year, showStart.Month, showStart.Day, showStart.Hour, showStart.Minute
				, showEnd.Hour, showEnd.Minute);
		} else {
			waitLeagutTime.text = DictionaryDataProvider.Format (704, showStart.Year, showStart.Month, showStart.Day, showStart.Hour, showStart.Minute
				, showEnd.Year, showEnd.Month, showEnd.Day, showEnd.Hour, showEnd.Minute);
		}
	}

	/// <summary>
	/// 开始对战
	/// </summary>
	public void OnCombatClick ()
	{
		NetSystem.Instance.helper.StartMatchReq (NetMessage.MatchType.MT_League, leagueInfo.id, false);
		// NetSystem.Instance.helper.MatchLeague (leagueInfo.Id);
	}

	/// <summary>
	/// 更新等待界面时间
	/// </summary>
	private void UpdateTimeWait ()
	{
		DateTime serverTime = TimeSystem.Instance.GetServerTime ();

		if (start.CompareTo (serverTime) > 0) {
			// 尚未开始
			TimeSpan t = start - serverTime;
			waitAreadyTime.gameObject.SetActive (false);
			waitLastTime.transform.parent.gameObject.SetActive (true);
			waitLastTime.text = DictionaryDataProvider.Format (702, (int)Math.Floor (t.TotalHours), t.Minutes, t.Seconds);

		} else if (end.CompareTo (serverTime) < 0) {
			// 已结束
			waitAreadyTime.gameObject.SetActive (false);
			waitLastTime.transform.parent.gameObject.SetActive (true);
			waitLastTime.text = DictionaryDataProvider.GetValue (710);
		} else {
			// 战斗时段中
			TimeSpan t = serverTime - start;
			waitAreadyTime.gameObject.SetActive (true);
			if (t.TotalHours > 1) {
				waitAreadyTime.text = DictionaryDataProvider.Format (714, (int)Math.Floor (t.TotalHours), t.Minutes, t.Seconds);
			} else if (t.TotalMinutes > 1) {
				waitAreadyTime.text = DictionaryDataProvider.Format (713, t.Minutes, t.Seconds);
			} else {
				waitAreadyTime.text = DictionaryDataProvider.Format (712, t.Seconds);
			}
			waitLastTime.transform.parent.gameObject.SetActive (false);
		}
	}

	/// <summary>
	/// 等待页帮助按钮
	/// </summary>
	public void OnWaitHelpBtnClick ()
	{
		UISystem.Get ().ShowDialogWindow (1, DictionaryDataProvider.GetValue (751));
	}

	/// <summary>
	/// 显示战斗过程中的界面
	/// </summary>
	private void SetBattlePage ()
	{
		ShowPage (false, false, false, true, false);

		start = TimeSystem.Instance.GetTime (leagueInfo.combat_start);
		end = TimeSystem.Instance.GetTime (leagueInfo.combat_finish);

		DateTime serverTime = TimeSystem.Instance.GetServerTime ();
		//结束时间
		TimeSpan t = end - serverTime;
		battleLastTime.text = DictionaryDataProvider.Format (702, (int)Math.Floor (t.TotalHours), t.Minutes, t.Seconds);
		// score
		battleScore.text = selfInfo.score.ToString();
		// mvp
		battleMVP.text = selfInfo.mvp.ToString();

		// 排行榜由另一个页面设置，先设空
		for (int i = 0; i < battleStaticRanks.Length; ++i) {
			SetBattleRankItem (battleStaticRanks [i], null, i + 1);
		}
	}

	private void UpdateTimeBattle ()
	{
		DateTime serverTime = TimeSystem.Instance.GetServerTime ();
		TimeSpan t = end - serverTime;
		battleLastTime.text = DictionaryDataProvider.Format (702, (int)Math.Floor (t.TotalHours), t.Minutes, t.Seconds);
	}

	/// <summary>
	/// 设置排行单项数据
	/// </summary>
	private void SetBattleRankItem (GameObject go, NetMessage.MemberInfo memberInfo, int rank)
	{
		string icon = "";
		string name = "--";
		int score = 0;
		int mvp = 0;
		bool self = false;
		if (memberInfo != null) {
			icon = memberInfo.icon;
			name = memberInfo.name;
			score = memberInfo.score;
			mvp = memberInfo.mvp;
			self = memberInfo.id == LocalPlayer.Get ().playerData.userId;
		}
		go.transform.Find ("icon").GetComponent<UISprite> ().spriteName = icon;
		go.transform.Find ("name").GetComponent<UILabel> ().text = name;
		go.transform.Find ("score").GetComponent<UILabel> ().text = score.ToString();
		go.transform.Find ("mvp").GetComponent<UILabel> ().text = mvp.ToString();
		go.transform.Find ("rank").GetComponent<UILabel> ().text = rank.ToString();
		if (rank > 3) {
			go.transform.Find ("rank/bg").gameObject.SetActive (false);
		}
		go.transform.Find ("bg").gameObject.SetActive (self);
		go.SetActive (memberInfo != null);
	}

	/// <summary>
	/// 设置排行数据
	/// </summary>
	private void SetRankScrollView (UIScrollView scrollview, UIGrid grid, GameObject[] staticRanks)
	{
		this.grid = grid;
		template = staticRanks [0];

		grid.onCustomSort = (arg0, arg1) => {
			int a0 = int.Parse(arg0.name);
			int a1 = int.Parse(arg1.name);
			return a0.CompareTo (a1);
		};

		scrollview.onShowMore = OnScrollViewShowMore;
		scrollview.onShowLess = OnScrollViewShowLess;

		// 1-3
		for (int i = 0; i < staticRanks.Length; ++i) {
			SetBattleRankItem (staticRanks [i], i >= rankList.Count ? null : rankList [i], i + 1);
		}

		// self rank
		selfRankIndex = -1;
		for (int i = 0, max = rankList.Count; i < max; ++i) {
			if (rankList [i].id == LocalPlayer.Get ().playerData.userId) {
				selfRankIndex = i;
				break;
			}
		}

		// 列表
		if (rankList.Count > 3) {
			grid.transform.DestroyChildren ();

			if (selfRankIndex >= 3) {

				// 先倒数10个出来，看自己在不在里面
				showIndexMax = rankList.Count - 1;
				showIndexMin = showIndexMax - 9;

				if (selfRankIndex >= showIndexMin + 2 && selfRankIndex <= showIndexMax) {
					// 这是正常时候.。。。自己上面还要显示两个

				} else {
					// 不在这里面时，需要从自己的位置进行切割
					showIndexMin = selfRankIndex - 2;
					showIndexMax = showIndexMin + 9;
				}

				if (showIndexMin < 3) {
					showIndexMin = 3;
				}
				if (showIndexMax > rankList.Count - 1) {
					showIndexMax = rankList.Count - 1;
				}

			} else {
				showIndexMin = 3;
				showIndexMax = showIndexMin + 9;
				if (showIndexMax > rankList.Count - 1) {
					showIndexMax = rankList.Count - 1;
				}
			}

			// 显示从min到max的
			for (int i = showIndexMin; i <= showIndexMax; ++i) {
				GameObject go = NGUITools.AddChild (grid.gameObject, staticRanks [0]);
				SetBattleRankItem (go, rankList [i], i + 1);
				go.SetActive (true);
				go.name = (i + 1).ToString ();
			}

			grid.Reposition ();
			scrollview.ResetPosition ();

			int offset = 0;
			// 计算偏移量
			if (selfRankIndex - showIndexMin + 1 > 3) {
				// 上方大于3，则偏移掉
				offset = selfRankIndex - showIndexMin + 1 - 3;
			}

			if (showIndexMax - showIndexMin + 1 - offset < 4) {
				// 偏移完之后不足4个，则只偏移到最后一个正好显示
				offset = showIndexMax - showIndexMin - 3;
			}

			if (offset > 0) {
				// 跳转到固定位置
				scrollview.MoveRelative (new Vector3(0, offset * grid.cellHeight + 10, 0));
			}
			//Debug.Log ("自己的index ：" + selfRankIndex);
		}
	}

	/// <summary>
	/// 依次创建scrollview序列
	/// </summary>
	private void FillScrollView (int start, bool more, int count)
	{
		// 每次创建10个
		if (more) {
			for (int i = start; i <= start + count; ++i) {
				if (i > rankList.Count - 1)
					break;

				GameObject go = NGUITools.AddChild (grid.gameObject, template);
				SetBattleRankItem (go, rankList [i], i + 1);
				go.SetActive (true);
				go.name = (i + 1).ToString ();
				showIndexMax++;
			}
		} else {
			for (int i = start; i >= start - count; --i) {
				if (i < 3)
					break;

				GameObject go = NGUITools.AddChild (grid.gameObject, template);
				SetBattleRankItem (go, rankList [i], i + 1);
				go.SetActive (true);
				go.name = (i + 1).ToString ();
				showIndexMin--;
			}
		}

		grid.Reposition ();
	}

	private void OnScrollViewShowLess ()
	{
		if (showIndexMin == 3)
			return;

		FillScrollView (showIndexMin - 1, false, 10);
	}

	private void OnScrollViewShowMore ()
	{
		if (showIndexMax == rankList.Count - 1)
			return;
		
		FillScrollView (showIndexMax + 1, true, 10);
	}

	/// <summary>
	/// 设置结果页
	/// </summary>
	private void SetResultPage ()
	{
		ShowPage (false, false, false, false, true);

		// rank
		resultRank.text = DictionaryDataProvider.Format (716, "?");
		// score
		resultScore.text = selfInfo.score.ToString();
		// mvp
		resultMVP.text = selfInfo.mvp.ToString();

		// 排行榜由另一个页面设置，先设空
		for (int i = 0; i < battleStaticRanks.Length; ++i) {
			SetBattleRankItem (battleStaticRanks [i], null, i + 1);
		}
	}

	/// <summary>
	/// 设置结果页自己排名
	/// </summary>
	private void SetResultSelfRank ()
	{
		resultRank.text = DictionaryDataProvider.Format (716, selfRankIndex + 1);
	}

	/// <summary>
	/// 结果页确定
	/// </summary>
	public void OnResultConfirmClick ()
	{
		
	}
}

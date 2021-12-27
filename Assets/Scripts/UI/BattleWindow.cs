using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Solarmax;





public class UISkillData
{
	public TechniqueEntiy skill;
	public UISlider slider;
	public UISprite icon;
	public UISprite cd;
	public UILabel cost;
	public bool first;
}

public class BattleWindow : BaseWindow
{
	/// <summary>
	/// The background.
	/// </summary>
	public UITexture background = null;
    
	/// <summary>
	/// 飞船数量Label
	/// </summary>
	public UILabel populationLabel = null;
	public UILabel populationValueLabel = null;

	/// <summary>
	/// 投降字
	/// </summary>
	private float giveupCountTime = 0;
	private bool giveupForQuit = false;
	public UILabel giveupLabel = null;

	/// <summary>
	/// 进度条，滑块图，百分比字
	/// </summary>
	public GameObject percentGo = null;
    public GameObject ProcessAram = null;
	public UISprite percentleft = null;
	public UISprite percentright = null;
	public UISprite percetPic = null;
	public UILabel percentLabel = null;
	/// <summary>
	/// 技能图标
	/// </summary>
	public UISlider[] sliders;
	UISkillData[] uiSkillDataList = new UISkillData[3];
	/// <summary>
	/// 能量Label
	/// </summary>
	public UILabel skillTextLabel = null;
	public UILabel skillPowerLabel = null;

	/// <summary>
	/// 触摸精度
	/// </summary>
	public float sensitive = 1;

	/// <summary>
	/// 新的分兵条，长按
	/// </summary>
	public GameObject newProgressGo = null;
	public UISprite newProgressPic = null;
	public UILabel newProgressLabel = null;
	private int newProgressCount = 0;
	private string newProgressTag = string.Empty;

	/// <summary>
	/// 进度条，滑块图宽
	/// </summary>
	private float lineTotalLength = 0;
	/// <summary>
	/// 进度条，滑块图起始移动位置
	/// </summary>
	private Vector3 percentZeroPos = Vector3.zero;

	/// <summary>
	/// 当前的百分比
	/// </summary>
	private float percent = 0;

	private float percentPicWidth = 0;


    public GameObject raceFrame;
    public GameObject playerFrame;

    public GameObject[] racebtn;
	public UISprite[] raceBG;
	public UISprite[] raceIcon;

    public UISprite[] playerBG;
    public UISprite[] playerIcon;
    public UILabel[] playerName;
    public UILabel[] playerSrc;
    public UISprite[] playerFlag;
    public UISprite[] playerBei;
    public UILabel[] nameFlag;
    public UILabel[] dwonFlag;
    public GameObject[] playerEffect;
    public UILabel battleTime;
    public UILabel battleCoundown;


	public GameObject raceTip;
	public UISprite tipIconBG;
	public UISprite tipIcon;

    public UILabel   username;
    public UILabel   raceinfo;
	public UILabel[] skillname;
	public UILabel[] skilldesc;


	public UILabel popLable1;
	public UILabel popLableValue1;
	public UILabel popLableAdd;

	public GameObject skillTip;
	public UILabel skillTipDesc;
	public UILabel skillTipName;
    

	private int battleTimeMax = 0; // PVP最长时间
    private bool bProceesEnd = false;
    private int curAddSpeedSegment = 0;

    /// <summary>
    /// 队伍到空间下表映射关系维护
    /// </summary>
    Dictionary<int, int> mapTeam2Index = new Dictionary<int, int>();
	void Start()
	{
        
		raceTip.SetActive (false);
        battleCoundown.transform.parent.gameObject.SetActive(false);
		HideSkillTip ();
		percent = 1;
		SetPercent ();
        bProceesEnd = false;
		// 设置drag参数的像素修正
		//UIRoot nguiRoot = UISystem.Get ().GetNGUIRoot ();
		//sensitive *= nguiRoot.pixelSizeAdjustment;

        curAddSpeedSegment = 0;
        for( int i = 0; i < racebtn.Length; ++i )
        {
            GameObject go = racebtn[i];
            go.GetComponent<UIEventListener>().onPress += OnPressIcon;
        }

		battleTimeMax = int.Parse(GameVariableConfigProvider.Instance.GetData (4)) * 60;
	}

	public override bool Init ()
	{
		RegisterEvent (EventId.NoticeSelfTeam);
		RegisterEvent (EventId.OnBattleDisconnect);
		RegisterEvent (EventId.RequestUserResult);
		RegisterEvent (EventId.OnTouchBegin);
		RegisterEvent (EventId.OnTouchPause);
		RegisterEvent (EventId.OnTouchEnd);
		RegisterEvent (EventId.OnSelfDied);
		RegisterEvent (EventId.OnPopulationUp);
		RegisterEvent (EventId.OnPopulationDown);

        RegisterEvent (EventId.PlayerGiveUp);
		RegisterEvent (EventId.PlayerDead);
      
		return true;
	}

	/// <summary>
	/// 显示当前飞船数量
	/// </summary>
	public override void OnShow()
	{
		percentPicWidth = percetPic.width;
		
		giveupForQuit = false;
		giveupLabel.text = "放弃";
		string giveupconfig = GameVariableConfigProvider.Instance.GetData (2);
		giveupCountTime = int.Parse (giveupconfig);

		// 设置分兵方式
		InitFightProgressOption ();

		Team team = BattleSystem.Instance.sceneManager.teamManager.GetTeam (BattleSystem.Instance.battleData.currentTeam);
		Color color = team.color;
		color.a = 0.7f;

		// 人口颜色
		populationLabel.color = color;
		populationValueLabel.color = color;

        int current     = 0;
        int currentMax  = 0;
        for (int i = 0; i < team.battleArray.Count; i++ )
        {
            current     += team.battleArray[i].current;
            currentMax  += team.battleArray[i].currentMax;
        }
        populationValueLabel.text = string.Format("{0}/{1}", current, currentMax);

		#if SKILL_MODE
		skillTextLabel.color = color;
		skillPowerLabel.color = color;
		skillTextLabel.enabled = false;
		skillPowerLabel.enabled = false;
		#else
		skillTextLabel.enabled = false;
		skillPowerLabel.enabled = false;
		#endif

		TimerProc ();

		//skill init
		for (int index = 0; index < 3; index++)
		{
			UISkillData uiSkillData = new UISkillData ();
			uiSkillDataList [index] = uiSkillData;

			uiSkillData.slider = sliders[index];
			uiSkillData.slider.GroupIndex = index;
			uiSkillData.icon = uiSkillData.slider.backgroundWidget.GetComponent<UISprite> ();
			uiSkillData.cd = uiSkillData.slider.foregroundWidget.GetComponent<UISprite> ();
			uiSkillData.cost = uiSkillData.slider.GetComponentInChildren<UILabel> ();
			uiSkillData.first = true;
			
			if (uiSkillData.skill == null) {
				uiSkillData.slider.gameObject.SetActive (false);
			}
		}

//		AudioManger.Get ().PlayAudioBG ("Wandering");

		// 隐藏根据地图顺序设置背景偏移值
//		int mapIndex = 0, mapTotal = 0;
//		GetBattleMapIndex (BattleManager.Get ().matchId, out mapIndex, out mapTotal);
//		SetBattleBg (mapIndex, mapTotal);

		UISystem.Get ().ShowWindow ("PopTextWindow");

        ProcessAram.GetComponent<UIEventListener>().onClick += OnSelectProcessAram;
		
	}

    void ShowPlayerInfo()
    {
        mapTeam2Index.Clear();
        raceFrame.SetActive(false);
        playerFrame.SetActive(true);
        Team team = BattleSystem.Instance.sceneManager.teamManager.GetTeam(BattleSystem.Instance.battleData.currentTeam);
        int indx  = 0;
        for (int i = 1; i < 5; i++)
        {
            Team teamTmp = BattleSystem.Instance.sceneManager.teamManager.GetTeam((TEAM)i);
            if (teamTmp.team == team.team)
                continue;

            if (indx >= 3)
                return;

            Color col   = teamTmp.color;
            col.a = 1;
            playerBG[indx].color        = col;
            playerIcon[indx].spriteName = teamTmp.playerData.icon;
            playerName[indx].color      = col;
            playerSrc[indx].color       = col;
            playerBei[indx].color       = col;
            playerSrc[indx].text        = string.Format("{0}", teamTmp.playerData.score);
            playerName[indx].text       = teamTmp.playerData.name;
            playerBei[indx].color       = col;
            mapTeam2Index.Add(i, indx);
            indx++;
        }
    }

    void ShowRaceInfo()
    {
       
    }
    private void OnSelectProcessAram(GameObject go)
    {
        Vector2 lefscr = UICamera.currentCamera.WorldToScreenPoint(percentleft.gameObject.transform.position);
        Vector2 rigscr = UICamera.currentCamera.WorldToScreenPoint(percentright.gameObject.transform.position);
        Vector2 curpos = UICamera.lastEventPosition;

        percent = (curpos.x - lefscr.x) / (rigscr.x - lefscr.x);
        if (percent < 0)
            percent = 0;
        if (percent > 1.0f)
            percent = 1.0f;

        percent = Mathf.Round(percent * 100) / 100;
        SetPercent();
    }


	

	public override void OnHide()
	{
		UISystem.Get ().HideWindow ("PopTextWindow");
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		if (eventId == EventId.NoticeSelfTeam) {

			// 获取当前战斗中所有己方星球的ID
			if (BattleSystem.Instance.battleData.isReplay)
				return;	// 如果重播，则不提示这个窗口

			MapConfig map = MapConfigProvider.Instance.GetData (BattleSystem.Instance.battleData.matchId);

			Team selfTeam = BattleSystem.Instance.sceneManager.teamManager.GetTeam (BattleSystem.Instance.battleData.currentTeam);

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

		} else if (eventId == EventId.OnBattleDisconnect) {
			// 战斗中掉线
			// 此时需要提示，并尝试重连接
			UISystem.Instance.ShowWindow ("ReconnectWindow");

		} else if (eventId == EventId.RequestUserResult) {
			// 玩家在战斗中

			NetMessage.ErrCode code = (NetMessage.ErrCode)args [0];
			if (code == NetMessage.ErrCode.EC_NeedResume) {
				NetSystem.Instance.helper.ReconnectResume (BattleSystem.Instance.GetCurrentFrame () / 5 + 1);
			} else {
				//  如果不是，则进入主页
				//BattleManager.Get ().Release ();
				BattleSystem.Instance.Reset ();
				UISystem.Get ().HideAllWindow ();
				UISystem.Get ().ShowWindow ("CustomSelectWindowNew");
			}
		} else if (eventId == EventId.OnTouchBegin) {
			Node node = (Node)args [0];
			ShowNewProgress1 (true, node);
		} else if (eventId == EventId.OnTouchPause) {
			Node node = (Node)args [0];
			PauseNewProgress1 (node);
		} else if (eventId == EventId.OnTouchEnd) {
			ShowNewProgress1 (false);
		} else if (eventId == EventId.OnSelfDied) {
			// 自己死亡，进入观看
			giveupLabel.text = "退出";
			giveupForQuit = true;
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

        if (eventId == EventId.PlayerGiveUp)
        {
            // 玩家放弃
            TEAM team = (TEAM)args[0];
            OperateTeamGiveUP((int)team);
        }

        if (eventId == EventId.PlayerDead)
        {
            // 玩家死亡
            TEAM team = (TEAM)args[0];
            OperateTeamDead((int)team);
        }

	}

    float fUpdateBattleTime = 0;
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

		if (isPressSkill) {
			pressTime += Time.deltaTime;
			if (pressTime > 1.5f) //按下弹出tips时间为1.5秒
				ShowSkillTip (curSelectSkill);
		}

        //#if SKILL_MODE
        //RefreshIconCoolDown();
        //#endif

        fUpdateBattleTime += Time.deltaTime;
        if (fUpdateBattleTime > 0.5f)
        {
            fUpdateBattleTime = 0;
            UpdateBattleTime();

            UpdateBattleTimeColor();
        }
	}

	private int lastTimeSeconds = -1;
   
    void UpdateBattleTime()
    {
		int now = Mathf.RoundToInt(battleTimeMax - BattleSystem.Instance.sceneManager.GetBattleTime ());
        if (now == lastTimeSeconds)
        {
            if (lastTimeSeconds <= 0 )
                PlayTimeWillEnd(false);
            return;
        }
		
		lastTimeSeconds = now;

        if (lastTimeSeconds < 0)
        {
            lastTimeSeconds = 0;
        }


        battleCoundown.text = lastTimeSeconds.ToString();
        if (!bProceesEnd)
            PlayTimeWillEnd(true);
        bProceesEnd = true;
		
    }
	private void TimerProc()
	{
		Team team       = BattleSystem.Instance.sceneManager.teamManager.GetTeam (BattleSystem.Instance.battleData.currentTeam);
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

	Vector2 totalDrag;
	private void OnProgressDragStart (GameObject go)
	{
		totalDrag = Vector2.zero;
	}

	private void OnProgressDragEnd (GameObject go)
	{
		float x = totalDrag.x * sensitive / lineTotalLength;

		if (x <= -0.05f || x >= 0.05f) {
			int a = (int)(x / 0.1f);
			int b = (int)((x % 0.1f) / 0.05f);
			x = (a + b) * 0.1f;

			totalDrag.x -= x * lineTotalLength;
		}
		else
		{
			x = 0;
			return;
		}

		percent += x;

		if (percent < 0)
			percent = 0;

		if (percent > 1)
			percent = 1;

		percent = Mathf.Round(percent * 100) / 100;

		SetPercent ();
	}

	private void OnProgressDrag(GameObject go, Vector2 delta)
	{
		totalDrag += delta;

		OnProgressDragEnd (null);
	}

	public void OnCloseClick()
	{
		UISystem.Get ().HideAllWindow ();
		UISystem.Get ().ShowWindow ("CustomSelectWindowNew");
	}

	private void SetPercent()
	{
		if (LocalSettingStorage.Get ().fightOption == 1) {
			newProgressPic.fillAmount = percent;
			newProgressLabel.text = string.Format ("{0}%", Mathf.RoundToInt (percent * 100));
		} else {
			float length = lineTotalLength * percent;
			Vector3 pos = percentZeroPos;
			pos.x += length;
			int width = (int)((lineTotalLength * percent) + 2 - percentPicWidth / 2);
			percentleft.width = width;
			width = (int)((lineTotalLength * (1 - percent)) + 2 - percentPicWidth / 2);
			percentright.width = width;
			percentleft.gameObject.SetActive (true);
			percentright.gameObject.SetActive (true);
			if (percent == 0)
				percentleft.gameObject.SetActive (false);
			else if (percent == 1)
				percentright.gameObject.SetActive (false);
			//percentleft.fillAmount = percent - 0.06f;
			//percetright.fillAmount = 1f - percent - 0.06f;
			percetPic.transform.localPosition = pos;
			// 设置百分比数字
			percentLabel.text = string.Format ("{0}%", Mathf.RoundToInt (percent * 100));
		}

		// 设置游戏内飞船移动百分比
		BattleSystem.Instance.battleData.sliderRate = percent;
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
	/// <param name="mapId">Map identifier.</param>
	public static void SetBattleBg (int mapIndex, int mapTotalCount)
	{
		GameObject bgParent = GameObject.Find ("Battle/BG");
        float x = -58.4f / mapTotalCount;
		x *= mapIndex;
		bgParent.transform.localPosition = new Vector3 (x, 0, 0);
	}

	public void OnSkillClicked(int index)
	{
		if (index > uiSkillDataList.Length)
			return;

		UISkillData uiSkillData = uiSkillDataList [index];

		if (uiSkillData.slider.value < 1f)
			return;


		Debug.Log ("Click skill " + uiSkillData.skill.proto.name);

		// 发送技能
		NetSystem.Instance.helper.SendSkillPacket (uiSkillData.skill.proto.id);
		uiSkillData.slider.value = 0;
	}

	int curSelectSkill = 0;
	bool isPressSkill = false;
	float pressTime = 0f;

	public void OnPressSkill(int index)
	{
		Debug.Log ("OnPressSkill:" + index.ToString ());
		curSelectSkill = index;
		isPressSkill = true;
		pressTime = 0f;
	}

	public void OnReleaseSkill()
	{
		isPressSkill = false;
		HideSkillTip ();
		if (pressTime < 1.0f)
			OnSkillClicked (curSelectSkill);
	}
		
	void ShowSkillTip(int index)
	{
		if (index > uiSkillDataList.Length)
			return;
		UISkillData uiSkillData = uiSkillDataList [index];

		Debug.Log ("ShowSkillTip:" + index.ToString());
		isPressSkill = false;
		skillTip.transform.parent = uiSkillData.slider.transform;
		skillTip.transform.localPosition = Vector3.zero;
		skillTip.transform.localScale = Vector3.one;
		skillTip.SetActive (true);

		skillTipDesc.text = uiSkillData.skill.proto.desc;
		skillTipName.text = uiSkillData.skill.proto.name;
	}

	void HideSkillTip()
	{
		skillTip.SetActive (false);
	}

	/// <summary>
	/// 初始化分兵状态
	/// </summary>
	private void InitFightProgressOption()
	{
		percentGo.SetActive (false);
		newProgressGo.SetActive (false);


		int option = LocalSettingStorage.Get ().fightOption;
		if (option == 1) {
			// 长按
			ShowNewProgress1 (false);

		} else {
			// 普通进度条
			// 进度条滑块绑定事件
			UIEventListener listener = percetPic.gameObject.GetComponent<UIEventListener> ();
			listener.onDrag = OnProgressDrag;
			listener.onDragStart = OnProgressDragStart;

			// line total Length
			lineTotalLength = 1200;

			percentZeroPos = new Vector3 (percetPic.transform.localPosition.x - (lineTotalLength/2),
				percetPic.transform.localPosition.y, 0);

			percentGo.SetActive (true);
		}
	}

	/// <summary>
	/// 新的分兵方式，长按
	/// </summary>
	private void ShowNewProgress1(bool show, Node node = null)
	{
		if (LocalSettingStorage.Get ().fightOption != 1)
			return;

		if (newProgressGo == null)
			return;

		if (show) {
			newProgressGo.gameObject.SetActive (true);
			Vector3 pos = Camera.main.WorldToScreenPoint (node.GetPosition ());
			pos.x -= Screen.width / 2;
			pos.y -= Screen.height / 2;
			newProgressGo.transform.localPosition = pos;
			newProgressPic.transform.localScale = Vector3.one * node.GetScale ();

			SetPercent ();

			if (!IsInvoking ("UpdateNewProgress1")) {
				InvokeRepeating ("UpdateNewProgress1", 0.1f, 0.05f);
			}

			newProgressTag = node.tag;

		} else {
			newProgressGo.SetActive (false);
			CancelInvoke ("UpdateNewProgress1");
			newProgressTag = string.Empty;
			newProgressCount = 0;
			percent = 1.0f;
		}
	}

	private void PauseNewProgress1 (Node node)
	{
		if (LocalSettingStorage.Get ().fightOption != 1)
			return;
		if (newProgressTag.Equals (node.tag) && IsInvoking ("UpdateNewProgress1")) {
			CancelInvoke ("UpdateNewProgress1");
		}
	}

	private void UpdateNewProgress1()
	{
		if (LocalSettingStorage.Get ().fightOption != 1)
			return;
		
		if (newProgressCount >= 40) {
			newProgressCount = 0;
		}

		percent = 1 - newProgressCount * 0.025f;

		SetPercent ();

		newProgressCount++;
	}

	public void GiveUpOnClicked()
	{
		if (!giveupForQuit) {
			UISystem.Get ().ShowWindow ("CommonDialogWindow");
			EventSystem.Instance.FireEvent (EventId.OnCommonDialog, 
				3, DictionaryDataProvider.GetValue (801), new EventDelegate (GiveUp), null, Mathf.CeilToInt(giveupCountTime - BattleSystem.Instance.sceneManager.GetBattleTime()));

		} else {
			// 退出
			BattleSystem.Instance.OnPlayerDirectQuit ();
		}
	}

	void GiveUp()
	{
		BattleSystem.Instance.PlayerGiveUp ();
	}

    private void OnPressIcon(GameObject obj, bool isPressed )
	{

        if( !isPressed )
        {
            ReleaseIcon();
            return;
        }
        AudioManger.Get().PlayEffect("onClick");
	}

	private void ReleaseIcon()
	{
		raceTip.SetActive (false);
	}

    private int nPlayEffectTimes = 0;
    private void UpdateBattleTimeColor( )
    {
        int curSegment = 0;
        if( curSegment != curAddSpeedSegment && curSegment > curAddSpeedSegment )
        {
            curAddSpeedSegment = curSegment;
            if( curSegment == 1 )
                battleTime.color = new Color(1, 1, 0);
            if (curSegment == 2)
                battleTime.color = new Color(1, 0, 0);
           
            PlayTimeEffectOut();
        }

        if( nPlayEffectTimes == 1 )
        {
            PlayTimeEffectIn();
        }
    }


    private void PlayTimeEffectOut()
    {
        nPlayEffectTimes = 0;
        TweenScale ts = battleTime.gameObject.GetComponent<TweenScale>();
        if (ts == null)
        {
            ts = battleTime.gameObject.AddComponent<TweenScale>();
        }

        ts.ResetToBeginning();
        ts.from = Vector3.one;
        ts.to = Vector3.one * 1.5f;
        ts.duration = 0.5f;
        ts.SetOnFinished(() =>
        {
            nPlayEffectTimes++;
        });
        ts.Play(true);
    }

    private void PlayTimeEffectIn()
    {
        nPlayEffectTimes++;
        TweenScale ts = battleTime.gameObject.GetComponent<TweenScale>();
        if (ts == null)
        {
            ts = battleTime.gameObject.AddComponent<TweenScale>();
        }

        ts.ResetToBeginning();
        ts.onFinished.Clear();
        ts.from     = Vector3.one * 1.5f;
        ts.to       = Vector3.one;
        ts.duration = 0.5f;
        ts.Play(true);
    }

	/// <summary>
	/// 倒计时快结束时时间动画
	/// </summary>
	private void PlayTimeWillEnd ( bool bStart )
	{
        if (bStart) { 
            battleTime.transform.parent.gameObject.SetActive(false);
            battleCoundown.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            battleCoundown.transform.parent.gameObject.SetActive(false);
        }
	}

    /// <summary>
    /// 处理队伍投降, 现在只处理
    /// </summary>
    private void OperateTeamGiveUP(int team)
    {
        int idex = -1;
        bool ret = mapTeam2Index.TryGetValue( team, out idex );
        if( ret && idex >= 0 && idex < playerFlag.Length )
        {
            playerFlag[idex].gameObject.SetActive(true);
            nameFlag[idex].gameObject.SetActive(true);
            nameFlag[idex].text  = DictionaryDataProvider.GetValue(803);
            dwonFlag[idex].text = DictionaryDataProvider.GetValue(803);
            playerEffect[idex].SetActive(true);
        }
    }

    /// <summary>
    /// 处理队伍死亡
    /// </summary>
    private void OperateTeamDead(int team)
    {
        int idex = -1;
        Color32 color = new Color32(0xcc, 0xcc, 0xcc, 0xFF);
        bool ret = mapTeam2Index.TryGetValue(team, out idex);
        if (ret && idex >= 0 && idex < playerFlag.Length)
        {
            playerBG[idex].color    = color;
            playerName[idex].color  = color;
            playerSrc[idex].color   = color;
            playerBei[idex].color   = color;
            nameFlag[idex].gameObject.SetActive(true);
            nameFlag[idex].text     = DictionaryDataProvider.GetValue(802);
            playerFlag[idex].enabled= false;
            dwonFlag[idex].text = DictionaryDataProvider.GetValue(802);
            playerEffect[idex].SetActive(true);
        }
    }
}


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NetMessage;
using Plugin;
using Solarmax;
using System.IO;

/// <summary>
/// 网络相关
/// </summary>
public class PacketHelper
{
	void Log (string fmt, params object[] args)
	{
		string str = string.Format (fmt, args);
		LoggerSystem.Instance.Debug (str);
	}

	/// <summary>
	/// 注册网络消息
	/// </summary>
	public void RegisterAllPacketHandler ()
	{
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCMatch,                this.OnMatch);
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCMatchFail,            this.OnMatchFailed);
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCStartBattle,          this.OnNetStartBattle);	//
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCFrame,                this.OnNetFrame);		//
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCFinishBattle,         this.OnFinishBattle);	//
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCGetUserData,          this.OnRequestUser);
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCCreateUserData,       this.OnCreateUser);
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCPong,                 this.OnPong);
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCLoadRank,             this.OnLoadRank);
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCReady,                this.OnReady);					//

		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCFriendFollow,        this.OnFriendFollow);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCFriendLoad,          this.OnFriendLoad);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCFriendSearch,        this.OnFriendSearch);

		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCBattleReportLoad,    this.OnBattleReportLoad);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCBattleReportPlay,    this.onBattleReportPlay);

		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCBattleResume,        this.OnBattleResume);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCJoinRoom,            this.OnRequestJoinRoom); 
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCCreateRoom,          this.OnRequestCreateRoom);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCQuitRoom,            this.OnRequestQuitRoom); 
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCRoomRefresh,         this.RoomRefresh);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCRoomListRefresh,     this.RoomListRefresh);

		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCMatch2CurNum,        this.OnMatch2CurNum);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCStartMatch2,         this.ONMatchGameBack);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCPackUpdate,          this.OnPackUpdate);


		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCSingleMatch,         this.OnSingleMatch);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCSetCurLevel,         this.OnSetCurrentLevelResult);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCRename,              this.OnChangeName);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCClientStorageLoad,   this.OnClientStorageLoad);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCClientStorageSet,    this.OnClientStorageSet);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCResume,              this.OnReconnectResumeResult);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCKickUserNtf,         this.OnKickUserNtf);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCNotify,              this.OnServerNotify);

		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCStartMatchReq,       this.OnStartMatchRequest);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCMatchInit,           this.OnMatchInit);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCMatchUpdate,         this.OnMatchUpdate);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCQuitMatch,           this.OnQuitMatch);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCMatchComplete,       this.OnMatchComplete);
		NetSystem.Instance.GetConnector ().RegisterHandler ((int)NetMessage.MsgId.ID_SCMatchPos,            this.OnMatchPos);


		//关卡有关的
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCLoadChapters,         this.OnLoadChapters);
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCLoadChapter,          this.OnLoadOneChapter);
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCStartLevel,           this.OnStartLevel);
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCSetLevelStar,         this.OnSetLevelStar);
		NetSystem.Instance.GetConnector ().RegisterHandler((int)NetMessage.MsgId.ID_SCIntAttr,              this.OnIntAttr);
	}

	
    /// --------------------------------------------------------------------------------------------------
    /// <summary>
    /// 连接到服务器， 登录界面和短线重连界面调用
    /// </summary>
    /// --------------------------------------------------------------------------------------------------
	public IEnumerator ConnectServer ()
	{
		string addr = string.Empty;
		ConfigSystem.Instance.TryGetConfig ("R_SERVER", out addr);

		#if SERVER
		{
			ConfigSystem.Instance.TryGetConfig ("Referee_SERVER", out addr);
		}
		#else

		//新服务器
		#if R_SERVER
		ConfigSystem.Instance.TryGetConfig ("R_SERVER", out addr);
		#elif L_SERVER
		//老服务器
		ConfigSystem.Instance.TryGetConfig ("L_SERVER", out addr);
		#elif DEV_SERVER
		//开发服务器
		ConfigSystem.Instance.TryGetConfig ("DEV_SERVER", out addr);
		#endif

		#endif

		string[] addrs = addr.Split (':');
		string ip = addrs [0];
		int port = int.Parse (addrs [1]);

        #if UNITY_EDITOR
		LocalSettingStorage.Get ().ip = ip;
        #else
		LocalSettingStorage.Get ().ip = ip;
        #endif

		NetSystem.Instance.Connect (LocalSettingStorage.Get ().ip, port);
		while (NetSystem.Instance.GetConnector ().GetConnectStatus () == ConnectionStatus.CONNECTING)
			yield return 1;
		if (NetSystem.Instance.GetConnector ().GetConnectStatus () == ConnectionStatus.CONNECTED) 
        {
			NetSystem.Instance.ping.Pong (100);
			EventSystem.Instance.FireEvent (EventId.NetworkStatus, true);
		} 
        else 
        {
			#if !SERVER
			Debug.LogError ("连接服务器失败");
			Tips.Make (Tips.TipsType.FlowUp, "连接服务器失败", 1.0f);
			#endif

			NetSystem.Instance.Close ();
			NetSystem.Instance.ping.Pong (-1);
			EventSystem.Instance.FireEvent (EventId.NetworkStatus, false);
		}
	}

	/// <summary>
	/// 开始匹配
	/// </summary>
	public void Match (bool single, bool team, string matchId)
	{
		NetMessage.CSMatch bd = new NetMessage.CSMatch();
		if (single) {
			bd.type =  NetMessage.BattleType.Melee;
		} else if (team) {
            bd.type = NetMessage.BattleType.Group2v2;
		} else {
			BattleSystem.Instance.battleData.matchId = matchId;
			bd.match_id = matchId;
		}

		NetSystem.Instance.Send ((int)NetMessage.MsgId.ID_CSMatch, bd);
	}

	/// <summary>
	/// 匹配响应
	/// </summary>
	/// <param name="msgID">Message I.</param>
	/// <param name="msgBody">Message body.</param>
	void OnMatch (int msgID, PacketEvent msgBody)
	{
        MemoryStream ms  = msgBody.Data as MemoryStream;
        SCMatch response = ProtoBuf.Serializer.Deserialize<NetMessage.SCMatch>(ms) ;
		EventSystem.Instance.FireEvent (EventId.OnMatch, response.count_down);
	}

	
	/// <summary>
	/// 匹配失败
	/// </summary>
	private void OnMatchFailed (int msgId, PacketEvent msg)
	{
		#if !SERVER
		// 收到此包意味着服务器已经被从匹配队列删除，所以关闭页面
		UISystem.Get ().HideAllWindow ();
		UISystem.Get ().ShowWindow ("StartWindow");
		#endif
	}

	/// <summary>
	/// 收到ready之后展示玩家信息，之后再收到startbattle
	/// </summary>
	public void OnReady ( NetMessage.SCReady proto)
	{

        BattleSystem.Instance.SetPlayMode (true, false);
		if (proto.match_type == MatchType.MT_League) {
			BattleSystem.Instance.battleData.gameType = GameType.League;
		} else if (proto.match_type == MatchType.MT_Ladder) {
			BattleSystem.Instance.battleData.gameType = GameType.PVP;
		} else if (proto.match_type == MatchType.MT_Room) {
			BattleSystem.Instance.battleData.gameType = GameType.PVP;
		} else {
			BattleSystem.Instance.battleData.gameType = GameType.PVP;
		}

		// 地图、设置随机种子
		BattleSystem.Instance.battleData.matchId	= proto.match_id;
		BattleSystem.Instance.battleData.rand.seed	= proto.random_seed;

		//组队信息
		int[] teamGroup = new int[proto.data.Count];
		//初始化组队列表
		for (int i = 0; i < teamGroup.Length; i++) {
			teamGroup [i] = i;
		}
		//解析组队信息
		if (!string.IsNullOrEmpty (proto.group)) {
			string[] strGroup = proto.group.Split ('|');
			for (int i = 0; i < strGroup.Length; i++) {
				string[] teams = strGroup [i].Split (',');
				for (int j = 0; j < teams.Length; j++) {
					int userIndex = int.Parse (teams [j]);
					teamGroup [userIndex] = i;
				}
			}
			BattleSystem.Instance.battleData.teamFight = true;
		} else {
			BattleSystem.Instance.battleData.teamFight = false;
		}

		bool containLocalTeam = false;

		// 获取玩家数据
		NetMessage.UserData ud;
		int index = 0;
		// 正常选手
		for (; index < proto.data.Count; ++index) {
			ud = proto.data[index];

			// 队伍数据
			TEAM team = (TEAM)(index + 1);
			Team t = BattleSystem.Instance.sceneManager.teamManager.GetTeam (team);
			// 设置组队group
			t.groupID = teamGroup [index];
			// 设置队伍数据
			t.playerData.Init (ud);
			// 所有队伍都初始化ai数据
			BattleSystem.Instance.sceneManager.aiManager.AddAI (t, AIType.FriendSmart, t.playerData.level, 3 );

			if (ud.userid > 0) {
				if (LocalPlayer.Get ().playerData.userId == ud.userid) {
					BattleSystem.Instance.battleData.currentTeam = team;
					containLocalTeam = true;
				}
				t.aiEnable = false;
			} else {
				#if !SERVER
				t.playerData.name = AIManager.GetAIName (t.playerData.userId);
				t.playerData.icon = AIManager.GetAIIcon (t.playerData.userId);
				#endif
				t.aiEnable = true;

				Log ("加入了AI：{0}, 类型：{1}", t.playerData.name, t.aiData.aiType);
			}
		}

		if (!containLocalTeam) {
			// 不包含当前队伍，则认为当前队伍为空
			// 此时，有两种情况，A播放热门战报，B观战
			BattleSystem.Instance.battleData.gameState = GameState.Watcher;
		} else {
			BattleSystem.Instance.battleData.gameState = GameState.Game;
		}

		// 预加载资源
		AssetManager.Get ().LoadBattleResources ();
		// 预加载特效
		EffectManager.Get ().PreloadEffect ();
	}


    
    public void OnReady(int msgId, PacketEvent msgBody)
    {
        MemoryStream ms = msgBody.Data as MemoryStream;
        NetMessage.SCReady proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCReady>(ms);
        OnReady(proto);
    }

    /////////////////////////////////////
    /// 此处在OnReady之后应该增加一个客户端加载完成后发送给服务器的消息，告知服务器本地准备完毕，可以开始游戏
    /// 作用：在速度慢的机器上，能够让大家在321倒计时界面不卡顿，顺利并且同时进入。
    /////////////////////////////////////

    /// <summary>
    /// 战斗开始
    /// </summary>
    /// <param name="msgID">Message I.</param>
    /// <param name="msgBody">Message body.</param>
    public void OnNetStartBattle(int msgID, PacketEvent msgBody)
    {
        MemoryStream ms = msgBody.Data as MemoryStream;
        NetMessage.SCStartBattle proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCStartBattle>(ms);
        OnNetStartBattle(proto);
    }

    public void OnNetStartBattle(NetMessage.SCStartBattle proto)
    {
        BattleSystem.Instance.sceneManager.Reset();

        // 获取玩家数据
        MapConfig matchMap = MapConfigProvider.Instance.GetData(BattleSystem.Instance.battleData.matchId);
        List<int> userIdList = new List<int>();
        List<int> teamList = new List<int>();
        for (int i = 0; i < matchMap.player_count; ++i)
        {
            TEAM team = (TEAM)(i + 1);
            Team t = BattleSystem.Instance.sceneManager.teamManager.GetTeam(team);
            t.StartTeam();
            teamList.Add(i + 1);
            userIdList.Add(t.playerData.userId);
        }


        /// 随机队伍到不同的星球
        // 创建地图
        BattleSystem.Instance.sceneManager.CreateScene(teamList, false);
        if (BattleSystem.Instance.battleData.useAI)
        {
            BattleSystem.Instance.sceneManager.aiManager.Start(1);
        }
        else
        {
            BattleSystem.Instance.battleData.useAI = true;
        }

        UISystem.Get().HideAllWindow();
        UISystem.Get().ShowWindow("PreviewWindow");

        // lockstep 模式
        BattleSystem.Instance.lockStep.replay = BattleSystem.Instance.battleData.isReplay;
        BattleSystem.Instance.lockStep.playSpeed = 1;
    }

    /// <summary>
    /// 网络帧同步
    /// </summary>
    public void OnNetFrame (int msgID, PacketEvent msgBody )
	{
        MemoryStream ms = msgBody.Data as MemoryStream;
        NetMessage.SCFrame proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCFrame>(ms);
        OnNetFrame(proto);
	}

    public void OnNetFrame(NetMessage.SCFrame frame)
    {
        BattleSystem.Instance.OnRecievedFramePacket(frame);
        return;
    }

    /// <summary>
    /// 结束战斗回复
    /// </summary>
    void OnFinishBattle (int msgID, PacketEvent msgBody )
	{
        MemoryStream ms = msgBody.Data as MemoryStream;
        NetMessage.SCFinishBattle proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCFinishBattle>(ms);
		for (int i = 0; i < proto.users.Count; ++i) {
			Team t = BattleSystem.Instance.sceneManager.teamManager.GetTeam (proto.users [i]);
			if (t == null)
				continue;
			t.scoreMod = proto.score_mods [i];
			t.resultOrder = -3 + i;
			t.resultRank = proto.rank [i];
			t.resultEndtype = proto.end_type [i];
			if (proto.mvp_num.Count > 0) {
				t.leagueMvp = proto.mvp_num [i];
			}
		}

		EventSystem.Instance.FireEvent (EventId.OnFinished, proto);
	}



	/// <summary>
	/// 发送移动消息
	/// </summary>
	public void SendMoveMessaeg (Node from, Node to, float rate = -1f)
	{

		if (BattleSystem.Instance.battleData.gameState != GameState.Game)
			return;

		if (from == null || to == null)
			return;

		int shipNum = from.GetShipCount ((int)BattleSystem.Instance.battleData.currentTeam);
		if (shipNum == 0)
			return;
		FramePacket packet = new FramePacket ();
		packet.type = 0;
		packet.move = new MovePacket ();
		packet.move.from = from.tag;
		packet.move.to = to.tag;
		packet.move.rate = rate == -1f ? BattleSystem.Instance.battleData.sliderRate : rate;

		byte[] byteString           = Json.EnCodeBytes(packet);
		NetMessage.PbFrame pb       = new NetMessage.PbFrame ();
		NetMessage.CSFrame build    = new NetMessage.CSFrame ();

		pb.content  = byteString;
		build.frame = pb;
		NetSystem.Instance.Send ((int)NetMessage.MsgId.ID_CSFrame, build);
	}

	
	/// <summary>
	/// 发送技能网络包
	/// </summary>
	public void SendSkillPacket (int skillId)
	{
		if (BattleSystem.Instance.battleData.gameState != GameState.Game)
			return;

		FramePacket packet = new FramePacket ();
		packet.type = 1;
		packet.skill = new SkillPacket ();
		packet.skill.skillID = skillId;
		packet.skill.from = BattleSystem.Instance.battleData.currentTeam;
		packet.skill.to = TEAM.Neutral;
		packet.skill.tag = string.Empty;

		byte[] byteString = Json.EnCodeBytes(packet);

		NetMessage.PbFrame pb       = new NetMessage.PbFrame ();
		NetMessage.CSFrame build    = new NetMessage.CSFrame ();

		pb.content = byteString;
		build.frame= pb;

		NetSystem.Instance.Send ((int)NetMessage.MsgId.ID_CSFrame, build);
	}

	/// <summary>
	/// 向服务器请求玩家数据，如果本地无账号则新建账号
	/// </summary>
	public void RequestUser (bool needChangeAccount = false)
	{
		string account = LocalPlayer.Get ().GetLocalAccount ();
		if (string.IsNullOrEmpty (account)) 
        {
			account = LocalPlayer.Get ().GenerateLocalAccount ();
		} 
        if (needChangeAccount) 
        {
			account = LocalPlayer.Get ().GenerateLocalAccount (true);
		}

		NetMessage.CSGetUserData proto = new NetMessage.CSGetUserData ();
		proto.account       = account;
		proto.app_version   = UpdateSystem.Instance.GetAppVersion ();
		proto.imei_md5      = EngineSystem.Instance.GetUUID ();
		proto.channel       = "Test";
		proto.device_model  = EngineSystem.Instance.GetDeviceModel ();
		proto.os_version    = EngineSystem.Instance.GetOS ();
		NetSystem.Instance.Send ((int)NetMessage.MsgId.ID_CSGetUserData, proto);
	}

	/// <summary>
	/// 请求玩家数据的数据处理
	/// </summary>
	private void OnRequestUser (int msgId, PacketEvent msg)
	{
        MemoryStream ms                = msg.Data as MemoryStream;
        NetMessage.SCGetUserData proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCGetUserData>(ms); ;
		if (proto.errcode == ErrCode.EC_Ok || proto.errcode == ErrCode.EC_NeedResume) 
        {
            // 如果成功，则包含玩家数据
			NetMessage.UserData ud = proto.data;
			// 这样子就把数据赋值到内存中吧，放在BattleManager中存着先
			LocalPlayer.Get ().playerData.Init (ud);
			LocalPlayer.Get ().isAccountTokenOver = false;
            LocalPlayer.Get ().InitLocalPlayer();
		}

		if (proto.now > 0 ) 
        {
			// 设置服务器时间

			TimeSystem.Instance.SetServerTime (proto.now );

		}
		EventSystem.Instance.FireEvent (EventId.RequestUserResult, proto.errcode);
	}

	/// <summary>
	/// 注册新玩家
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="iconPath">Icon path.</param>
	public void CreateUser (string name, string iconPath)
	{
		string account = LocalPlayer.Get ().GetLocalAccount ();
		if (string.IsNullOrEmpty (account)) {
			Debug.LogError ("注册时本地账号为空");
		}

		NetMessage.CSCreateUserData proto = new CSCreateUserData ();
		proto.account   = account;
		proto.name      = name;
		proto.icon      = iconPath;
		NetSystem.Instance.Send<NetMessage.CSCreateUserData> ((int)NetMessage.MsgId.ID_CSCreateUserData, proto);
	}

	/// <summary>
	/// 创建玩家的数据处理
	/// </summary>
	/// <param name="msgId">Message identifier.</param>
	/// <param name="msg">Message.</param>
	private void OnCreateUser (int msgId, PacketEvent msg)
	{

        MemoryStream ms                   = msg.Data as MemoryStream;
        NetMessage.SCCreateUserData proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCCreateUserData>(ms); ;
		if (proto.errcode == ErrCode.EC_Ok)
        {
            // 如果成功，则包含玩家数据
			NetMessage.UserData ud = proto.data;
			LocalPlayer.Get ().playerData.Init (ud);

			// 注册统计
			//ThirdPartySystem.Instance.OnRegister ();
		}

		EventSystem.Instance.FireEvent (EventId.CreateUserResult, proto.errcode);
	}



	/// <summary>
	/// 切换头像
	/// </summary>
	/// <param name="iconPath">Icon path.</param>
	public void ChangeIcon (string iconPath)
	{
		// 设置本地的头像
		LocalPlayer.Get ().playerData.icon = iconPath;
		NetMessage.CSSetIcon proto = new NetMessage.CSSetIcon ();
		proto.icon = iconPath;
		NetSystem.Instance.Send<NetMessage.CSSetIcon>((int)NetMessage.MsgId.ID_CSSetIcon, proto);
	}


	public void PingNet ()
	{
		NetMessage.CSPing proto = new CSPing ();
		proto.timestamp         = DateTime.Now.ToBinary ();
		NetSystem.Instance.Send<NetMessage.CSPing> ((int)NetMessage.MsgId.ID_CSPing, proto);
	}

	/// <summary>
	/// 心跳检测
	/// </summary>
	/// <param name="msgId">Message identifier.</param>
	/// <param name="msg">Message.</param>
	private void OnPong (int msgId, PacketEvent msg)
	{
        MemoryStream ms         = msg.Data as MemoryStream;
        NetMessage.SCPong proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCPong>(ms);
		TimeSpan ts             = DateTime.Now - DateTime.FromBinary (proto.timestamp);
		float millseconds       = (float)ts.TotalMilliseconds;
		// 发送给界面
		NetSystem.Instance.ping.Pong (millseconds);
		EventSystem.Instance.FireEvent (EventId.PingRefresh, millseconds);
	}

	/// <summary>
	/// 请求排行数据
	/// </summary>
	/// <param name="start">Start.</param>
	public void LoadRank (int start)
	{
		NetMessage.CSLoadRank proto = new CSLoadRank();
		proto.start = start;
		NetSystem.Instance.Send<NetMessage.CSLoadRank> ((int)NetMessage.MsgId.ID_CSLoadRank, proto);
	}

	private void OnLoadRank (int msgId, PacketEvent msgBody)
	{

        MemoryStream ms             = msgBody.Data as MemoryStream;
		NetMessage.SCLoadRank proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCLoadRank>(ms);

		List <NetPlayer> rankData = new List<NetPlayer> ();
		for (int i = 0; i < proto.data.Count; ++i)
        {
            NetPlayer pd = new NetPlayer();
			pd.Init (proto.data [i]);
			rankData.Add (pd);
		}

		EventSystem.Instance.FireEvent (EventId.LoadRankList, rankData, proto.start, proto.self);
	}

	/// <summary>
	/// 关注好友，follow是否关注
	/// </summary>
	public void FriendFollow (int userId, bool follow)
	{
		NetMessage.CSFriendFollow proto = new CSFriendFollow ();
		proto.userid    = userId;
		proto.follow    = follow;
		NetSystem.Instance.Send<NetMessage.CSFriendFollow> ((int)NetMessage.MsgId.ID_CSFriendFollow, proto);
	}

	private void OnFriendFollow (int msgId, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
		NetMessage.SCFriendFollow proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCFriendFollow>(ms);
		if (proto.err == ErrCode.EC_Ok) {
            SimplePlayerData d = new SimplePlayerData();
            d.Init(proto.data);
            if (proto.follow)
            {
                bool bFollow = FriendDataHandler.Get().IsIsFollowEX(d.userId);
                FriendDataHandler.Get().AddMyFollow(d, bFollow);
            }
            else
            {
                FriendDataHandler.Get().DelMyFollow(d);
                FriendDataHandler.Get().SetMutualFollow(d.userId, false);
            }
        }
		EventSystem.Instance.FireEvent (EventId.OnFriendFollowResult, proto.userid, proto.follow, proto.err);
	}

	/// <summary>
	/// 加载关注列表，cared：true表示自己关注的人，false表示关注自己的人
	/// </summary>
	public void FriendLoad (int start, bool myfollow)
	{
		NetMessage.CSFriendLoad proto = new CSFriendLoad ();
		proto.start     = start;
		proto.myfollow  = myfollow;
		NetSystem.Instance.Send<NetMessage.CSFriendLoad> ((int)NetMessage.MsgId.ID_CSFriendLoad, proto);
	}

	private void OnFriendLoad (int msgId, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
		NetMessage.SCFriendLoad proto   = ProtoBuf.Serializer.Deserialize<NetMessage.SCFriendLoad>(ms);
		for (int i = 0; i < proto.data.Count; ++i) {
			SimplePlayerData d = new SimplePlayerData ();
			d.Init (proto.data [i]);
            if (proto.myfollow)
            {
                FriendDataHandler.Get().AddMyFollow(d, proto.follow_status[i]);

            }
            else
            {
                FriendDataHandler.Get().AddFollower(d, proto.follow_status[i]);
            }
		}

		EventSystem.Instance.FireEvent (EventId.OnFriendLoadResult, proto.start, proto.myfollow);
	}

	public void FriendSearch (string name, int userId)
	{
		NetMessage.CSFriendSearch proto = new CSFriendSearch ();
		proto.name      = name;
		proto.userid    = userId;
		NetSystem.Instance.Send<NetMessage.CSFriendSearch> ((int)NetMessage.MsgId.ID_CSFriendSearch, proto);
	}

	private void OnFriendSearch (int msgId, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
		NetMessage.SCFriendSearch proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCFriendSearch>(ms);

		SimplePlayerData data = null;
		bool follow = false;
        int follownum = 0;
        int fensinum  = 0;
        int mvpnum = 33;
        int battlenum = 89;
		if (proto.data != null ) {
			data        = new SimplePlayerData ();
			data.Init (proto.data);
			follow      = proto.followed;
            follownum   = proto.following_count;
            fensinum    = proto.followers_count;
            mvpnum      = proto.data.mvp_count;
            battlenum   = proto.data.battle_count;
		}
        EventSystem.Instance.FireEvent(EventId.OnFriendSearchResult, data, follow, follownum, fensinum, mvpnum, battlenum );
	}


	/// <summary>
	/// 请求战报
	/// </summary>
	public void BattleReportLoad (bool self, int start)
	{
		NetMessage.CSBattleReportLoad proto = new CSBattleReportLoad ();
		proto.self  = self;
		proto.start = start;
		NetSystem.Instance.Send<NetMessage.CSBattleReportLoad>((int)NetMessage.MsgId.ID_CSBattleReportLoad, proto);
	}

	/// <summary>
	/// 加载战报回复
	/// </summary>
	/// <param name="msgid">Msgid.</param>
	/// <param name="msg">Message.</param>
	private void OnBattleReportLoad (int msgid, PacketEvent msg)
	{
        MemoryStream ms                     = msg.Data as MemoryStream;
		NetMessage.SCBattleReportLoad proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCBattleReportLoad>(ms);
		List<BattleReportData> reportList = new List<BattleReportData> ();
		for (int i = 0; i < proto.report.Count; ++i) {
			BattleReportData brd = new BattleReportData ();
			brd.Init (proto.report[i]);
			reportList.Add (brd);
		}

		EventSystem.Instance.FireEvent (EventId.OnBattleReportLoad, proto.self, proto.start, reportList);
	}

	/// <summary>
	/// 请求播放战报
	/// </summary>
	public void BattleReportPlay (BattleReportData brd)
	{
		BattleSystem.Instance.replayManager.reportData = brd;

		NetMessage.CSBattleReportPlay proto = new CSBattleReportPlay ();
		proto.battleid      = brd.id;
		NetSystem.Instance.Send<NetMessage.CSBattleReportPlay>((int)NetMessage.MsgId.ID_CSBattleReportPlay, proto);
	}

	/// <summary>
	/// 播放战报回复
	/// </summary>
	/// <param name="msgid">Msgid.</param>
	/// <param name="msg">Message.</param>
	public void onBattleReportPlay (int msgid, PacketEvent msg)
	{
        MemoryStream ms                     = msg.Data as MemoryStream;
		NetMessage.SCBattleReportPlay proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCBattleReportPlay>(ms);
		PbSCFrames frames                   = proto.report;

		AssetManager.Get ().LoadBattleResources ();
		EventSystem.Instance.FireEvent (EventId.OnBattleReportPlay, frames);
	}

	
	

	/// <summary>
	/// 返回战斗包，这个包中可能包含ready和start，但是不是必须，需要根据当前状态进行判断进行何种操作。
	/// </summary>
	/// <param name="msgid">Msgid.</param>
	/// <param name="msg">Message.</param>
	public void OnBattleResume (int msgid, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
		NetMessage.SCBattleResume proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCBattleResume>(ms);
		PbSCFrames frames               = proto.report;
		AssetManager.Get ().LoadBattleResources ();

		// 此时需要判断当前是否在战斗中
		GameState state = BattleSystem.Instance.battleData.gameState;
		if (state == GameState.Game || state == GameState.GameWatch || state == GameState.Watcher) {
			// 战斗中只添加frame包
			for (int i = 0; i < frames.frames.Count; ++i) {
				SCFrame scf = frames.frames [i];
				OnNetFrame (scf);
			}
			// 直接执行到最后
			BattleSystem.Instance.lockStep.RunToFrame (BattleSystem.Instance.GetCurrentFrame () / 5 + frames.frames.Count);
			BattleSystem.Instance.sceneManager.SilentMode (true);
			BattleSystem.Instance.battleData.resumingFrame = (BattleSystem.Instance.GetCurrentFrame () / 5 + frames.frames.Count) * 5;
			UISystem.Instance.ShowWindow ("ResumingWindow");
		} else {
			BattleSystem.Instance.SetPlayMode (true, false);
			// 从ready开始解包
			OnReady (frames.ready);
			// start
			OnNetStartBattle ( frames.start );
			// frame
			for (int i = 0; i < frames.frames.Count; ++i) {
				SCFrame scf = frames.frames [i];
				OnNetFrame (scf);
			}
			BattleSystem.Instance.lockStep.runFrameCount = 20;
			// 直接执行到最后
			BattleSystem.Instance.lockStep.RunToFrame (frames.frames.Count);
			BattleSystem.Instance.sceneManager.SilentMode (true);
			BattleSystem.Instance.battleData.resumingFrame = (frames.frames.Count + frames.frames.Count / 200) * 5;
		}
		if (UISystem.Get ().IsWindowVisible ("CommonDialogWindow")) {
			UISystem.Get ().HideWindow ("CommonDialogWindow");
		}
	}

   
	/// <summary>
	/// Requests the join room.
	/// </summary>
	/// <param name="roomid">Roomid.</param>
	public void RequestJoinRoom (int roomid)
	{
		NetMessage.CSJoinRoom proto = new CSJoinRoom();
		proto.roomid = roomid;
		NetSystem.Instance.Send<NetMessage.CSJoinRoom>((int)NetMessage.MsgId.ID_CSJoinRoom, proto);
	}

	/// <summary>
	/// Raises the request join room event.
	/// </summary>
	/// <param name="msgid">Msgid.</param>
	/// <param name="msg">Message.</param>
	void OnRequestJoinRoom (int msgid, PacketEvent msg)
	{
        MemoryStream ms             = msg.Data as MemoryStream;
        NetMessage.SCJoinRoom proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCJoinRoom>(ms);
		if (proto.code != ErrCode.EC_Ok)
			return;

		BattleSystem.Instance.battleData.matchId = proto.room.matchid;
		#if!SERVER
		EventSystem.Instance.FireEvent (EventId.OnJoinRoom, proto.data );
		#endif
	}

	/// <summary>
	/// Requests the create room.
	/// </summary>
	/// <param name="matchid">Matchid.</param>
	public void RequestCreateRoom (string matchid)
	{
		NetMessage.CSCreateRoom proto = new CSCreateRoom();
		proto.matchid       = matchid;
		NetSystem.Instance.Send<NetMessage.CSCreateRoom>((int)NetMessage.MsgId.ID_CSCreateRoom, proto);
	}

	/// <summary>
	/// Raises the request create room event.
	/// </summary>
	/// <param name="msgid">Msgid.</param>
	/// <param name="msg">Message.</param>
	void OnRequestCreateRoom (int msgid, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
		NetMessage.SCCreateRoom proto   = ProtoBuf.Serializer.Deserialize<NetMessage.SCCreateRoom>(ms);
		if (proto.code != ErrCode.EC_Ok)
			return;
		BattleSystem.Instance.battleData.matchId = proto.room.matchid;
		#if!SERVER
		EventSystem.Instance.FireEvent (EventId.OnCreateRoom, proto.data);
		#endif
	}

	/// <summary>
	/// Requests the quit room.
	/// </summary>
	public void RequestQuitRoom ()
	{
		NetMessage.CSQuitRoom proto = new CSQuitRoom();
		NetSystem.Instance.Send< NetMessage.CSQuitRoom >((int)NetMessage.MsgId.ID_CSQuitRoom, proto);
	}

	/// <summary>
	/// Raises the request quit room event.
	/// </summary>
	/// <param name="msgid">Msgid.</param>
	/// <param name="msg">Message.</param>
	void OnRequestQuitRoom (int msgid, PacketEvent msg)
	{
        MemoryStream ms             = msg.Data as MemoryStream;
        NetMessage.SCQuitRoom proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCQuitRoom>(ms);
		if (proto.code != ErrCode.EC_Ok)
			return;

		#if !SERVER
		UISystem.Get ().HideAllWindow ();
		UISystem.Get ().ShowWindow ("StartWindow");
		#endif
	}

	/// <summary>
	/// Rooms the refresh.
	/// </summary>
	/// <param name="msgid">Msgid.</param>
	/// <param name="msg">Message.</param>
	void RoomRefresh (int msgid, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
        NetMessage.SCRoomRefresh proto  = ProtoBuf.Serializer.Deserialize<NetMessage.SCRoomRefresh>(ms);

        #if !SERVER
        EventSystem.Instance.FireEvent ( EventId.OnRoomRefresh, proto.data );
		#endif
	}

	void RoomListRefresh (int msgid, PacketEvent msg)
	{
        MemoryStream ms                    = msg.Data as MemoryStream;
        NetMessage.SCRoomListRefresh proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCRoomListRefresh>(ms);

        #if !SERVER
        EventSystem.Instance.FireEvent (EventId.OnRoomListREfresh, proto.roomid, proto.playernum );
		#endif
	}

	void OnMatch2CurNum (int msgid, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
        NetMessage.SCMatch2CurNum proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCMatch2CurNum>(ms);

        #if !SERVER
        EventSystem.Instance.FireEvent (EventId.OnRoomListREfresh, proto.playernum );
		#endif
	}

	public void MatchGame2 ()
	{
		BattleSystem.Instance.battleData.gameType = GameType.PVP;
		NetMessage.CSStartMatch2 proto = new CSStartMatch2();
		NetSystem.Instance.Send< NetMessage.CSStartMatch2 >((int)NetMessage.MsgId.ID_CSStartMatch2, proto);
	}

	void ONMatchGameBack (int msgid, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
        NetMessage.SCStartMatch2 proto  = ProtoBuf.Serializer.Deserialize<NetMessage.SCStartMatch2>(ms);
        if (proto.code != ErrCode.EC_Ok)
			return;
		#if !SERVER
		UISystem.Get ().ShowWindow ("WaitWindow");
		EventSystem.Instance.FireEvent (EventId.OnStartMatch2);
		UISystem.Get ().HideWindow ("SelectRaceWindow");
		#endif
	}

	
	
	/// <summary>
	/// 背包数据更新
	/// </summary>
	private void OnPackUpdate (int msgid, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
		NetMessage.SCPackUpdate proto   = ProtoBuf.Serializer.Deserialize<NetMessage.SCPackUpdate>(ms);

		NetMessage.PackItem pi = null;
		for (int i = 0; i < proto.modified.Count; ++i) {
			pi = proto.modified[i];

		}
		#if !SERVER
		EventSystem.Instance.FireEvent (EventId.OnCoinSync);
		#endif
	}

	
	public void RequestGMCommand (string cmd)
	{
		NetMessage.CSGMCmd proto = new CSGMCmd();
		proto.cmd                = cmd;
		NetSystem.Instance.Send<NetMessage.CSGMCmd>((int)NetMessage.MsgId.ID_CSGMCmd, proto);
	}

	public void RequestSingleMatch (string matchId, GameType etype, List<BuildTypeBehaviour> mapList = null )
	{
		BattleSystem.Instance.Reset ();

        // 设置战斗模式、随机种子、地图
        BattleSystem.Instance.SetPlayMode(false, true);
        BattleSystem.Instance.battleData.gameType   = etype;
		BattleSystem.Instance.battleData.matchId    = matchId;
		BattleSystem.Instance.battleData.rand.seed  = 1;
		BattleSystem.Instance.battleData.teamFight  = false;
		BattleSystem.Instance.battleData.sceneRoot  = Game.game.sceneRoot;
		BattleSystem.Instance.battleData.sceneRoot.SetActive(true);

		// 玩家数据
		MapConfig mapConfig  = MapConfigProvider.Instance.GetData (matchId);
		List<int> userIdList = new List<int> ();
		for (int i = 0; i < mapConfig.player_count; ++i) {
			Team t = BattleSystem.Instance.sceneManager.teamManager.GetTeam ((TEAM)(i + 1));
			if (t.team == TEAM.Team_1) {
				t.playerData.Init (LocalPlayer.Get ().playerData);
				BattleSystem.Instance.battleData.currentTeam = t.team;
			} else {
				t.playerData.userId     = -10000 - i;
				t.playerData.name       = AIManager.GetAIName (t.playerData.userId);
				t.playerData.icon       = AIManager.GetAIIcon (t.playerData.userId);
				// 加入ai数据
				BattleSystem.Instance.sceneManager.aiManager.AddAI (t, AIType.FriendSmart, t.playerData.level, BattleSystem.Instance.battleData.aiLevel );
				t.aiEnable = true;
			}

			userIdList.Add (t.playerData.userId);
		}


        // 状态、预加载资源、预加载特效
		BattleSystem.Instance.battleData.gameState = GameState.Game;
		AssetManager.Get().LoadBattleResources ();
		EffectManager.Get().PreloadEffect ();
		
        // 创建地图
		BattleSystem.Instance.sceneManager.Reset ();
		if(mapList == null )
			BattleSystem.Instance.sceneManager.CreateScene (userIdList);
		else
			BattleSystem.Instance.sceneManager.CreateBattleScene(mapList);

		if (BattleSystem.Instance.battleData.useAI) {
			// 启动ai
			BattleSystem.Instance.sceneManager.aiManager.Start (1);
		} 
		else 
		{
			BattleSystem.Instance.battleData.useAI = true;
		}
		// lockstep 模式
		BattleSystem.Instance.lockStep.replay		= true;
		BattleSystem.Instance.lockStep.playSpeed	= 1;
		EventSystem.Instance.FireEvent (EventId.OnStartSingleBattle);
	}

	private void OnSingleMatch (int msgid, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
        NetMessage.SCSingleMatch proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCSingleMatch>(ms);
		if (proto.code != ErrCode.EC_Ok)
        {
			Tips.Make ("开始单机游戏错误：" + proto.code.ToString ());
			return;
		}
	}


	/// <summary>
	/// 设置当前通关关卡
	/// </summary>
	/// <param name="map">Map.</param>
	public void RequestSetCurrentLevel (string matchId, string guidId, string guidlevel)
	{
        string seriziedStr = string.Format("{0},{1},{2}", matchId, guidId, guidlevel );

		NetMessage.CSSetCurLevel proto  = new CSSetCurLevel();
		proto.cur_level                 = seriziedStr;
		NetSystem.Instance.Send<NetMessage.CSSetCurLevel>((int)NetMessage.MsgId.ID_CSSetCurLevel, proto);
	}

	private void OnSetCurrentLevelResult (int msgid, PacketEvent msg)
	{
        #if !SERVER
        MemoryStream ms                 = msg.Data as MemoryStream;
		NetMessage.SCSetCurLevel proto  = ProtoBuf.Serializer.Deserialize<NetMessage.SCSetCurLevel>(ms);
		if (proto.code == ErrCode.EC_Ok) {
			LocalAccountStorage.Get ().singleCurrentLevel = string.Empty;
			LocalStorageSystem.Instance.NeedSaveToDisk ();
		}
		#endif
	}

	/// <summary>
	/// 改名字
	/// </summary>
	/// <param name="name">Name.</param>
	public void ChangeName (string name)
	{
		NetMessage.CSRename proto = new CSRename();
		proto.name                = name;
		NetSystem.Instance.Send<NetMessage.CSRename>((int)NetMessage.MsgId.ID_CSRename, proto);
	}

	private void OnChangeName (int msgId, PacketEvent msg)
	{
        #if !SERVER
        MemoryStream ms             = msg.Data as MemoryStream;
		NetMessage.SCRename proto   = ProtoBuf.Serializer.Deserialize<NetMessage.SCRename>(ms);
		EventSystem.Instance.FireEvent (EventId.OnRename, proto.code);
		#endif
	}


	/// <summary>
	/// 请求客户端保存数据
	/// tips：其中包含所有客户端相关的杂碎数据
	/// </summary>
	public void LoadClientStorage ()
	{
		NetMessage.CSClientStorageLoad proto = new CSClientStorageLoad();
		NetSystem.Instance.Send<NetMessage.CSClientStorageLoad>((int)NetMessage.MsgId.ID_CSClientStorageLoad, proto);
	}

	private void OnClientStorageLoad (int msgId, PacketEvent msg)
	{
        #if !SERVER
        MemoryStream ms                         = msg.Data as MemoryStream;
		NetMessage.SCClientStorageLoad proto    =ProtoBuf.Serializer.Deserialize<NetMessage.SCClientStorageLoad>(ms);
		if (proto.values.Count >= 3)
        {
			EventSystem.Instance.FireEvent (EventId.OnStorageLoaded, proto.values[ ((int)ClientStorageConst.ClientStorageRedPoints)]);
		}
		#endif
	}

	/// <summary>
	/// 请求设置客户端保存数据
	/// </summary>
	public void SetClientStorage (int storageIndex, string vv)
	{
		NetMessage.CSClientStorageSet proto = new CSClientStorageSet();
		proto.index.Add (storageIndex);
		proto.value.Add (vv);
		NetSystem.Instance.Send<NetMessage.CSClientStorageSet>((int)NetMessage.MsgId.ID_CSClientStorageSet, proto);
	}

	private void OnClientStorageSet (int msgId, PacketEvent msg)
	{
        #if !SERVER
        MemoryStream ms                     = msg.Data as MemoryStream;
		NetMessage.SCClientStorageSet proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCClientStorageSet>(ms);
		if (proto.code == ErrCode.EC_Ok) {
			
		}
		#endif
	}

	/// <summary>
	/// 重连恢复战斗
	/// </summary>
	public void ReconnectResume (int frame = -2)
	{
		NetMessage.CSResume proto = new CSResume();
		if (frame > -2)
        {
			proto.startFrameNo = frame;
		}
		NetSystem.Instance.Send<NetMessage.CSResume>((int)NetMessage.MsgId.ID_CSResume, proto);
	}

	/// <summary>
	/// 重连恢复战斗回复
	/// </summary>
	private void OnReconnectResumeResult (int msgId, PacketEvent msg)
	{
        #if !SERVER
        MemoryStream ms             = msg.Data as MemoryStream;
		NetMessage.SCResume proto   = ProtoBuf.Serializer.Deserialize<NetMessage.SCResume>(ms);

		if (proto.code != ErrCode.EC_Ok) {
			BattleSystem.Instance.Reset ();
			UISystem.Get ().HideAllWindow ();
            UISystem.Get().ShowWindow("CustomSelectWindowNew");

			return;
		}

		// 判断各种状态
		if (proto.match != null )
        {
			// 匹配状态
			if (proto.match.typ == MatchType.MT_Ladder || proto.match.typ == MatchType.MT_League)
			{
				UISystem.Instance.HideAllWindow ();
				UISystem.Get ().ShowWindow ("PVPWaitWindow");
				OnMatchInit ( proto.match );
			}
			else if (proto.match.typ == MatchType.MT_Room)
			{
				UISystem.Instance.HideAllWindow ();
				UISystem.Instance.ShowWindow ("RoomWaitWindow");
				OnMatchInit ( proto.match);
			}
		}
        else if (proto.report != null ) {
			// 战斗中状态
			PbSCFrames frames = proto.report;
			AssetManager.Get ().LoadBattleResources ();

			// 此时需要判断当前是否在战斗中
			GameState state = BattleSystem.Instance.battleData.gameState;
			if (state == GameState.Game || state == GameState.GameWatch || state == GameState.Watcher) {
				// 战斗中只添加frame包
				for (int i = 0; i < frames.frames.Count; ++i) {
					SCFrame scf = frames.frames[i];
					OnNetFrame (scf);
				}
				// 直接执行到最后
				BattleSystem.Instance.lockStep.RunToFrame (BattleSystem.Instance.GetCurrentFrame () / 5 + frames.frames.Count);
				BattleSystem.Instance.sceneManager.SilentMode (true);
				BattleSystem.Instance.battleData.resumingFrame = (BattleSystem.Instance.GetCurrentFrame () / 5 + frames.frames.Count) * 5;
				UISystem.Instance.ShowWindow ("ResumingWindow");
			} else {
				BattleSystem.Instance.SetPlayMode (true, false);

				BattleSystem.Instance.battleData.resumingFrame = (frames.frames.Count + frames.frames.Count / 200) * 5; // modify for jira-491

				// 从ready开始解包
				OnReady (frames.ready);
				// start
				OnNetStartBattle (frames.start);
				// frame
				for (int i = 0; i < frames.frames.Count; ++i)
                {
					SCFrame scf = frames.frames [i];
					OnNetFrame (scf);
				}

				BattleSystem.Instance.lockStep.runFrameCount = 20;
				// 直接执行到最后
				BattleSystem.Instance.lockStep.RunToFrame (frames.frames.Count);
				BattleSystem.Instance.sceneManager.SilentMode (true);
				UISystem.Instance.ShowWindow ("ResumingWindow");
			}
		}

		if (UISystem.Get ().IsWindowVisible ("CommonDialogWindow")) {
			UISystem.Get ().HideWindow ("CommonDialogWindow");
		}

		#endif
	}

	/// <summary>
	/// 玩家被踢跳线
	/// </summary>
	private void OnKickUserNtf (int msgId, PacketEvent msg)
	{
#if !SERVER
        MemoryStream ms                 = msg.Data as MemoryStream;
		NetMessage.SCKickUserNtf proto  = ProtoBuf.Serializer.Deserialize<NetMessage.SCKickUserNtf>(ms);

		if (proto.code == ErrCode.EC_AccountTakenOver) {
			// 设定当前不走断线重连
			LocalPlayer.Get ().isAccountTokenOver = true;
			// 被踢后主动断线
			NetSystem.Instance.Close ();

			string device = proto.device_model;

			UISystem.Instance.ShowWindow ("CommonDialogWindow");
			// 提示是否重连
			EventSystem.Instance.FireEvent (EventId.OnCommonDialog, 1, DictionaryDataProvider.Format (505, device)
				, new EventDelegate (() => {

				// 标记可以重连，然后重连
				LocalPlayer.Get ().isAccountTokenOver = false;
				NetSystem.Instance.DisConnectedCallback ();

			}));
			
		}
		#endif
	}

	/// <summary>
	/// 服务器通知
	/// </summary>
	private void OnServerNotify (int msgId, PacketEvent msg)
	{
        #if !SERVER
        MemoryStream ms             = msg.Data as MemoryStream;
		NetMessage.SCNotify proto   = ProtoBuf.Serializer.Deserialize<NetMessage.SCNotify>(ms);
		if (proto.typ == NotifyType.NT_Popup) {
			// 弹窗
			UISystem.Instance.ShowWindow ("CommonDialogWindow");
			EventSystem.Instance.FireEvent (EventId.OnCommonDialog, 1, proto.text);
		} else if (proto.typ == NotifyType.NT_Scroll) {
			// 滚动条
			Tips.Make (Tips.TipsType.FlowLeft, proto.text, 2.0f);
			Tips.Make (Tips.TipsType.FlowLeft, proto.text, 2.0f);
		} else if (proto.typ == NotifyType.NT_Error) {
			// 通用错误
			Tips.Make (Tips.TipsType.FlowUp, proto.text, 1.0f);
		}

		#endif
	}

	/// <summary>
	/// 开始匹配
	/// 参数1，游戏类型；参数2，游戏类型的额外参数（房间id、联赛id等）
	/// </summary>
	public void StartMatchReq (NetMessage.MatchType type, string misc_id, bool hasRace)
	{
		if (type == MatchType.MT_Ladder) {
			BattleSystem.Instance.SetPlayMode (true, false);
			BattleSystem.Instance.battleData.gameType = GameType.PVP;
		} else if (type == MatchType.MT_League) {
		
		} else if (type == MatchType.MT_Room) {
			
		}

		NetMessage.CSStartMatchReq proto = new CSStartMatchReq ();
		proto.typ = type;
		if (!string.IsNullOrEmpty (misc_id)) {
			proto.misc_id = misc_id;
		}
		proto.has_race = false;
		NetSystem.Instance.Send<NetMessage.CSStartMatchReq>((int)NetMessage.MsgId.ID_CSStartMatchReq, proto);
	}
		
	/// <summary>
	/// 开始战斗的回复
	/// </summary>
	private void OnStartMatchRequest (int msgId, PacketEvent msg)
	{
        MemoryStream ms                     = msg.Data as MemoryStream;
        NetMessage.SCStartMatchReq proto    = ProtoBuf.Serializer.Deserialize<NetMessage.SCStartMatchReq>(ms);
		if (proto.typ == MatchType.MT_Ladder) {
			#if !SERVER
			if (proto.code != ErrCode.EC_Ok) {
				Tips.Make (Tips.TipsType.FlowUp, string.Format ("匹配失败 code={0}", proto.code), 1);
				return;
			}

			// 开始模拟pvp进入的游戏局
            UISystem.Get().HideWindow("CustomSelectWindowNew");
			UISystem.Get ().HideWindow ("StartWindow");
			UISystem.Get ().ShowWindow ("PVPWaitWindow");
			#endif
		} else if (proto.typ == MatchType.MT_League) {
			#if !SERVER
			if (proto.code == ErrCode.EC_Ok) {
                UISystem.Get().HideWindow("CustomSelectWindowNew");
				UISystem.Get ().HideWindow ("LeagueWindow");
				UISystem.Get ().ShowWindow ("PVPWaitWindow");
			}
			EventSystem.Instance.FireEvent (EventId.OnLeagueMatchResult, proto.code);

			#endif
		} else if (proto.typ == MatchType.MT_Room) {

			EventSystem.Instance.FireEvent (EventId.OnStartMatchResult, proto.code);
		}
	}

	/// <summary>
	/// 房间初始化消息
	/// </summary>
	private void OnMatchInit (int msgId, PacketEvent msg)
	{
        MemoryStream ms              = msg.Data as MemoryStream;
        NetMessage.SCMatchInit proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCMatchInit>(ms);
		string matchId               = proto.matchid;
		NetMessage.MatchType matchType = proto.typ;

		if (matchType == MatchType.MT_Ladder) {
			EventSystem.Instance.FireEvent (EventId.OnMatchInit, matchId, proto.miscid, proto.user, proto.useridx, proto.countdown);
		} else if (matchType == MatchType.MT_League) {
			EventSystem.Instance.FireEvent (EventId.OnMatchInit, matchId, proto.miscid, proto.user, proto.useridx, proto.countdown);
		} else if (matchType == MatchType.MT_Room) {
			EventSystem.Instance.FireEvent (EventId.OnMatchInit, matchId, proto.miscid, proto.user, proto.useridx, proto.masterid);
		}
	}

    private void OnMatchInit(NetMessage.SCMatchInit proto)
    {
        string matchId = proto.matchid;
        NetMessage.MatchType matchType = proto.typ;

        if (matchType == MatchType.MT_Ladder)
        {
            EventSystem.Instance.FireEvent(EventId.OnMatchInit, matchId, proto.miscid, proto.user, proto.useridx, proto.countdown, 0);
        }
        else if (matchType == MatchType.MT_League)
        {
            EventSystem.Instance.FireEvent(EventId.OnMatchInit, matchId, proto.miscid, proto.user, proto.useridx, proto.countdown, 0);
        }
        else if (matchType == MatchType.MT_Room)
        {
            EventSystem.Instance.FireEvent(EventId.OnMatchInit, matchId, proto.miscid, proto.user, proto.useridx, proto.countdown, 0);
        }
    }

    /// <summary>
    /// 房间更新消息
    /// </summary>
    private void OnMatchUpdate (int msgId, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
        NetMessage.SCMatchUpdate proto  = ProtoBuf.Serializer.Deserialize<NetMessage.SCMatchUpdate>(ms);

		NetMessage.MatchType matchType = proto.typ;
		if (proto.typ == MatchType.MT_Ladder) {
			EventSystem.Instance.FireEvent (EventId.OnMatchUpdate, proto.user_added, proto.index_added, proto.index_deled);
		}
        else if (proto.typ == MatchType.MT_League) {
			EventSystem.Instance.FireEvent (EventId.OnMatchUpdate, proto.user_added, proto.index_added, proto.index_deled);
		}
        else if (proto.typ == MatchType.MT_Room) {
			if (proto.masterid > 0 )
            {
				// 房主更新
				EventSystem.Instance.FireEvent (EventId.OnMatchUpdate, proto.user_added, proto.index_added, proto.index_deled, proto.kick, proto.change_from, proto.change_from, proto.masterid);
			} else {
				EventSystem.Instance.FireEvent (EventId.OnMatchUpdate, proto.user_added, proto.index_added, proto.index_deled, proto.kick, proto.change_from, proto.change_from);
			}
		}
	}

	/// <summary>
	/// 房间开始，房主发动
	/// </summary>
	public void MatchComplete ()
	{
		NetMessage.CSMatchComplete proto = new CSMatchComplete ();
		NetSystem.Instance.Send<NetMessage.CSMatchComplete>((int)NetMessage.MsgId.ID_CSMatchComplete, proto);
	}

	private void OnMatchComplete (int msgId, PacketEvent msg)
	{
        MemoryStream ms                  = msg.Data as MemoryStream;
		NetMessage.SCMatchComplete proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCMatchComplete>(ms);
		if (proto.code == ErrCode.EC_Ok) {

			UISystem.Get ().HideWindow ("RoomWaitWindow");
			UISystem.Get ().ShowWindow ("PVPWaitWindow");
		} else {
			#if !SERVER
			Tips.Make (Tips.TipsType.FlowUp, DictionaryDataProvider.Format (901, proto.code), 1.0f);
			#endif
		}
	}

	/// <summary>
	/// 房间中玩家位置移动
	/// </summary>
	public void RequestMatchMovePos (int userId, int toIndex)
	{
		NetMessage.CSMatchPos proto = new CSMatchPos();
		proto.userid    = userId;
		proto.index     = toIndex;
		NetSystem.Instance.Send<NetMessage.CSMatchPos>((int)NetMessage.MsgId.ID_CSMatchPos, proto);
	}

	private void OnMatchPos (int msgId, PacketEvent msg)
	{
        #if !SERVER
        MemoryStream ms             = msg.Data as MemoryStream;
		NetMessage.SCMatchPos proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCMatchPos>(ms);
		if (proto.code == ErrCode.EC_Ok) {
		
		}
        else
        {
			Tips.Make (DictionaryDataProvider.GetValue (911));
		}
		#endif
	}

	/// <summary>
	/// 退出、踢出匹配，房间
	/// </summary>
	public void QuitMatch (int userId = -1)
	{
		NetMessage.CSQuitMatch proto = new CSQuitMatch();
		if (userId > 0)
        {
			proto.userid = userId;
		}
		NetSystem.Instance.Send<NetMessage.CSQuitMatch>((int)NetMessage.MsgId.ID_CSQuitMatch, proto);
	}

	/// <summary>
	/// 退出匹配房间
	/// </summary>
	private void OnQuitMatch (int msgId, PacketEvent msg)
	{
        MemoryStream ms              = msg.Data as MemoryStream;
        NetMessage.SCQuitMatch proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCQuitMatch>(ms);
		EventSystem.Instance.FireEvent (EventId.OnMatchQuit, proto.code);
	}

	/// <summary>
	/// 请求获取章节信息
	/// </summary>
	/// <param name="levelID">Level I.</param>
	public void RequestChapters()
	{
		NetMessage.CSLoadChapters proto = new CSLoadChapters();
		NetSystem.Instance.Send<NetMessage.CSLoadChapters>((int)NetMessage.MsgId.ID_CSLoadChapters, proto);
	}

	/// <summary>
	/// 请求获取章节信息
	/// </summary>
	/// <param name="msgId">Message identifier.</param>
	/// <param name="msg">Message.</param>
	private void OnLoadChapters(int msgId, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
        NetMessage.SCLoadChapters proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCLoadChapters>(ms);
		EventSystem.Instance.FireEvent (EventId.OnLoadChaptersResult, proto);
	}

	/// <summary>
	/// 请求获取关卡信息
	/// </summary>
	/// <param name="levelID">Level I.</param>
	public void RequestOneChapter(string chapterId)
	{
		NetMessage.CSLoadChapter proto = new CSLoadChapter();
		proto.chapter   = chapterId;
		NetSystem.Instance.Send<NetMessage.CSLoadChapter>((int)NetMessage.MsgId.ID_CSLoadChapter, proto);
	}

	/// <summary>
	/// 请求获取关卡信息
	/// </summary>
	/// <param name="msgId">Message identifier.</param>
	/// <param name="msg">Message.</param>
	private void OnLoadOneChapter(int msgId, PacketEvent msg)
	{
        MemoryStream ms                = msg.Data as MemoryStream;
        NetMessage.SCLoadChapter proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCLoadChapter>(ms);
        EventSystem.Instance.FireEvent (EventId.OnLoadOneChapterResult, proto);
	}

	/// <summary>
	/// 开始关卡
	/// </summary>
	/// <param name="levelId">Level identifier.</param>
	public void RequestStartLevel (string levelId)
	{
		NetMessage.CSStartLevel proto = new CSStartLevel ();
		proto.level_name              = levelId;
		NetSystem.Instance.Send<NetMessage.CSStartLevel>((int)NetMessage.MsgId.ID_CSStartLevel, proto);
	}


	private void OnStartLevel (int msgId, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
        NetMessage.SCStartLevel proto   = ProtoBuf.Serializer.Deserialize<NetMessage.SCStartLevel>(ms);
        EventSystem.Instance.FireEvent (EventId.OnStartLevelResult, proto);
	}

	/// <summary>
	/// 通关后，改变星星数量
	/// </summary>
	/// <param name="levelID">Level I.</param>
	/// <param name="star">Star.</param>
	public void SetLevelStar(string levelID, int star)
	{
		NetMessage.CSSetLevelStar proto = new CSSetLevelStar();
		proto.level_name    = levelID;
		proto.star          = star;
		NetSystem.Instance.Send<NetMessage.CSSetLevelStar>((int)NetMessage.MsgId.ID_CSSetLevelStar, proto);
	}

	/// <summary>
	/// 改变星星数量回复
	/// </summary>
	/// <param name="msgId">Message identifier.</param>
	/// <param name="msg">Message.</param>
	private void OnSetLevelStar(int msgId, PacketEvent msg)
	{
        MemoryStream ms                 = msg.Data as MemoryStream;
        NetMessage.SCSetLevelStar proto = ProtoBuf.Serializer.Deserialize<NetMessage.SCSetLevelStar>(ms);
        EventSystem.Instance.FireEvent (EventId.OnSetLevelStarResult, proto);
	}

	/// <summary>
	/// Raises the int attr event.
	/// </summary>
	/// <param name="msgId">Message identifier.</param>
	/// <param name="msg">Message.</param>
	private void OnIntAttr(int msgId, PacketEvent msg)
	{
        MemoryStream ms             = msg.Data as MemoryStream;
		NetMessage.SCIntAttr proto  = ProtoBuf.Serializer.Deserialize<NetMessage.SCIntAttr>(ms);
		LocalPlayer.Get ().playerData.UpdateFromNetMsg (proto);
		EventSystem.Instance.FireEvent (EventId.UpdatePower, proto);
	}

}


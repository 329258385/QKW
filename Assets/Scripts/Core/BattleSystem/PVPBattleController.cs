using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plugin;
using Solarmax;
using NetMessage;


public class BattleEndData
{
	public TEAM team;
	public string userName;
	public int userId;
	public EndType endType; //  
	public int endFrame;
	public int destroy;
	public int survive;
}

/// <summary>
/// 多人战斗的控制器
/// </summary>
public class PVPBattleController : IBattleController
{
	private BattleData battleData;
	private BattleSystem battleSystem;

	private int tickEndCount = 0;
	private List<BattleEndData> battleEndDatas = new List<BattleEndData>();
	private int diedTeamCount = 0;

	private int battleTimeMax = 0;

	private int survivorTimeMax = 0;    //失去所有星球后的生存时间

	public PVPBattleController(BattleData bd, BattleSystem bs)
	{
		battleData = bd;
		battleSystem = bs;
	}

	public bool Init()
	{
		tickEndCount = 0;
		diedTeamCount = 0;

		battleTimeMax = int.Parse(GameVariableConfigProvider.Instance.GetData(4)) * 60;
		survivorTimeMax = int.Parse(GameVariableConfigProvider.Instance.GetData(6));

		return true;
	}

	public void Tick(int frame, float interval)
	{
		UpdateEnd(frame, interval);

		UpdateResuming(frame, interval);
	}

	public void Destroy()
	{

	}

	public void Reset()
	{
		tickEndCount = 0;
		battleEndDatas.Clear();
	}

	public void OnRecievedFramePacket(NetMessage.SCFrame frame)
	{
		List<Packet> list = new List<Packet>();

		for (int i = 0; i < frame.users.Count; i++) {

			int uid = frame.users[i];
			NetMessage.PbFrames pbs = frame.frames[i];

			if (pbs == null || pbs.frames.Count == 0)
				continue;

			for (int k = 0; k < pbs.frames.Count; k++) {
				NetMessage.PbFrame pb = pbs.frames[k];
				if (pb == null)
					continue;

				Packet move = new Packet();
				move.team   = (TEAM)(uid + 1);
				move.packet = Json.DeCode<FramePacket>(pb.content);
                list.Add(move);
			}
		}
		battleSystem.lockStep.AddFrame(frame.frameNum, list.ToArray());
	}

	public void OnRunFramePacket(FrameNode frameNode)
	{
		battleSystem.sceneManager.RunFramePacket(frameNode);
	}

    public void OnPlayerMove(Node from, Node to, BattleTeam battleTea)
	{
		NetSystem.Instance.helper.SendMoveMessaeg(from, to);
	}

	public void PlayerGiveUp()
	{
		SendGiveUpFrame();
	}

	public void OnPlayerGiveUp(TEAM giveUpTeam)
	{
		OnPlayerDiedOrGiveUp(EndType.ET_Giveup, giveUpTeam, battleSystem.GetCurrentFrame());
	}

	public void OnPlayerDirectQuit(TEAM team)
	{
		QuitBattle();
	}

	/// <summary>
	/// 检查是否到达结果
	/// </summary>
	private void UpdateEnd(int frame, float dt)
	{
		if (++tickEndCount == 5) {
			// 先查询谁死了
			for (int i = 0; i < (int)TEAM.TeamMax; ++i) {
				Team t = battleSystem.sceneManager.teamManager.GetTeam((TEAM)i);
				if (t != null && t.Valid() && !t.isEnd) {
					if (t.CheckDead()) {
						// 死亡
						OnPlayerDiedOrGiveUp(EndType.ET_Dead, t.team, frame);
					}
					
				}
			}

			if (diedTeamCount != battleData.currentPlayers - 1)
				tickEndCount = 0;
		}

		// 限制超时时间
		if (battleSystem.sceneManager.GetBattleTime() > battleTimeMax) {
			// 当前所有没有死亡的都给个Timout
			for (int i = 0; i < (int)TEAM.TeamMax; ++i) {
				Team t = battleSystem.sceneManager.teamManager.GetTeam((TEAM)i);
				if (t != null && t.Valid() && !t.isEnd) {
					if (!t.CheckDead()) {
						OnPlayerEnterEnd(t, EndType.ET_Timeout, frame);
					}
				}
			}

			// 结束
			QuitBattle(true);
		}
	}

	private void OnPlayerEnterEnd(Team t, EndType type, int frame)
	{
		BattleEndData data = new BattleEndData();
		data.team = t.team;
		data.userName = t.playerData.name;
		data.userId = t.playerData.userId;
		data.destroy = t.destory;
		data.endType = type;
		data.endFrame = frame;
		t.isEnd = type == EndType.ET_Dead;

		battleEndDatas.Add(data);
	}

	public void OnPlayerDiedOrGiveUp(EndType type, TEAM team, int frame)
	{
		//Debug.LogFormat ("PlayerDiedOrGiveUp      type:{0}, team:{1}, frame{2}", type, team, frame);

		// 当前队伍
		Team t = battleSystem.sceneManager.teamManager.GetTeam(team);

		// 如果结果中已经有当前种类和当前队伍的信息时，不需要再处理
		bool haveEnd = false;
		for (int i = 0; i < battleEndDatas.Count; ++i) {
			BattleEndData d = battleEndDatas[i];
			if (d.team == team && d.endType == type) {
				haveEnd = true;
				break;
			}
		}
		if (haveEnd) {
#if !SERVER
			LoggerSystem.Instance.Error(string.Format("PlayerDiedOrGiveUp  Name:{0} Team:{1} type:{2}  frame:{3}", t.playerData.name, team, type, frame));
#endif
			return;
		}

		// 不管在已经有没有结果，都加入。区分giveup和died
		OnPlayerEnterEnd(t, type, frame);

		// 如果是放弃，则使用AI
		if (type == EndType.ET_Giveup) {
			t.aiEnable = true;
#if !SERVER
			LoggerSystem.Instance.Info(string.Format("玩家:{0} 投降，使用AI替代", t.playerData.name));
#endif
		}

		// 死亡队伍计数
		if (type == EndType.ET_Dead)
			++diedTeamCount;

		// 如果已经有三个队伍都是died，则将最后一个加入
		if (diedTeamCount == battleData.currentPlayers - 1) {
			for (int i = 0; i < (int)TEAM.TeamMax; ++i) {
				Team wint = battleSystem.sceneManager.teamManager.GetTeam((TEAM)i);
				if (wint != null && wint.Valid() && !wint.isEnd) {
#if !SERVER
					Debug.Log("加入最后一个队伍:" + wint.team + "    name:" + wint.playerData.name);
#endif
					OnPlayerEnterEnd(wint, EndType.ET_Win, frame);
					battleData.winTEAM = wint.team;
					break;
				}
			}
		}

		// 如果还是有三个队伍died，其实上一步已经把第四个加入了结果，此时结束就好了
		if (diedTeamCount == battleData.currentPlayers - 1)
		{
			QuitBattle(true);

		}
		else
		{
			if (team == battleData.currentTeam)
			{
				// 投降则进入结束流程，正常死亡则进观战
				if (type == EndType.ET_Giveup)
				{
					QuitBattle();
				}
				else
				{
#if !SERVER
					BattleSystem.Instance.battleData.gameState = GameState.GameWatch;
#endif
					EventSystem.Instance.FireEvent(EventId.OnSelfDied);
				}
			}
		}

		// 界面刷新
		if (type == EndType.ET_Giveup)
		{
			EventSystem.Instance.FireEvent(EventId.PlayerGiveUp, team);
		}
		else if (type == EndType.ET_Dead)
		{
			EventSystem.Instance.FireEvent(EventId.PlayerDead, team);
		}
	}

    // 发送
    private void SendGiveUpFrame()
    {
#if !SERVER
        FramePacket packet      = new FramePacket();
        packet.type             = 2;
        packet.giveup           = new GiveUpPacket();
        packet.giveup.team      = battleData.currentTeam;
        byte[] bytestr          = Json.EnCodeBytes(packet);
        NetMessage.PbFrame pb   = new PbFrame();
        NetMessage.CSFrame build = new CSFrame();

        pb.content              = bytestr;
        build.frame             = pb;

        NetSystem.Instance.Send<NetMessage.CSFrame>((int)NetMessage.MsgId.ID_CSFrame, build);
#endif
    }


    // 发送退出战斗指令
    public void QuitBattle(bool finish = false)
	{
		//Debug.LogFormat ("QuitBattle   finished:{0}", finish);

		battleSystem.StopLockStep();

#if !SERVER
		if (UISystem.Instance.IsWindowVisible ("ResumingWindow"))
		UISystem.Instance.HideWindow ("ResumingWindow");

		if (finish)
		{
			UISystem.Get ().ShowWindow ("BattleEndWindow");
			// 闪白
			Team winTeam = BattleSystem.Instance.sceneManager.teamManager.GetTeam (battleData.winTEAM);
			//Debug.Log ("闪白的队伍:" + winTeam.team + "    name:" + winTeam.playerData.name);
			Color winColor = winTeam.color;
			BattleSystem.Instance.sceneManager.ShowWinEffect (winTeam, winColor);
			EventSystem.Instance.FireEvent (EventId.OnFinishedColor, winTeam.color);
		}
		else
		{
			// 观战模式战斗中退出不需要结果界面
			if (battleData.gameState != GameState.Watcher)
			{
				UISystem.Get ().ShowWindow ("BattleEndWindow");
			}
		}

#endif

		if (battleData.isReplay)
		{
			battleSystem.replayManager.NotifyPlayRecordEnd();
			battleSystem.replayManager.PlayRecordOver();

		}
		else
		{
			// 直接从这儿设置状态为战斗结束
			battleSystem.battleData.gameState = GameState.GameEnd;

			//发送消息
			NetMessage.CSQuitBattle quit = new NetMessage.CSQuitBattle();
			//System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			for (int i = 0; i < battleEndDatas.Count; ++i) {
				BattleEndData data = battleEndDatas[i];
				NetMessage.EndEvent e = new NetMessage.EndEvent();
				e.userid                = data.userId;
				e.end_type              = data.endType;
				e.end_frame             = data.endFrame;
				e.end_destroy           = data.destroy;
				e.end_survive           = data.survive;
				quit.events.Add(e);

				//sb.Append (string.Format ("[{0},{1},{2},{3}];", data.userName, data.userId, data.endType, data.endFrame));
			}

			//Debug.Log (sb.ToString());

#if ROBOT
			battleSystem.robotBattle.robot.netSystem.Send((int)NetMessage.MsgId.ID_CSQuitBattle, quit);
#else
			NetSystem.Instance.Send((int)NetMessage.MsgId.ID_CSQuitBattle, quit);
#endif
		}
	}

	public void UpdateResuming (int frame, float interval)
	{
		if (battleData.silent && battleSystem.lockStep.messageCount < 5) {
			battleData.resumingFrame = -1;
			battleSystem.sceneManager.SilentMode (false);
#if !SERVER
			UISystem.Instance.HideWindow ("ResumingWindow");
#endif
		}
	}
}


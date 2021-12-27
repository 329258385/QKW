using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plugin;
using Solarmax;
using NetMessage;


/// <summary>
/// 单人战斗管理器
/// </summary>
public class SingleBattleController : IBattleController
{
	private BattleData battleData;

	private int tickEndCount;

	private List<BattleEndData> battleEndDatas = new List<BattleEndData> ();

	private Packet[] emptyPackets = null;
	private int emptyPacketId = 0;

	private bool useCommonEndCondition = true;

	public SingleBattleController (BattleData bd)
	{
		battleData = bd;
	}

	public bool Init ()
	{
		tickEndCount = 0;

		List<Packet> packets = new List<Packet> ();
		emptyPackets         = packets.ToArray ();
		emptyPacketId        = 0; // 外部已增加了第一个帧包，则此处使用前++加入第二个

		// 加入第一个
		BattleSystem.Instance.lockStep.AddFrame (++emptyPacketId, emptyPackets);

		// 判断一下用那种侦测结束方式
		int validTeamCount = 0;
		for (int i = 0; i < (int)TEAM.TeamMax; ++i) {
			Team t = BattleSystem.Instance.sceneManager.teamManager.GetTeam ((TEAM)i);
			if (t != null && t.Valid ()) {
				++validTeamCount;
			}
		}

		useCommonEndCondition = validTeamCount > 1;

		//ThirdPartySystem.Instance.OnStartPve (battleData.matchId);

		return true;
	}

	public void Tick (int frame, float interval)
	{
		if (useCommonEndCondition) {
			UpdateEnd (frame, interval);
		} else {
			UpdateEndAllOccupied (frame, interval);
		}


		// 自驱动型的单机，判断剩余包数量是不是为0，为0，则继续加入包
		if (BattleSystem.Instance.lockStep.messageCount < 2) {
			BattleSystem.Instance.lockStep.AddFrame (++emptyPacketId, emptyPackets);
		}
	}

	public void Destroy ()
	{

	}

	public void Reset ()
	{
		tickEndCount = 0;
		battleEndDatas.Clear ();
	}

	public void OnRecievedFramePacket (NetMessage.SCFrame frame)
	{
		// nothing.
	}

	public void OnRunFramePacket (FrameNode frameNode)
	{
		// nothing.
	}

    public void OnPlayerMove(Node from, Node to, BattleTeam battleTea)
	{
		//BattleMember shipController = battleTea.CaleCameraTarget();
  //      if( shipController != null )
  //      {
  //          CameraFollow.Instance.SetTarget(shipController);
  //      }
        from.MoveEffect(to, battleTea.ID );
	}

	public void PlayerGiveUp()
	{
		QuitBattle (false);
	}

	public void OnPlayerGiveUp (TEAM giveUpTeam)
	{
		// nothing
	}

	public void OnPlayerDirectQuit (TEAM team)
	{
		
	}

	/// <summary>
	/// 检查是否到达结果
	/// </summary>
	private void UpdateEnd(int frame, float dt)
	{
		if (++tickEndCount == 5) {
			// 先查询谁死了
			for (int i = 0; i < (int)TEAM.TeamMax; ++i) {
				Team t = BattleSystem.Instance.sceneManager.teamManager.GetTeam ((TEAM)i);
				if (t != null && t.Valid () && !t.isEnd) {
					if (t.CheckDead ()) {
						// 死亡
						OnPlayerDiedOrGiveUp (EndType.ET_Dead, t.team, frame);
					}
				}
			}

			if (battleEndDatas.Count != battleData.currentPlayers)
				tickEndCount = 0;
		}
	}

	private void OnPlayerEnterEnd (Team t, EndType type, int frame)
	{
		BattleEndData data = new BattleEndData ();
		data.team = t.team;
		data.userId = t.playerData.userId;
		data.destroy = t.destory;
		data.endType = type;
		data.endFrame = frame;
		t.isEnd = true;

		battleEndDatas.Add (data);
	}

	public void OnPlayerDiedOrGiveUp(EndType type, TEAM team, int frame)
	{
		// 检查他在不在结果列表，在的话，不再加入了
		bool inList = false;
		for (int i = 0; i < battleEndDatas.Count; ++i) {
			if (battleEndDatas [i].team == team) {
				inList = true;
				break;
			}
		}
		if (!inList) {
			Team t = BattleSystem.Instance.sceneManager.teamManager.GetTeam (team);
			OnPlayerEnterEnd (t, type, frame);
		}

		// 判断是不是总人数-1，如果是，则将第一名加入
		if (battleEndDatas.Count == battleData.currentPlayers - 1) {
			for (int i = 0; i < (int)TEAM.TeamMax; ++i)
			{
				Team t = BattleSystem.Instance.sceneManager.teamManager.GetTeam ((TEAM)i);
				if (t != null && t.Valid() && !t.isEnd) {
					OnPlayerEnterEnd (t, EndType.ET_Win, frame);
					battleData.winTEAM = t.team;
					break;
				}
			}
		}

		// 出现结果，结束战斗
		if (battleEndDatas.Count == battleData.currentPlayers) {
			QuitBattle (true);
		}
	}

	// 发送退出战斗指令
	public void QuitBattle(bool finish = false)
	{
		#if !SERVER
		BattleSystem.Instance.StopLockStep ();

        Camera.main.transform.position          = new Vector3(0, 25, -60f);
        Camera.main.transform.localEulerAngles  = new Vector3(25f, 0, 0);
		if (BattleSystem.Instance.battleData.gameType != GameType.TestLevel &&
			BattleSystem.Instance.battleData.gameType != GameType.SingleLevel)
		{
			// 单机关卡需要上报完成当前关卡
			if (battleData.currentTeam == battleData.winTEAM ) 
			{
				
				string sub = battleData.matchId.Substring(1);
				int curID  = int.Parse(sub);
				if (curID == LocalLevelStorage.Get().LevelID)
				{
					LocalLevelStorage.Get().Levelfails = 0;
					LocalLevelStorage.Get().LevelID = 0;
					LocalStorageSystem.Instance.NeedSaveToDisk();
				}
				
			}
			else 
			{   
				
				
			}
		}

		//新关卡模式
		if (battleData.currentTeam == battleData.winTEAM &&
			BattleSystem.Instance.battleData.gameType == GameType.SingleLevel)
		{
			//新关卡模式
			LevelDataHandler.Instance.SetLevelStarToServer (BattleSystem.Instance.battleData.difficultyLevel);
		}
			
		if (finish)
		{
			// 闪白
			Team winTeam = BattleSystem.Instance.sceneManager.teamManager.GetTeam (battleData.winTEAM);
			Color winColor = winTeam.color;
			BattleSystem.Instance.sceneManager.ShowWinEffect (winTeam, winColor);

			UISystem.Get ().ShowWindow ("BattleEndWindow");
			EventSystem.Instance.FireEvent (EventId.OnFinished);
			EventSystem.Instance.FireEvent (EventId.OnFinishedColor, winColor);

			//sdk
			//ThirdPartySystem.Instance.OnFinishPve (battleData.matchId);
		}
		else
		{

			// 关闭窗口退出
			UISystem.Get ().HideAllWindow ();

			BattleSystem.Instance.BeginFadeOut();

			UISystem.Get ().FadeBattle(false, new EventDelegate(()=>{
                if (BattleSystem.Instance.battleData.gameType == GameType.Single)
                {
                    BattleSystem.Instance.Reset();
                    UISystem.Get().ShowWindow("CustomSelectWindow");
                }
                if (BattleSystem.Instance.battleData.gameType == GameType.Guide)
                {
                    BattleSystem.Instance.Reset();
                    UISystem.Get().ShowWindow("CustomSelectWindowNew");
                }

                if (BattleSystem.Instance.battleData.gameType == GameType.TestLevel)
                {
                    BattleSystem.Instance.Reset();
                    UISystem.Get().ShowWindow("CustomTestLevelWindow");
                }

				if (BattleSystem.Instance.battleData.gameType == GameType.SingleLevel)
				{
					BattleSystem.Instance.Reset();
					UISystem.Get().ShowWindow("CustomSelectLevelWindow");
				}
			}));

			//sdk
			//ThirdPartySystem.Instance.OnFailPve (battleData.matchId);
		}
		#endif
	}

	/// <summary>
	/// 所有全部销毁
	/// </summary>
	private void UpdateEndAllOccupied (int frame, float interval)
	{
		if (++tickEndCount == 5) {

			if (BattleSystem.Instance.sceneManager.nodeManager.AllOccupied ((int)battleData.currentTeam)) {

				battleData.winTEAM = battleData.currentTeam;
				QuitBattle (true);
				return;
			}

			tickEndCount = 0;
		}
	}
}


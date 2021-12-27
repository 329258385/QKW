using System;
using System.Collections.Generic;
using UnityEngine;
using Plugin;
using Solarmax;




/// <summary>
/// 战斗系统
/// fangjun
/// </summary>
public class BattleSystem : Solarmax.Singleton<BattleSystem>, Solarmax.Lifecycle
{
	/// <summary>
	/// 帧同步
	/// </summary>
	public LockStep                 lockStep;

	/// <summary>
	/// 场景管理
	/// </summary>
	public SceneManager             sceneManager;

	/// <summary>
	/// 战斗数据
	/// </summary>
	public BattleData               battleData;

    /// <summary>
    /// 回放管理器
    /// </summary>
	public ReplayManager            replayManager;

	/// <summary>
	/// 战斗控制器
	/// </summary>
	private IBattleController       battleController;

    /// <summary>
    /// 单机战斗控制器
    /// </summary>
	private SingleBattleController  singleBattleController;

    /// <summary>
    /// 联网战斗控制器
    /// </summary>
	private PVPBattleController     pvpBattleController;

	/// <summary>
	/// 暂停
	/// </summary>
	private bool                    pause;

	public BattleSystem ()
	{
		battleData              = new BattleData ();
		lockStep                = new LockStep ();
		sceneManager            = new SceneManager (battleData);
		battleController        = null;
		pvpBattleController     = new PVPBattleController (battleData, this);
		singleBattleController  = new SingleBattleController (battleData);
		replayManager           = new ReplayManager (battleData);
	}



	public bool Init()
	{
		lockStep.frameThreshold = 3;
		lockStep.AddListennerLogic(FrameTick);
		lockStep.AddListennerPacket(FramePacketRun);

		//string produceValue = GameVariableConfigProvider.Instance.GetData (3);
		//string[] pros = produceValue.Split (';');
		//for (int i= 0; i < pros.Length; ++i)
		//{
		//	List<float> args = Converter.ConvertNumberList<float> (pros [i]);
		//}

		battleData.Init();
		sceneManager.Init();
		replayManager.Init();

		AssetManager.Get().Init();
		EffectManager.Get().Init();
		GameTimeManager.Get().Init();

		pause = false;
		return true;
	}

	public void FrameTick (int frame, float interval)
	{
		if (battleController != null)
			battleController.Tick (frame, interval);

		battleData.Tick (frame, interval);
		sceneManager.Tick (frame, interval);
		replayManager.Tick (frame, interval);

	}

	public void FramePacketRun (FrameNode frameNode)
	{
		if (battleController != null)
			battleController.OnRunFramePacket (frameNode);
	}

	public void Tick(float interval)
	{
		if (pause)
			return;

		lockStep.Tick (interval);

		#if !SERVER
        float fScaleSpeed = sceneManager.GetbattleScaleSpeed();
        float fesp        = interval * fScaleSpeed;
        EffectManager.Get().fPlayAniSpeed = fScaleSpeed;
        EffectManager.Get().Tick(Time.frameCount, fesp);
		#endif
	}

	public void Destroy()
	{
		LoggerSystem.Instance.Debug("BattleSystem    destroy  begin");

		Reset ();

		if (battleController != null)
			battleController.Destroy ();

		EffectManager.Instance.Destroy();

		lockStep.StopLockStep (true);
		AssetManager.Get().UnLoadBattleResources ();
		LoggerSystem.Instance.Debug("BattleSystem    destroy  end");
	}

	public void Reset ()
	{
		pause = false;
        if(CameraFollow.Instance != null )
            CameraFollow.Instance.SetTarget(null);
        
		battleData.Init ();
		
		if (battleController != null)
			battleController.Reset ();

		lockStep.StopLockStep (false);
		sceneManager.Destroy ();
		battleData.Destroy ();
		replayManager.Destroy ();

		GameTimeManager.Get ().Release ();
		Resources.UnloadUnusedAssets ();
		System.GC.Collect ();
	}

	/// <summary>
	/// 战斗开始淡出
	/// </summary>
	public void BeginFadeOut()
	{
		#if !SERVER
		EffectManager.Instance.Destroy ();
		#endif
	}

	public void SetPlayMode (bool pvp, bool single)
	{
		if (pvp) {
			battleController = pvpBattleController;
		} else if (single) {
			battleController = singleBattleController;
		} else {
			battleController = null;
			LoggerSystem.Instance.Error ("PlayMode error!");
		}

		LoggerSystem.Instance.Info ("设置战斗模式为：pvp:{0}, single:{1}", pvp, single);
	}

	public void OnPlayerMove (Node from, Node to, int BattleID = 0)
	{
        if (from == null)
            return;

        BattleTeam selectBT = null;
        int nBattleTeamNum  = 0;
        int nIndex          = -1;   // 缓存将要移动的战队索引
        for (int i = 0; i < from.battArray.Count; i++ )
        {
            BattleTeam bt = from.battArray[i];
            if (bt == null)
                continue;

            /// 选择第一个战队
            if (bt != null && bt.team.team == BattleSystem.Get().battleData.currentTeam && selectBT == null)
            {
                nBattleTeamNum++;
                nIndex      = i;
                selectBT    = bt;
            }

            //if (BattleSystem.Instance.battleData.currentBattleTeam == null && selectBT != null)
            //{
            //    break;
            //}

            /// 如果星球有选择的战队，则优先界面选择的战队 
            if (bt == BattleSystem.Instance.battleData.currentBattleTeam)
            {
                selectBT = bt;
                break;
            }
        }

        if (selectBT == null)
            return;

        if (selectBT.btFormation !=  Formation.FormationMove )
        {
            battleController.OnPlayerMove(from, to, selectBT);
        }
    }

    public void OnRecievedFramePacket (NetMessage.SCFrame frame)
	{
		battleController.OnRecievedFramePacket (frame);
	}

	public void PlayerGiveUp ()
	{
		battleController.PlayerGiveUp ();
	}

	public void OnPlayerGiveUp(TEAM team)
	{
		battleController.OnPlayerGiveUp (team);
	}

	public void OnPlayerDirectQuit()
	{
		battleController.OnPlayerDirectQuit (battleData.currentTeam);
	}

	/// <summary>
	/// 获取当前帧号
	/// </summary>
	/// <returns>The current frame.</returns>
	public int GetCurrentFrame()
	{
		return lockStep.GetCurrentFrame ();
	}

	/// <summary>
	/// 启动lockstep
	/// </summary>
	public void StartLockStep ()
	{
		if (battleController != null)
			battleController.Init ();
		
		lockStep.StarLockStep ();
	}

	public void StopLockStep ()
	{
		lockStep.StopLockStep ();
	}

	/// <summary>
	/// 暂停
	/// </summary>
	public void SetPause (bool status)
	{
		pause = status;
	}

	public bool IsPause ()
	{
		return pause;
	}
}



using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plugin;
using Solarmax;
using NetMessage;


public class ReplayManager : Lifecycle2
{
	BattleData battleData;

	/// <summary>
	/// 战报简略数据
	/// </summary>
	public BattleReportData reportData;

	/// <summary>
	/// 当前播放的战报
	/// </summary>
	public PbSCFrames curPlayRecord;

	/// <summary>
	/// 战报时间
	/// </summary>
	//float recordTotleTime;

	public ReplayManager (BattleData bd)
	{
		battleData = bd;
	}

	public bool Init ()
	{
		curPlayRecord = null;
		reportData = null;

		return true;
	}

	public void Tick (int frame, float interval)
	{
		UpdateRecord (frame, interval);
	}

	public void Destroy ()
	{

		battleData.isReplay = false;
	}

	/// <summary>
	/// 播放战报
	/// param PbSCFrame
	/// </summary>
	public void TryPlayRecord(PbSCFrames msg)
	{
		battleData.isReplay = true;

		curPlayRecord = msg;
		//recordTotleTime = 0;
		battleData.matchId = curPlayRecord.ready.match_id;
		BattleSystem.Instance.SetPlayMode (true, false);

		NetSystem.Instance.helper.OnReady (curPlayRecord.ready);
		battleData.isReplay = true;
		NetSystem.Instance.helper.OnNetStartBattle (curPlayRecord.start);

		//OnRecord(curPlayRecord.Ready.Group, curPlayRecord.Ready.DataList, curPlayRecord.Ready.RandomSeed);
		AddReplayFrame ();

	}

	/// <summary>
	/// 初始化结束添加循环
	/// </summary>
	public void AddReplayFrame()
	{
		if (curPlayRecord == null)
			return;

		/// 一次性把所有包放里面
		for (int i = 0; i < curPlayRecord.frames.Count; ++i)
		{
			SCFrame scf = curPlayRecord.frames[i];
			NetSystem.Instance.helper.OnNetFrame (scf);
		}
	}

	/// <summary>
	/// 播放结束推出循环
	/// return b
	/// </summary>
	public bool PlayRecordOver()
	{
		if (curPlayRecord == null)
			return false;
		curPlayRecord = null;
		Time.timeScale = 1f;
		return true;
	}

	/// <summary>
	/// Update
	/// </summary>
	void UpdateRecord(int frame,float dt)
	{
		#if !SERVER
		if (curPlayRecord != null)
			EventSystem.Instance.FireEvent (EventId.OnBattleReplayFrame, frame, curPlayRecord.frames.Count);
		#endif
	}

	public void NotifyPlayRecordEnd()
	{
		
	}

	/// <summary>
	/// 设置游戏播放速度
	/// </summary>
	/// <param name="value">Value.</param>
	public void SetPlaySpeed(int value)
	{
		BattleSystem.Instance.lockStep.playSpeed = value;
	}

	public void SetPlayToPercent(float percent)
	{
		if (curPlayRecord == null)
			return;


		int frame = (int)(percent * curPlayRecord.frames.Count + 0.5);

		if (frame >= curPlayRecord.frames.Count)
			frame = curPlayRecord.frames.Count - 1;

		//Debug.LogFormat ("to frame...{0}--{1}", frame, curPlayRecord.FramesCount);

		BattleSystem.Instance.lockStep.runFrameCount = 20;
		BattleSystem.Instance.lockStep.RunToFrame (frame);
	}
}


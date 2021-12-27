using System;
using System.Collections;
using System.Collections.Generic;
using Plugin;
using Solarmax;

public interface IBattleController : Lifecycle2
{
	/// <summary>
	/// 游戏结束重置
	/// </summary>
	void Reset ();
	
	/// <summary>
	/// 收到网络包的解析
	/// </summary>
	/// <param name="frame">Frame.</param>
	void OnRecievedFramePacket (NetMessage.SCFrame frame);

	/// <summary>
	/// 执行包
	/// </summary>
	/// <param name="frameNode">Frame node.</param>
	void OnRunFramePacket (FrameNode frameNode);

	/// <summary>
	/// 玩家手指操作
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	/// <param name="percent">Percent.</param>
	void OnPlayerMove (Node from, Node to, BattleTeam battleTea );

	/// <summary>
	/// 玩家放弃
	/// </summary>
	void PlayerGiveUp ();

	/// <summary>
	/// 玩家放弃的回调
	/// </summary>
	void OnPlayerGiveUp (TEAM giveUpTeam);

	/// <summary>
	/// 玩家退出游戏
	/// </summary>
	void OnPlayerDirectQuit (TEAM team);
}

